using UnityEngine;
using System.Collections.Generic;

public class FollowerController : RoadWalker
{
    public Dragon Master;

    private Treasure treasureHeld;
    [SerializeField] SpriteRenderer tresureSprite;
    private static Dictionary<Treasure, Sprite> spritesByTresureTypes;
    [SerializeField] Sprite[] tresureSprites;
    private void initSpritesByTresureTypes()
    {
        spritesByTresureTypes = new Dictionary<Treasure, Sprite>();
        for (int i = 0; i < tresureSprites.Length; i++)
        {
            spritesByTresureTypes.Add((Treasure)i + 1, tresureSprites[i]);
        }
    }
    public Treasure TresureHeld
    {
        get => treasureHeld;
        set
        {
            if (spritesByTresureTypes == null) initSpritesByTresureTypes();
            treasureHeld = value;
            tresureSprite.sprite = spritesByTresureTypes[value];
            Debug.Log(value);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Dragon")
        {
            Master.GiveTreasure(treasureHeld);
            Destroy(gameObject);
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}