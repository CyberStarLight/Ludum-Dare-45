using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThoughtBubble : MonoBehaviour
{
    public Dragon CenterDragon;
    public Animator BubbleAnimator;
    public Image TreasureImage;

    private TreasureInfo desired;
    private TreasureInfo undesired;
    private TreasureInfo repeatedDesired;

    public void Show(TreasureInfo _desired, TreasureInfo _undesired, TreasureInfo repeatedDesire)
    {
        desired = _desired;
        undesired = _undesired;
        repeatedDesired = repeatedDesire;

        BubbleAnimator.SetTrigger("Spawn");
    }

    public void BubbleSpawned()
    {
        BubbleAnimator.SetBool("HasWanted", desired != null && desired.Value != Treasure.None);
        BubbleAnimator.SetBool("HasUnwanted", undesired != null && undesired.Value != Treasure.None);

        if(undesired != null)
        {
            TreasureImage.sprite = undesired.UISprite;
        }
        else if (desired != null)
        {
            TreasureImage.sprite = desired.UISprite;
        }
    }
    
    public void NotDesireShown()
    {
        CenterDragon.DesiredTreasure3 = null;

        if (desired != null)
            TreasureImage.sprite = desired.UISprite;

        if(repeatedDesired != null && repeatedDesired.Value != Treasure.None)
        {
            CenterDragon.DesiredTreasure3 = CenterDragon.DesiredTreasure2;
            CenterDragon.DesiredTreasure2 = CenterDragon.DesiredTreasure1;
            CenterDragon.DesiredTreasure1 = repeatedDesired;
        }
    }

    public void DesireShown()
    {
        CenterDragon.DesiredTreasure3 = CenterDragon.DesiredTreasure2;
        CenterDragon.DesiredTreasure2 = CenterDragon.DesiredTreasure1;
        CenterDragon.DesiredTreasure1 = desired;
    }

    public void WasHidden()
    {
        BubbleAnimator.SetBool("HasWanted", false);
        BubbleAnimator.SetBool("HasUnwanted", false);

        CenterDragon.ThoughtEnded();
    }


    
}
