using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
    public float boost;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("WaspQueen"))
        {
            WaspQueen.instance.temporaryBoost += boost;
            SnackPool.Instance.AddToPool(gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            Combat.instance.UpdateKi(boost);
            SnackPool.Instance.AddToPool(gameObject);
        } else if (collision.CompareTag("Nongrappable"))
        {
            SnackPool.Instance.AddToPool(gameObject);
        }
    }
}
