using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeController : MonoBehaviour
{
    public static TimeController Instance;
    public float slowdownFactor = 0.05f;
    public float slowdownLength = 5;
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
                Combat.instance.UpdateKi(-25);
                StartCoroutine(FlashLine());
            }
            else
            {
                Combat.instance.ResetLine();
            }
        }
    }

    public void StopSlowdown(bool attacked)
    {
        bulletSlashing = attacked;
        if (attacked)
            slowdownTimer = -50;
        else
            slowdownTimer = 0;
    }

    IEnumerator FlashLine()
    {
        Combat.instance.line.SetColors(Color.white, Color.white);
        Combat.instance.line.SetWidth(2, 2);
        yield return new WaitForSeconds(0.2f);
        Combat.instance.ResetLine();
        bulletSlashing = false;
    }
    public void SlowdownTime()
    {
        Combat.instance.UpdateKi(-25);
        Time.timeScale = slowdownFactor;
        Time.fixedDeltaTime = slowdownFactor * .02f;
        slowdownTimer = slowdownLength;
        slowedTime = true;
    }
}
