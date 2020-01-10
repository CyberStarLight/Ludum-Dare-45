using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TreasureInfo
{
    public string Name;
    public Treasure Value;
    public Sprite Sprite;
    public Sprite UISprite;
    public bool IsTrash;

    public TreasureInfo GetCopy()
    {
        return new TreasureInfo() { 
            IsTrash = IsTrash,
            Name = Name,
            Sprite = Sprite,
            Value = Value,
            UISprite = UISprite,
        };
    }
}