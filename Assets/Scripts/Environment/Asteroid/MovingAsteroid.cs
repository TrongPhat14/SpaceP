using DG.Tweening;
using UnityEngine;

public class MovingAsteroid : MonoBehaviour
{

    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform Asteroid;
    [SerializeField] private float duration = 2f;
    [SerializeField] private float restartDelay = 0.15f;
    [SerializeField] private float rotationDuration = 3f;
    [SerializeField] private bool randomizeRotation = true;
    [SerializeField] private Vector2 randomRotationDurationRange = new Vector2(2.2f, 5f);

    private Sequence moveSequence;
    private Tween rotateTween;

    private void Start()
    {
        if (pointA == null || pointB == null || Asteroid == null)
        {
            return;
        }

        Asteroid.position = pointA.position;

        moveSequence = DOTween.Sequence()
            .SetLink(Asteroid.gameObject)
            .Append(Asteroid.DOMove(pointB.position, Mathf.Max(0.01f, duration)).SetEase(Ease.Linear))
            .AppendInterval(Mathf.Max(0f, restartDelay))
            .AppendCallback(() => Asteroid.position = pointA.position)
            .SetLoops(-1, LoopType.Restart);

        float startAngle = randomizeRotation ? Random.Range(0f, 360f) : Asteroid.eulerAngles.z;
        float spinDirection = randomizeRotation && Random.value < 0.5f ? -1f : 1f;
        float spinDuration = GetRotationDuration();

        Asteroid.rotation = Quaternion.Euler(0f, 0f, startAngle);

        rotateTween = Asteroid
            .DORotate(new Vector3(0f, 0f, 360f * spinDirection), spinDuration, RotateMode.FastBeyond360)
            .SetRelative()
            .SetLink(Asteroid.gameObject)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart);
    }

    private void OnDestroy()
    {
        if (Asteroid != null)
        {
            Asteroid.DOKill();
        }

        moveSequence?.Kill();
        rotateTween?.Kill();
    }

    private float GetRotationDuration()
    {
        if (!randomizeRotation)
        {
            return Mathf.Max(0.01f, rotationDuration);
        }

        float minDuration = Mathf.Min(randomRotationDurationRange.x, randomRotationDurationRange.y);
        float maxDuration = Mathf.Max(randomRotationDurationRange.x, randomRotationDurationRange.y);
        return Mathf.Max(0.01f, Random.Range(minDuration, maxDuration));
    }

    private void OnValidate()
    {
        duration = Mathf.Max(0.01f, duration);
        restartDelay = Mathf.Max(0f, restartDelay);
        rotationDuration = Mathf.Max(0.01f, rotationDuration);
        randomRotationDurationRange.x = Mathf.Max(0.01f, randomRotationDurationRange.x);
        randomRotationDurationRange.y = Mathf.Max(0.01f, randomRotationDurationRange.y);
    }
}
