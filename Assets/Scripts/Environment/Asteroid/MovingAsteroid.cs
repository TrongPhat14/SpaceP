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

        rotateTween = Asteroid
            .DORotate(new Vector3(0f, 0f, 360f), Mathf.Max(0.01f, rotationDuration), RotateMode.FastBeyond360)
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
}
