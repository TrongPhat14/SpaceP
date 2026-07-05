using System;
using System.Collections;
using UnityEngine;

#if ADMOB_ENABLED && !UNITY_EDITOR
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
#endif

public class AdsManager : MonoBehaviour
{
    private const float ConsentRetryDelaySeconds = 30f;
    private const float ConsentUpdateTimeoutSeconds = 20f;
    private const float MobileAdsInitializationTimeoutSeconds = 20f;
    private const float RewardedAdLoadTimeoutSeconds = 20f;
    private const float RewardedAdRetryDelaySeconds = 15f;
    private const bool SkipUmpForAdLoadTest = false;

    public enum RewardedAdState
    {
        Initializing,
        Loading,
        Ready,
        Showing,
        Unavailable
    }

    // Google test ad unit IDs. Replace these with your own IDs before release.
    private const string AndroidRewardedAdUnitId =
        "ca-app-pub-3940256099942544/5224354917";
    private const string IosRewardedAdUnitId =
        "ca-app-pub-3940256099942544/1712485313";

    private static AdsManager instance;

    public static AdsManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<AdsManager>();
            }

            if (instance == null)
            {
                GameObject managerObject = new GameObject(nameof(AdsManager));
                instance = managerObject.AddComponent<AdsManager>();
            }

            return instance;
        }
    }

    public event Action<RewardedAdState, string> RewardedStateChanged;

    public RewardedAdState CurrentState { get; private set; } =
        RewardedAdState.Initializing;

    public bool IsRewardedAdReady => CurrentState == RewardedAdState.Ready;

    private bool isInitialized;
    private bool isInitializationInProgress;
    private bool rewardGrantedForCurrentAd;
    private Coroutine retryCoroutine;
    private Coroutine consentUpdateTimeoutCoroutine;
    private Coroutine initializationTimeoutCoroutine;
    private Coroutine rewardedAdLoadTimeoutCoroutine;

#if ADMOB_ENABLED && !UNITY_EDITOR
    private bool mobileAdsInitializationStarted;
    private RewardedAd rewardedAd;
    private int consentRequestId;
    private int rewardedAdLoadRequestId;
#endif

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        if (isInitialized || isInitializationInProgress)
        {
            NotifyCurrentState();
            return;
        }

        isInitializationInProgress = true;
        SetState(RewardedAdState.Initializing, "INITIALIZING...");

#if UNITY_EDITOR
        isInitialized = true;
        isInitializationInProgress = false;
        SetState(RewardedAdState.Ready, "EDITOR TEST READY");
#elif ADMOB_ENABLED
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        RequestConsentAndInitializeAds();
#else
        isInitializationInProgress = false;
        SetState(
            RewardedAdState.Unavailable,
            "ADMOB SDK NOT ENABLED"
        );
#endif
    }

    public void ShowRewardedAd(Action onRewardEarned)
    {
        if (!IsRewardedAdReady)
        {
            NotifyCurrentState();
            return;
        }

        SetState(RewardedAdState.Showing, "PLAYING AD...");

#if UNITY_EDITOR
        StartCoroutine(SimulateRewardedAd(onRewardEarned));
#elif ADMOB_ENABLED
        if (rewardedAd == null || !rewardedAd.CanShowAd())
        {
            LoadRewardedAd();
            return;
        }

        rewardGrantedForCurrentAd = false;
        rewardedAd.Show(_ =>
        {
            if (rewardGrantedForCurrentAd)
            {
                return;
            }

            rewardGrantedForCurrentAd = true;
            onRewardEarned?.Invoke();
        });
#else
        SetState(RewardedAdState.Unavailable, "AD UNAVAILABLE");
#endif
    }

    private void NotifyCurrentState()
    {
        RewardedStateChanged?.Invoke(CurrentState, GetDefaultStatus(CurrentState));
    }

    private void SetState(RewardedAdState state, string status)
    {
        CurrentState = state;
        RewardedStateChanged?.Invoke(state, status);
    }

    private string GetDefaultStatus(RewardedAdState state)
    {
        switch (state)
        {
            case RewardedAdState.Initializing:
                return "INITIALIZING...";
            case RewardedAdState.Loading:
                return "LOADING...";
            case RewardedAdState.Ready:
#if UNITY_EDITOR
                return "EDITOR TEST READY";
#else
                return "FREE COINS";
#endif
            case RewardedAdState.Showing:
                return "PLAYING AD...";
            default:
                return "AD UNAVAILABLE";
        }
    }

