using UnityEngine;
using System.Collections.Generic;

public class FollowerController : RoadWalker
{
    public bool IsTargeted { get; private set; }
    public bool IsSpeciaKill { get; private set; }

    public float PanicAmountOnDeath = 2.5f;
    public float MaxPanicSpeedBonus = 2f;

    [HideInInspector]
    public Dragon Master;
    public ParticleSystem Dust;
    public Animator MainAnimator;

    //private Treasure treasureHeld;
    public SpriteRenderer TreasureRenderer;
    public SpriteRenderer TreasureRenderer2;
    [SerializeField] SpriteRenderer deathMarkRenderer;
    [SerializeField] Collider2D MainCollider;
    [SerializeField] Collider2D TouchScreenCollider;
    private static Dictionary<Treasure, Sprite> spritesByTresureTypes;
    [SerializeField] Sprite[] tresureSprites;

    public TreasureInfo TreasureHeld;

    private void Start()
    {
        Master = FindObjectOfType<Dragon>();
    }
    public void OnKilled()
    {
        if (IsSpeciaKill)
            return;

        Master.TreasureDestroyed(TreasureHeld);
        Master.PlayMoleDeath();
    }
    
    public void OnTargeted(bool isSpecialKill = false)
    {
        IsTargeted = true;
        IsSpeciaKill = isSpecialKill;

        //show death mark
        deathMarkRenderer.gameObject.SetActive(true);

        //push sprites to the back
        MainRenderer.sortingOrder -= 10;
        TreasureRenderer.sortingOrder -= 10;
        if(TreasureRenderer2 != null)
            TreasureRenderer2.sortingOrder -= 10;
        deathMarkRenderer.sortingOrder -= 10;

        //disable collider
        MainCollider.enabled = false;
        TouchScreenCollider.enabled = false;
    }

    public void SetTouchScreenCollider()
    {
        MainCollider.enabled = false;
        TouchScreenCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Dragon")
        {
            bool isDoubleTreasure = TreasureRenderer2 != null;
            Master.GiveTreasure(TreasureHeld, isDoubleTreasure);
            Destroy(gameObject);
        }
    }

    public override void OnDestroy()
    {
        currentRoad.UnregisterWalker(this);
        base.OnDestroy();
    }
}