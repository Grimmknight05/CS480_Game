using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    private Vector3 originalPosition;
    private Coroutine shakeRoutine;

    private bool isShaking;
    private float currentMagnitude;

    private void Awake()
    {
        originalPosition = transform.localPosition;
    }

    public void StartShake(float magnitude, float duration = -1f)
    {
        currentMagnitude = magnitude;

        if (!isShaking)
        {
            isShaking = true;
            shakeRoutine = StartCoroutine(ShakeLoop());
        }

        if (duration > 0f)
        {
            StartCoroutine(StopAfterDelay(duration));
        }
    }

    public void StopShake()
    {
        if (!isShaking) return;

        isShaking = false;

        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }

    private IEnumerator ShakeLoop()
    {
        while (isShaking)
        {
            float x = Random.Range(-1f, 1f) * currentMagnitude;
            float y = Random.Range(-1f, 1f) * currentMagnitude;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            yield return null;
        }
    }
    public void Shake(CamShakeConfig config)
    {
        if (config == null)
        {
            Debug.LogWarning("CameraShake: Missing config!");
            return;
        }

        if (config.useCurve)
        {
            StartShakeWithCurve(config);
        }
        else
        {
            StartShake(config.magnitude, config.duration);
        }
    }
    private IEnumerator ShakeWithCurveRoutine(CamShakeConfig config)
    {
        float elapsed = 0f;

        while (elapsed < config.duration)
        {
            float t = elapsed / config.duration;
            float strength = config.intensityOverTime.Evaluate(t) * config.magnitude;

            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = originalPosition + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPosition;
        shakeRoutine = null;
    }

    private void StartShakeWithCurve(CamShakeConfig config)
    {
        StopShake();

        isShaking = true;
        shakeRoutine = StartCoroutine(ShakeWithCurveRoutine(config));
    }
    private IEnumerator StopAfterDelay(float duration)
    {
        yield return new WaitForSeconds(duration);
        StopShake();
    }

    public void StartShakeEvent()
    {
        StartShake(0.25f);
    }

    public void StartShakeWithDuration(float duration)
    {
        StartShake(0.25f, duration);
    }

    public void StopShakeEvent()
    {
        StopShake();
    }
}