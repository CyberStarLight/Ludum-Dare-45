using UnityEngine;
using System.Collections.Generic;

public class FollowerController : RoadWalker
{
    public float PanicAmountOnDeath = 2.5f;
    public float MaxPanicSpeedBonus = 2f;

    [HideInInspector]
    public Dragon Master;
    public ParticleSystem Dust;
    public Animator MainAnimator;

    //private Treasure treasureHeld;
    public SpriteRenderer TreasureRenderer;
    [SerializeField] SpriteRenderer deathMarkRenderer;
    [SerializeField] Collider2D MainCollider;
    private static Dictionary<Treasure, Sprite> spritesByTresureTypes;
    [SerializeField] Sprite[] tresureSprites;
    //private void initSpritesByTresureTypes()
    //{
    //    spritesByTresureTypes = new Dictionary<Treasure, Sprite>();
    //    for (int i = 0; i < tresureSprites.Length; i++)
    //    {
    //        spritesByTresureTypes.Add((Treasure)i + 1, tresureSprites[i]);
    //    }
    //}
    public TreasureInfo TreasureHeld;
    //{
    //    get => treasureHeld;
    //    set
    //    {
    //        if (spritesByTresureTypes == null) initSpritesByTresureTypes();
    //        treasureHeld = value;
    //        treasureSprite.sprite = spritesByTresureTypes[value];
    //    }
    //}
    private void Start()
    {
        Master = FindObjectOfType<Dragon>();
    }
    public void Panic()
    {
        Master.Panic += PanicAmountOnDeath;
    }

    public void OnTargeted()
    {
        //show death mark
        deathMarkRenderer.gameObject.SetActive(true);

        //push sprites to the back
        MainRenderer.sortingOrder -= 10;
        TreasureRenderer.sortingOrder -= 10;
        deathMarkRenderer.sortingOrder -= 10;

        //disable collider
        MainCollider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Dragon")
        {
            Master.GiveTreasure(TreasureHeld);
            Destroy(gameObject);
        }
    }

    public override void OnDestroy()
    {
        currentRoad.UnregisterWalker(this);
        base.OnDestroy();
    }
}