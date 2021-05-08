using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteShift : MonoBehaviour
{
    public Animator animator;
    public SpriteRenderer SR;
    // Start is called before the first frame update
    void Start()
    {
        SR.sortingLayerID = SortingLayer.NameToID("Player");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool("Inside", true);
            SR.sortingLayerID = SortingLayer.NameToID("Background"); ;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            animator.SetBool("Inside", false);
            SR.sortingLayerID = SortingLayer.NameToID("Player");
        }
    }
}
