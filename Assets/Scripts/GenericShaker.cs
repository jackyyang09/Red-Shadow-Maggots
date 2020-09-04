
using UnityEngine;
using System.Collections;

public class GenericShaker : MonoBehaviour
{
    // How long the object should shake for.
    [SerializeField]
    float shakeDuration;
    float shakeTimer;

    // Amplitude of the shake. A larger value shakes the camera harder.
    [SerializeField]
    float shakeAmount = 0.7f;

    public float shakeIntensity { get { return shakeAmount; } set { shakeAmount = value; } }

    public float shakeTime { get { return shakeDuration; } set { shakeDuration = value; } }

    Vector3 originalPos;

    void OnEnable()
    {
        originalPos = transform.localPosition;
        shakeTimer = shakeDuration;
    }

    private void OnDisable()
    {
        transform.localPosition = originalPos;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            transform.localPosition = originalPos + Random.insideUnitSphere * Mathf.Lerp(0, shakeAmount, shakeTimer / shakeDuration);
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            enabled = false;
        }
    }
}