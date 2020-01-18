using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FMODManager : BaseFMODManager<FMODManager, Sounds, Music>
{
    public override void Awake()
    {
        base.Awake();
    }

    protected override FMODManager GetSingletonInstance()
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

#if UNITY_EDITOR
    public override void OnValidate()
    {
        base.OnValidate();
    }
#endif

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}

public enum Sounds
{
    DragonChoiceChange,
    MoleDeath,
    FireballShoot,
    FireballExplode,
    LoseFanfare,
    WinFanfare,
    PositiveTreasure,
    BuildFailed,
    NegativeTreasure,
    MinePlaced,
    Item_ClearScreen,
    Item_DoubleTreasure,
    Item_SpeedUp,

    //UI
    ButtonClick,
}

public enum Music
{
    MenuMusic,
    GameMusicIntro,
    GameMusic,
    GameMusicFast,
    FastScoreMusic,
    PauseMusic,
}