#if ADMOB_ENABLED && !UNITY_EDITOR
    private void RequestConsentAndInitializeAds()
    {
        if (SkipUmpForAdLoadTest)
        {
            ReleaseLog.Warning(
                "UMP consent is skipped for local ad loading test. " +
                "Disable SkipUmpForAdLoadTest before release."
            );
            TryInitializeMobileAds(skipConsentCheck: true);
            return;
        }

        SetState(RewardedAdState.Initializing, "CHECKING CONSENT...");
        StopConsentUpdateTimeout();

        int requestId = ++consentRequestId;
        consentUpdateTimeoutCoroutine =
            StartCoroutine(HandleConsentUpdateTimeout(requestId));

        ConsentRequestParameters requestParameters =
            new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };

        ConsentInformation.Update(
            requestParameters,
            consentUpdateError =>
            {
                if (requestId != consentRequestId)
                {
                    return;
                }

                StopConsentUpdateTimeout();

                if (consentUpdateError != null)
                {
                    ReleaseLog.Warning(
                        "UMP consent update failed: " +
                        consentUpdateError.Message
                    );

                    TryInitializeMobileAds(skipConsentCheck: false);

                    if (!mobileAdsInitializationStarted)
                    {
                        isInitializationInProgress = false;
                        SetState(
                            RewardedAdState.Unavailable,
                            "CONSENT CHECK FAILED"
                        );
                        ScheduleConsentRetry();
                    }

                    return;
                }

                ConsentForm.LoadAndShowConsentFormIfRequired(
                    consentFormError =>
                    {
                        if (consentFormError != null)
                        {
                            ReleaseLog.Warning(
                                "UMP consent form failed: " +
                                consentFormError.Message
                            );
                        }

                        TryInitializeMobileAds(skipConsentCheck: false);

                        if (!mobileAdsInitializationStarted)
                        {
                            isInitializationInProgress = false;
                            SetState(
                                RewardedAdState.Unavailable,
                                "CONSENT REQUIRED"
                            );
                            ScheduleConsentRetry();
                        }
                    }
                );
            }
        );
    }

    private IEnumerator HandleConsentUpdateTimeout(int requestId)
    {
        yield return new WaitForSecondsRealtime(ConsentUpdateTimeoutSeconds);

        if (requestId != consentRequestId ||
            CurrentState != RewardedAdState.Initializing)
        {
            yield break;
        }

        consentUpdateTimeoutCoroutine = null;
        isInitializationInProgress = false;
        ReleaseLog.Warning("UMP consent update timed out.");
        SetState(RewardedAdState.Unavailable, "CONSENT TIMEOUT");
        ScheduleConsentRetry();
    }

    private void TryInitializeMobileAds(bool skipConsentCheck)
    {
        if (mobileAdsInitializationStarted ||
            (!skipConsentCheck && !ConsentInformation.CanRequestAds()))
        {
            return;
        }

        mobileAdsInitializationStarted = true;
        SetState(RewardedAdState.Initializing, "INITIALIZING ADS...");
        initializationTimeoutCoroutine =
            StartCoroutine(HandleMobileAdsInitializationTimeout());

        MobileAds.Initialize(_ =>
        {
            StopInitializationTimeout();
            isInitialized = true;
            isInitializationInProgress = false;
            LoadRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        StopRetry();
        StopRewardedAdLoadTimeout();
        DisposeRewardedAd();
        SetState(RewardedAdState.Loading, "LOADING...");

        int requestId = ++rewardedAdLoadRequestId;
        rewardedAdLoadTimeoutCoroutine =
            StartCoroutine(HandleRewardedAdLoadTimeout(requestId));

        AdRequest request = new AdRequest();
        RewardedAd.Load(
            GetRewardedAdUnitId(),
            request,
            (RewardedAd loadedAd, LoadAdError error) =>
            {
                if (requestId != rewardedAdLoadRequestId)
                {
                    if (loadedAd != null)
                    {
                        loadedAd.Destroy();
                    }

                    return;
                }

                StopRewardedAdLoadTimeout();

                if (error != null || loadedAd == null)
                {
                    ReleaseLog.Warning(
                        "Rewarded ad load failed: " +
                        (error != null ? error.ToString() : "Loaded ad is null.")
                    );
                    SetState(RewardedAdState.Unavailable, "AD UNAVAILABLE");
                    ScheduleRewardedAdRetry();
                    return;
                }

                StopRetry();
                rewardedAd = loadedAd;
                RegisterRewardedAdEvents(rewardedAd);
                SetState(RewardedAdState.Ready, "FREE COINS");
            }
        );
    }

    private void RegisterRewardedAdEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += LoadRewardedAd;
        ad.OnAdFullScreenContentFailed += error =>
        {
            ReleaseLog.Warning("Rewarded ad show failed: " + error);
            LoadRewardedAd();
        };
    }

    private IEnumerator HandleMobileAdsInitializationTimeout()
    {
        yield return new WaitForSecondsRealtime(
            MobileAdsInitializationTimeoutSeconds
        );

        initializationTimeoutCoroutine = null;

        if (isInitialized)
        {
            yield break;
        }

        mobileAdsInitializationStarted = false;
        isInitializationInProgress = false;
        SetState(RewardedAdState.Unavailable, "AD INIT TIMEOUT");
        ScheduleConsentRetry();
    }

    private IEnumerator HandleRewardedAdLoadTimeout(int requestId)
    {
        yield return new WaitForSecondsRealtime(RewardedAdLoadTimeoutSeconds);

        if (requestId != rewardedAdLoadRequestId ||
            CurrentState != RewardedAdState.Loading)
        {
            yield break;
        }

        rewardedAdLoadTimeoutCoroutine = null;
        ReleaseLog.Warning("Rewarded ad load timed out.");
        SetState(RewardedAdState.Unavailable, "AD LOAD TIMEOUT");
        ScheduleRewardedAdRetry();
    }

    private void ScheduleConsentRetry()
    {
        StopRetry();
        retryCoroutine = StartCoroutine(RetryConsentAfterDelay());
    }

    private IEnumerator RetryConsentAfterDelay()
    {
        yield return new WaitForSecondsRealtime(ConsentRetryDelaySeconds);
        retryCoroutine = null;
        isInitializationInProgress = true;
        RequestConsentAndInitializeAds();
    }

    private void ScheduleRewardedAdRetry()
    {
        StopRetry();
        retryCoroutine = StartCoroutine(RetryLoadAfterDelay());
    }

    private IEnumerator RetryLoadAfterDelay()
    {
        yield return new WaitForSecondsRealtime(RewardedAdRetryDelaySeconds);
        retryCoroutine = null;
        LoadRewardedAd();
    }

    private string GetRewardedAdUnitId()
    {
#if UNITY_IOS
        return IosRewardedAdUnitId;
#else
        return AndroidRewardedAdUnitId;
#endif
    }

    private void DisposeRewardedAd()
    {
        if (rewardedAd == null)
        {
            return;
        }

        rewardedAd.Destroy();
        rewardedAd = null;
    }
#endif

#if UNITY_EDITOR
    private IEnumerator SimulateRewardedAd(Action onRewardEarned)
    {
        yield return new WaitForSecondsRealtime(0.8f);
        onRewardEarned?.Invoke();
        SetState(RewardedAdState.Loading, "LOADING...");
        yield return new WaitForSecondsRealtime(0.8f);
        SetState(RewardedAdState.Ready, "EDITOR TEST READY");
    }
#endif

    private void StopRetry()
    {
        if (retryCoroutine == null)
        {
            return;
        }

        StopCoroutine(retryCoroutine);
        retryCoroutine = null;
    }

    private void StopInitializationTimeout()
    {
        if (initializationTimeoutCoroutine == null)
        {
            return;
        }

        StopCoroutine(initializationTimeoutCoroutine);
        initializationTimeoutCoroutine = null;
    }

    private void StopConsentUpdateTimeout()
    {
        if (consentUpdateTimeoutCoroutine == null)
        {
            return;
        }

        StopCoroutine(consentUpdateTimeoutCoroutine);
        consentUpdateTimeoutCoroutine = null;
    }

    private void StopRewardedAdLoadTimeout()
    {
        if (rewardedAdLoadTimeoutCoroutine == null)
        {
            return;
        }

        StopCoroutine(rewardedAdLoadTimeoutCoroutine);
        rewardedAdLoadTimeoutCoroutine = null;
    }

    private void OnDestroy()
    {
        StopRetry();
        StopConsentUpdateTimeout();
        StopInitializationTimeout();
        StopRewardedAdLoadTimeout();

#if ADMOB_ENABLED && !UNITY_EDITOR
        DisposeRewardedAd();
#endif

        if (instance == this)
        {
            instance = null;
        }
    }
}
