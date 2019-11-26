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
        if(desired != null && desired.Value != Treasure.None)
        {
            BubbleAnimator.SetBool("HasWanted", true);
        }
        if(undesired != null && undesired.Value != Treasure.None)
        {
            BubbleAnimator.SetBool("HasUnwanted", true);
        }

        if(undesired != null && undesired.Value != Treasure.None)
        {
            TreasureImage.sprite = undesired.UISprite;
            CenterDragon.UndesiredInThoughtBubble = undesired;
        }
        else if (desired != null && desired.Value != Treasure.None)
        {
            TreasureImage.sprite = desired.UISprite;
        }

        if ((desired == null || desired.Value == Treasure.None) && (undesired == null || undesired.Value == Treasure.None))
        {
            print("Error! bubble showing, but no new desire or undesire is set!");
        }
    }
    
    public void NotDesireShown()
    {
        CenterDragon.DesiredTreasure3 = null;

        if (desired != null && desired.Value != Treasure.None)
            TreasureImage.sprite = desired.UISprite;

        if(repeatedDesired != null && repeatedDesired.Value != Treasure.None)
        {
            CenterDragon.DesiredTreasure3 = CenterDragon.DesiredTreasure2;
            CenterDragon.DesiredTreasure2 = CenterDragon.DesiredTreasure1;
            CenterDragon.DesiredTreasure1 = repeatedDesired;
        }

        CenterDragon.PlayDesireChangedSound();
        CenterDragon.UndesiredInThoughtBubble = undesired;
    }

    public void DesireShown()
    {
        CenterDragon.DesiredTreasure3 = CenterDragon.DesiredTreasure2;
        CenterDragon.DesiredTreasure2 = CenterDragon.DesiredTreasure1;
        CenterDragon.DesiredTreasure1 = desired;
        CenterDragon.PlayDesireChangedSound();
    }

    public void WasHidden()
    {
        BubbleAnimator.SetBool("HasWanted", false);
        BubbleAnimator.SetBool("HasUnwanted", false);

        CenterDragon.ThoughtEnded();
    }
}
