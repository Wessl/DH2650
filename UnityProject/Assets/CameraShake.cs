using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    public CinemachineVirtualCamera playerCamera;
    private float shakeTimer, shakeDuration, startingIntensity;
    // Start is called before the first frame update
    void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            CinemachineBasicMultiChannelPerlin perlin = playerCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            perlin.m_AmplitudeGain = Mathf.Lerp(0, startingIntensity, shakeTimer/shakeDuration);
        }
    }

    public void ShakeCamera(float intensity, float time)
    {
        CinemachineBasicMultiChannelPerlin perlin = playerCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

        startingIntensity = intensity;
        shakeTimer = time;
        shakeDuration = time;
    }
}
