using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarScript : MonoBehaviour
{
    public RectTransform rect;
    private float width;
    // Start is called before the first frame update
    void Start()
    {
        width = rect.sizeDelta.x;
    }

    public void UpdateBar(float currentPercent)
    {
        currentPercent = Mathf.Round(currentPercent * 50) / 50;
        rect.localPosition = new Vector3(-width * (1-currentPercent), 0, 0);
    }
}
