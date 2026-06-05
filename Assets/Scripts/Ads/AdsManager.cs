using System;
using System.Collections;
using UnityEngine;

#if ADMOB_ENABLED
using GoogleMobileAds.Api;
using GoogleMobileAds.Ump.Api;
#endif

public class AdsManager : MonoBehaviour
{
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

#if ADMOB_ENABLED
    private bool mobileAdsInitializationStarted;
    private RewardedAd rewardedAd;
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

#if ADMOB_ENABLED
        MobileAds.RaiseAdEventsOnUnityMainThread = true;
        RequestConsentAndInitializeAds();
#elif UNITY_EDITOR
        isInitialized = true;
        isInitializationInProgress = false;
        SetState(RewardedAdState.Ready, "EDITOR TEST READY");
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

#if ADMOB_ENABLED
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
#elif UNITY_EDITOR
        StartCoroutine(SimulateRewardedAd(onRewardEarned));
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
#if UNITY_EDITOR && !ADMOB_ENABLED
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

#if ADMOB_ENABLED
    private void RequestConsentAndInitializeAds()
    {
        ConsentRequestParameters requestParameters =
            new ConsentRequestParameters
            {
                TagForUnderAgeOfConsent = false
            };

        ConsentInformation.Update(
            requestParameters,
            consentUpdateError =>
            {
                if (consentUpdateError != null)
                {
                    Debug.LogWarning(
                        "UMP consent update failed: " +
                        consentUpdateError.Message
                    );
                }

                TryInitializeMobileAds();

                ConsentForm.LoadAndShowConsentFormIfRequired(
                    consentFormError =>
                    {
                        if (consentFormError != null)
                        {
                            Debug.LogWarning(
                                "UMP consent form failed: " +
                                consentFormError.Message
                            );
                        }

                        TryInitializeMobileAds();

                        if (!mobileAdsInitializationStarted)
                        {
                            isInitializationInProgress = false;
                            SetState(
                                RewardedAdState.Unavailable,
                                "CONSENT REQUIRED"
                            );
                        }
                    }
                );
            }
        );
    }

    private void TryInitializeMobileAds()
    {
        if (mobileAdsInitializationStarted ||
            !ConsentInformation.CanRequestAds())
        {
            return;
        }

        mobileAdsInitializationStarted = true;
        MobileAds.Initialize(_ =>
        {
            isInitialized = true;
            isInitializationInProgress = false;
            LoadRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        StopRetry();
        DisposeRewardedAd();
        SetState(RewardedAdState.Loading, "LOADING...");

        AdRequest request = new AdRequest();
        RewardedAd.Load(
            GetRewardedAdUnitId(),
            request,
            (RewardedAd loadedAd, LoadAdError error) =>
            {
                if (error != null || loadedAd == null)
                {
                    SetState(RewardedAdState.Unavailable, "AD UNAVAILABLE");
                    retryCoroutine = StartCoroutine(RetryLoadAfterDelay());
                    return;
                }

                rewardedAd = loadedAd;
                RegisterRewardedAdEvents(rewardedAd);
                SetState(RewardedAdState.Ready, "FREE COINS");
            }
        );
    }

    private void RegisterRewardedAdEvents(RewardedAd ad)
    {
        ad.OnAdFullScreenContentClosed += LoadRewardedAd;
        ad.OnAdFullScreenContentFailed += _ => LoadRewardedAd();
    }

    private IEnumerator RetryLoadAfterDelay()
    {
        yield return new WaitForSecondsRealtime(15f);
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

#if UNITY_EDITOR && !ADMOB_ENABLED
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

    private void OnDestroy()
    {
        StopRetry();

#if ADMOB_ENABLED
        DisposeRewardedAd();
#endif

        if (instance == this)
        {
            instance = null;
        }
    }
}
