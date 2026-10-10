using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class AutoReturnFX : MonoBehaviour
{
    [HideInInspector] public float duration = 2f;
    private float _startTime;
    private ParticleSystem _ps;

    private void Awake()
    {
        _ps = GetComponent<ParticleSystem>();
    }

    private void OnEnable()
    {
        _startTime = Time.time;

        if (_ps != null)
            duration = _ps.main.duration + _ps.main.startLifetime.constantMax;
    }

    private void Update()
    {
        if (Time.time - _startTime >= duration)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        if (ProjectilePoolManager.Instance != null && _ps != null)
        {
            _ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            ProjectilePoolManager.Instance.ReturnImpactFX(_ps);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}

