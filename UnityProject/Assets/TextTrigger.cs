using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTrigger : MonoBehaviour
{
    public GameObject textBox;
    public TextAsset textFile;
    public TextBoxManager manager;
    public Sprite avatar;
    public bool pause;
    string[] lines;
    bool printed;
    // Start is called before the first frame update
    void Awake()
    {
        if (textFile != null)
        {
            lines = textFile.text.Split('\n');
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.CompareTag("Player") && !printed)
        {
            printed = true;
            if (pause)
            {
                PlayerMovement.instance.rb.velocity = new Vector2(0,0);
                PlayerMovement.instance.GetPulled(false);
                PlayerMovement.instance.mx = 0;
                PlayerMovement.instance.my = 0;
                GameMenu.Instance.paused = true;
            }
            manager.PrintText(lines, avatar);
        }
    }
}
