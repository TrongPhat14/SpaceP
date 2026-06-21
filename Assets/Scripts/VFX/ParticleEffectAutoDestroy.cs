using UnityEngine;

public class ParticleEffectAutoDestroy : MonoBehaviour
{
    [SerializeField] private float lifetime = 2.5f;

    public void SetLifetime(float value)
    {
        lifetime = Mathf.Max(0.1f, value);
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }
}
