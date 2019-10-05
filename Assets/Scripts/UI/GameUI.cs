using System.Collections;
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
    }
}
