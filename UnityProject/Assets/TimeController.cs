using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController Instance;
    public float slowdownFactor = 0.05f;
    public float slowdownLength = 3;
    public float slowdownTimer;
    public bool slowedTime, bulletSlashing;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (slowdownTimer > 0)
        {
            slowdownTimer -= Time.unscaledDeltaTime;
        }
        else if (slowedTime)
        {
            slowedTime = false;
            Time.timeScale = 1;
            Time.fixedDeltaTime = .02f;
            if (slowdownTimer == -50)
            {
                Combat.instance.ReduceKi(100);
                StartCoroutine(FlashLine());
            }
            else
            {
                PlayerMovement.instance.ResetLine();
                Combat.instance.ReduceKi(50);
            }
        }
    }

    public void StopSlowdown()
    {
        bulletSlashing = true;
        slowdownTimer = -50;
    }

    IEnumerator FlashLine()
    {
        PlayerMovement.instance.line.SetColors(Color.white, Color.white);
        PlayerMovement.instance.line.SetWidth(2, 2);
        print(PlayerMovement.instance.line.GetPosition(0));
        print(PlayerMovement.instance.line.GetPosition(1));
        yield return new WaitForSeconds(0.2f);
        PlayerMovement.instance.ResetLine();
        bulletSlashing = false;
    }
    public void SlowdownTime()
    {
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = slowdownFactor * .02f;
        slowdownTimer = slowdownLength;
        slowedTime = true;
    }
}
