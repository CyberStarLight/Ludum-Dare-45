using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : SmartBeheviourSingleton<GameSettings>
{
    [Header("Default Level Settings")]
    public LevelConfig DefaultLevelConfig;
    public static LevelConfig LevelConfig { get { return Instance.DefaultLevelConfig; } }

    [Header("Treasures")]
    public TreasureInfo[] DefaultTreasures;
    public static TreasureInfo[] Treasures { get { return Instance.DefaultTreasures; } }

    [Header("Dragon")]
    public float Dragon_MaxRage = 100f;
    public float Dragon_MaxPanic = 100f;

    [Header("Other Settings")]
    public float MinRoadDistanceForMines = 0.6f;

    [Header("Item Settings")]
    public float Item_FastScore_FollowerSpeedMultiplier = 1.5f;
    public float Item_FastScore_ScoreMultiplier = 5f;
    public float Item_FastScore_Duration = 15f;

    public float Item_DoubleTreasure_Duration = 30f;

    [Header("Prefabs")]
    public MineController MinePrefab;
    public FollowerController FollowerPrefab;
    public FollowerController FollowerDoublePrefab;

    public override void Awake()
    {
        base.Awake();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override GameSettings GetSingletonInstance()
    {
        return this;
    }

    protected override bool IsDestroyedOnLoad()
    {
        return false;
    }

    protected override bool OverridePreviousSingletons()
    {
        return false;
    }
}
