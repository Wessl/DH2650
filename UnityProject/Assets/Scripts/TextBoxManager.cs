using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxManager : MonoBehaviour
{
    public GameObject textBox;
    public Text text;
    public TextAsset textFile;
    public string printText;
    public string[] lines;
    public string printLine;

    public int currentLine;
    public int lastLine;
    public float delay;
    public float timer;
    private int charIndex;
    private bool lineFinished;
    private float switchTimer;
    public Image avatar;

    // Start is called before the first frame update
    void Start()
    {
        textBox.SetActive(false);
    }

    // Call this function when you want to print something, seperate each message with a new line
    public void PrintText(string[] lines, Sprite avatar)
    {
        textBox.SetActive(true);
        currentLine = 0;
        this.lines = lines;
        this.avatar.sprite = avatar;
        lastLine = lines.Length;
        NewLine(lines[currentLine++]);
    }
   
    public void NewLine(string printText)
    {
        lineFinished = false;
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
                NewLine(lines[currentLine++]);
            } else
            {
                ClearTextBox();
            }
        }
        Write();
    }

    void Write()
    {
        if (!textBox.active)
            return;
        if (lineFinished)
        {
            if (switchTimer > 0)
            {
                switchTimer -= Time.deltaTime;
            } else
            {
                if (currentLine >= lastLine)
                {
                    ClearTextBox();
                } else 
                    NewLine(lines[currentLine++]);
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
                lineFinished = true;
                switchTimer = 2;
            }
        }
    }

    void ClearTextBox()
    {
        text.text = "";
        this.avatar.sprite = null;
        textBox.SetActive(false);
    }
}
