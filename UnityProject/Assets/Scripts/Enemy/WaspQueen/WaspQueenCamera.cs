using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class WaspQueenCamera : MonoBehaviour
{
    public static WaspQueenCamera Instance;
    public CinemachineVirtualCamera cameraCV;
    public Transform center;
    bool bossCamera, bossDefeated;
    int cameraSize = 23;
    CinemachineFramingTransposer ftransposer;
    float OGxDamp, OGyDamp, OGdeadzoneH, OGdeadzoneW, OGlensSize;
    private Transform OGfollow;
    // Start is called before the first frame update
    void Start()
    {
        Instance = this;
        OGfollow = cameraCV.Follow;
        ftransposer = cameraCV.GetCinemachineComponent<CinemachineFramingTransposer>();
        OGxDamp = ftransposer.m_XDamping;
        OGyDamp = ftransposer.m_YDamping;
        OGdeadzoneH = ftransposer.m_DeadZoneHeight;
        OGdeadzoneW = ftransposer.m_DeadZoneWidth;
        OGlensSize = cameraCV.m_Lens.OrthographicSize;
    }

    // Update is called once per frame
    void Update()
    {
        if(bossCamera && cameraCV.m_Lens.OrthographicSize < cameraSize && !bossDefeated)
        {
            cameraCV.m_Lens.OrthographicSize += Time.deltaTime*2.5f;
            if (cameraCV.m_Lens.OrthographicSize > cameraSize)
            {
                ftransposer.m_XDamping = 10;
                ftransposer.m_YDamping = 10;
                cameraCV.m_Lens.OrthographicSize = cameraSize;
            }
        } else if (bossDefeated)
        {
            if (cameraCV.m_Lens.OrthographicSize > OGlensSize) {
                cameraCV.m_Lens.OrthographicSize -= Time.deltaTime * 2.5f;
                if (cameraCV.m_Lens.OrthographicSize < OGlensSize)
                {
                    ftransposer.m_UnlimitedSoftZone = false;
                    ftransposer.m_DeadZoneHeight = OGdeadzoneH;
                    ftransposer.m_DeadZoneWidth = OGdeadzoneW;
                    ftransposer.m_XDamping = OGxDamp;
                    ftransposer.m_YDamping = OGyDamp;
                    cameraCV.m_Lens.OrthographicSize = OGlensSize;
                }
            } else
            {
                Destroy(gameObject);
            }
        }

    }

    public void ResetCamera()
    {
        bossDefeated = true;
        cameraCV.Follow = OGfollow;
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
            WaspQueen.instance.gameObject.SetActive(true);
        }

    }
}
