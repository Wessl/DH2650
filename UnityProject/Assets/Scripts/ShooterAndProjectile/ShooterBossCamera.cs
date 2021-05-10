using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class ShooterBossCamera : MonoBehaviour
{
    public CinemachineVirtualCamera cameraCV;
    public Transform center;
    bool bossCamera;
    int cameraSize = 18;
    CinemachineFramingTransposer ftransposer;
    // Start is called before the first frame update
    void Start()
    {
        ftransposer = cameraCV.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(bossCamera && cameraCV.m_Lens.OrthographicSize < cameraSize)
        {
            cameraCV.m_Lens.OrthographicSize += Time.deltaTime*2.5f;
            if (cameraCV.m_Lens.OrthographicSize > cameraSize)
            {
                ftransposer.m_XDamping = 10;
                ftransposer.m_YDamping = 10;
                cameraCV.m_Lens.OrthographicSize = cameraSize;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !bossCamera)
        {
            bossCamera = true;
            ftransposer.m_UnlimitedSoftZone = true;
            ftransposer.m_DeadZoneHeight = 0;
            ftransposer.m_DeadZoneWidth = 0;
            ftransposer.m_XDamping = 10;
            ftransposer.m_YDamping = 10;
            cameraCV.Follow = center;
        }

    }
}
