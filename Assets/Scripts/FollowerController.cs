using UnityEngine;
using System.Collections.Generic;

public class FollowerController : RoadWalker
{
    private Tresure tresureHeld;
    [SerializeField] SpriteRenderer tresureSprite;
    private Dictionary<Tresure, Sprite> spritesByTresureTypes;
    [SerializeField] Sprite[] tresureSprites;
    private void initSpritesByTresureTypes()
    {
        for (int i = 0; i < tresureSprites.Length; i++)
        {
            spritesByTresureTypes.Add((Tresure)i + 1, tresureSprites[i]);
        }
    }
    public Tresure TresureHeld
    {
        get => tresureHeld;
        set
        {
            tresureHeld = value;
            tresureSprite.sprite = spritesByTresureTypes[value];
        }
    }
}