using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxManager : MonoBehaviour
{
    public GameObject textBox;
    public Text text;
    public Text test;
    public TextAsset textFile;
    public string printText;
    public string[] lines;
    public string printLine;

    public int currentLine;
    public int lastLine;
    public float delay;
    public float timer;
    private int charIndex;
    private bool invisibleCars;
    private float switchTimer;

    // Start is called before the first frame update
    void Start()
    {
        currentLine = 0;
        if (textFile != null)
        {
            lines = textFile.text.Split('\n');
        }
        lastLine = lines.Length;
        PrintText(test, lines[currentLine++]);
    }

    // Call this function when you want to print something, seperate each message with a new line
    public void PrintText(Text text, string printText)
    {
        this.text = text;
        this.printText = printText;
        charIndex = 0;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))       // Return either skips to the end of current message or fetches the next message
        {
            if (charIndex < printText.Length)
                charIndex = printText.Length;
            else if (currentLine<lastLine)
            {
                switchTimer = 0;
                PrintText(test, lines[currentLine++]);
            } else
            {
                textBox.SetActive(false);
            }
        }
        Write();
    }

    void Write()
    {
        if (text == null)
        {
            if (switchTimer > 0)
            {
                switchTimer -= Time.deltaTime;
            } else
            {
                if (currentLine >= lastLine)
                {
                    textBox.SetActive(false);
                } else 
                    PrintText(test, lines[currentLine++]);
            }
            return;
        }
        timer -= Time.deltaTime;

        while (timer <= 0)
        {
            timer += delay;
            string txt = printText.Substring(0, charIndex);
            txt += "<color=#00000000>" + printText.Substring(charIndex) + "</color>";
            charIndex++;
            text.text = txt;

            if (charIndex > printText.Length)
            {
                print(charIndex);
                text = null;
                switchTimer = 3;
            }
        }
    }
}
