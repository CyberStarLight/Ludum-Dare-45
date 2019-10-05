﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    public const string GOLD_COUNTER_PREFIX = "Gold: ";

    [Header("UI References")]
    public Image GreedBar;
    public Image RageBar;
    public TextMeshPro GoldText;
    public Image Desire01;
    public Image Desire02;
    public Image Desire03;

    public Dragon Dragon;

    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        GreedBar.fillAmount = Dragon.GreedRatio;
        RageBar.fillAmount = Dragon.RageRatio;
        GoldText.text = GOLD_COUNTER_PREFIX + Dragon.GoldCoins.ToString("N0");

        //Update desires
        Desire01.sprite = Dragon.getSpriteForTreasure(Dragon.DesiredTreasure1);
        Desire02.sprite = Dragon.getSpriteForTreasure(Dragon.DesiredTreasure2);
        Desire03.sprite = Dragon.getSpriteForTreasure(Dragon.DesiredTreasure3);
    }
}
