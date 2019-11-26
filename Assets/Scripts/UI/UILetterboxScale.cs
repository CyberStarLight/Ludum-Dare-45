using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILetterboxScale : MonoBehaviour
{
    public RectTransform MainRectTransform;
    public int OriginalResolutionX = 1920;
    public int OriginalResolutionY = 1080;

    private Vector2 originalSize;
    private float originalRatio;

    private void Start()
    {
        originalSize = MainRectTransform.sizeDelta;
        originalRatio = (float)OriginalResolutionX / (float)OriginalResolutionY;
    }

    void Update()
    {
        MainRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        MainRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        MainRectTransform.pivot = new Vector2(0.5f, 0.5f);
        MainRectTransform.anchoredPosition = new Vector2(0f, 0f);
        MainRectTransform.sizeDelta = originalSize;

        float ratio = (float)Screen.width / (float)Screen.height;

        if (ratio <= originalRatio)
        {
            float scale = (float)Screen.width / OriginalResolutionX;

            MainRectTransform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            float scale = (float)Screen.height / OriginalResolutionY;

            MainRectTransform.localScale = new Vector3(scale, scale, scale);
        }
    }
}
