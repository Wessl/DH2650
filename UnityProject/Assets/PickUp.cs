using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUp : MonoBehaviour
{
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
            WaspQueen.instance.temporaryBoost += 25;
            SnackPool.Instance.AddToPool(gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            Combat.instance.UpdateKi(25);
            SnackPool.Instance.AddToPool(gameObject);
        } else if (collision.CompareTag("Nongrappable"))
        {
            SnackPool.Instance.AddToPool(gameObject);
        }
    }
}
