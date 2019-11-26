using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LevelConfig
{
    public LevelConfigOverride LevelOverride = new LevelConfigOverride();

    [SerializeField]
    private float _scorePerSecond = 5;
    public float ScorePerSecond { get { return LevelOverride.OverrideScorePerSecond ? LevelOverride.ScorePerSecond : _scorePerSecond; } }

    [Header("Followers")]
    [SerializeField]
    private float _followerSpeedMultiplier = 1f;
    public float FollowerSpeedMultiplier { get { return LevelOverride.OverrideFollowerSpeedMultiplier ? LevelOverride.FollowerSpeedMultiplier : _followerSpeedMultiplier; } }

    [SerializeField]
    private float _followerSpawnDelay_Min = 2.5f;
    public float FollowerSpawnDelay_Min { get { return LevelOverride.OverrideFollowerSpawnDelay_Min ? LevelOverride.FollowerSpawnDelay_Min : _followerSpawnDelay_Min; } }

    [SerializeField]
    private float _followerSpawnDelay_Max = 3.5f;
    public float FollowerSpawnDelay_Max { get { return LevelOverride.OverrideFollowerSpawnDelay_Max ? LevelOverride.FollowerSpawnDelay_Max : _followerSpawnDelay_Max; } }

    [SerializeField]
    [Range(0, 1)]
    private float _followerTrashRatio = 0.22f;
    public float FollowerTrashRatio { get { return LevelOverride.OverrideFollowerTrashRatio ? LevelOverride.FollowerTrashRatio : _followerTrashRatio; } }

    [Header("Gold and Mines")]
    [SerializeField]
    private int _mineCost = 25000;
    public int MineCost { get { return LevelOverride.OverrideMineCost ? LevelOverride.MineCost : _mineCost; } }

    [SerializeField]
    private int _gold_Min = 0;
    public int Gold_Min { get { return LevelOverride.OverrideGold_Min ? LevelOverride.Gold_Min : _gold_Min; } }

    [SerializeField]
    private int _gold_Max = 1000000;
    public int Gold_Max { get { return LevelOverride.OverrideGold_Max ? LevelOverride.Gold_Max : _gold_Max; } }

    [SerializeField]
    private int _gold_RequiredMineCount = 9;
    public int Gold_RequiredMineCount { get { return LevelOverride.OverrideGold_RequiredMineCount ? LevelOverride.Gold_RequiredMineCount : _gold_RequiredMineCount; } }

    public int Mine_CapBonus { get { return Gold_Max / (1 + Gold_RequiredMineCount); } }

    [Header("Treasure")]
    [SerializeField]
    private int _treasure_GoldBonus = 10000;
    public int Treasure_GoldBonus { get { return LevelOverride.OverrideTreasure_GoldBonus ? LevelOverride.Treasure_GoldBonus : _treasure_GoldBonus; } }

    [SerializeField]
    private int _treasure_RagePenalty = 4;
    public int Treasure_RagePenalty { get { return LevelOverride.OverrideTreasure_RagePenalty ? LevelOverride.Treasure_RagePenalty : _treasure_RagePenalty; } }

    [SerializeField]
    private int _treasure_TrashRagePenalty = 6;
    public int Treasure_TrashRagePenalty { get { return LevelOverride.OverrideTreasure_TrashRagePenalty ? LevelOverride.Treasure_TrashRagePenalty : _treasure_TrashRagePenalty; } }

    [Header("Dragon")]
    [SerializeField]
    private float _dragon_DesireIntervalMin = 9f;
    public float Dragon_DesireIntervalMin { get { return LevelOverride.OverrideDragon_DesireIntervalMin ? LevelOverride.Dragon_DesireIntervalMin : _dragon_DesireIntervalMin; } }

    [SerializeField]
    private float _dragon_DesireIntervalMax = 13f;
    public float Dragon_DesireIntervalMax { get { return LevelOverride.OverrideDragon_DesireIntervalMax ? LevelOverride.Dragon_DesireIntervalMax : _dragon_DesireIntervalMax; } }
}

[Serializable]
public class LevelConfigOverride
{
    public bool OverrideMineCost;
    public int MineCost = 0;

    public bool OverrideScorePerSecond;
    public float ScorePerSecond = 0;

    [Header("Followers")]
    public bool OverrideFollowerSpeedMultiplier;
    public float FollowerSpeedMultiplier = 0f;
    public bool OverrideFollowerSpawnDelay_Min;
    public float FollowerSpawnDelay_Min = 0f;
    public bool OverrideFollowerSpawnDelay_Max;
    public float FollowerSpawnDelay_Max = 0f;
    public bool OverrideFollowerTrashRatio;
    [Range(0, 1)] public float FollowerTrashRatio = 0f;

    [Header("Gold and Mines")]
    public bool OverrideGold_Min;
    public int Gold_Min = 0;
    public bool OverrideGold_Max;
    public int Gold_Max = 0;
    public bool OverrideGold_RequiredMineCount;
    public int Gold_RequiredMineCount = 0;

    [Header("Treasure")]
    public bool OverrideTreasure_GoldBonus;
    public int Treasure_GoldBonus = 0;
    public bool OverrideTreasure_RagePenalty;
    public int Treasure_RagePenalty = 0;
    public bool OverrideTreasure_TrashRagePenalty;
    public int Treasure_TrashRagePenalty = 0;

    [Header("Dragon")]
    public bool OverrideDragon_DesireIntervalMin;
    public float Dragon_DesireIntervalMin = 0;
    public bool OverrideDragon_DesireIntervalMax;
    public float Dragon_DesireIntervalMax = 0;
}