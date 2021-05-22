using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    [SerializeField]
    private float activeTime = 0.1f;
    private float timeActivated;

    private Transform player;

    private SpriteRenderer SR;
    private SpriteRenderer playerSR;

    public Sprite slash;

    void OnEnable()
    {
        SR = GetComponent<SpriteRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerSR = player.GetComponent<SpriteRenderer>();

        //alpha = alphaSet;
        SR.sprite = playerSR.sprite;
        transform.position = player.position;
        timeActivated = Time.time;
    }

    public void PulledInit(float alpha, float life, Vector2 pos)
    {
        activeTime = life;
        transform.position = pos;
        SR.sprite = slash;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        alpha -= alphaDecay * Time.deltaTime;
        color = new Color(1, 1, 1, alpha);
        SR.color = color;
        */
        if(Time.time >= (timeActivated + activeTime))
        {
            AfterImagePool.Instance.AddToPool(gameObject);
        }
    }
}
