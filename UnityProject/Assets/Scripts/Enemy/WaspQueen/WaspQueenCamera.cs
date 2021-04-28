using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WaspQueenCamera : MonoBehaviour
{
    public CinemachineVirtualCamera camera;
    public Transform center;
    bool bossCamera;
    int cameraSize = 23;
    CinemachineFramingTransposer ftransposer;
    // Start is called before the first frame update
    void Start()
    {
        ftransposer = camera.GetCinemachineComponent<CinemachineFramingTransposer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(bossCamera && camera.m_Lens.OrthographicSize < cameraSize)
        {
            camera.m_Lens.OrthographicSize += Time.deltaTime*2.5f;
            if (camera.m_Lens.OrthographicSize > cameraSize)
            {
                ftransposer.m_XDamping = 10;
                ftransposer.m_YDamping = 10;
                camera.m_Lens.OrthographicSize = cameraSize;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            bossCamera = true;
            ftransposer.m_UnlimitedSoftZone = true;
            ftransposer.m_DeadZoneHeight = 0;
            ftransposer.m_DeadZoneWidth = 0;
            ftransposer.m_XDamping = 20;
            ftransposer.m_YDamping = 20;
            camera.Follow = center;
        }

    }
}
