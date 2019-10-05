using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Dragon : MonoBehaviour
{
    public int GoldCoins;
    public int MaxGoldCoins;
    public float GoldRatio { get { return (float)GoldCoins / (float)MaxGoldCoins; } }

    public float Rage;
    public float MaxRage;
    public float RageRatio { get { return Rage / MaxRage; } }

    public float Greed;
    public float MaxGreed;
    public float GreedRatio { get { return Greed / MaxGreed; } }

    public float DesireIntervalMin = 1f;
    public float DesireIntervalMax = 5f;

    public int GoldCoinsPerTreasue = 1000;
    public int RagePerUnwantedTreasue = 10;
    public Treasure DesiredTreasure1;
    public Treasure DesiredTreasure2;
    public Treasure DesiredTreasure3;

    [Header("References")]
    public Transform FireballOrigin;
    public CircleCollider2D FireballArea;
    public Fireball FireballPrefab;
    public GameObject[] GoldPiles;
    public float ThoughtBubleFadeInDuration;
    public float ThoughtBubleStayDuration;
    public float ThoughtBubleFadeOutDuration;
    public SpriteRenderer ThoughtBubble;
    public SpriteRenderer ThoughtBubbleXMark;
    public SpriteRenderer ThoughtBubbleTreasure;

    [Header("Treasure Sprites")]
    public Sprite Treasue01;
    public Sprite Treasue02;
    public Sprite Treasue03;
    public Sprite Treasue04;
    public Sprite Treasue05;

    //State variables
    private float nextDesireChangeTime = 0f;

    void Update()
    {
        //Shoot fire ball when right clicking on followers within range
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if(FireballArea.OverlapPoint(mousePos))
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, 0f, LayerMask.GetMask("Follower"));

                if (hit)
                {
                    var target = hit.transform;

                    //Flip dragon in the right direction
                    transform.localScale = mousePos.x < 0 ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f);

                    var newFireball = Instantiate(FireballPrefab, FireballOrigin.position, Quaternion.identity, null);
                    newFireball.Target = target;
                    newFireball.LastKnownPosition = target.position;
                }
            }
        }

        //Update gold piles
        int pileCount = Mathf.FloorToInt(GoldRatio * 10f);
        for (int i = 0; i < 10; i++)
        {
            GoldPiles[i].SetActive(i < pileCount);
        }

        //Update desires
        if(Time.time >= nextDesireChangeTime)
        {
            GenerateNewDesire();
            nextDesireChangeTime = Time.time + Random.Range(DesireIntervalMin, DesireIntervalMax);
        }

        //Check victory / defeat
        if (RageRatio > 0.999f)
        {
            //Game over
            SceneManager.LoadScene("GameOver");
        }

        if(GoldRatio > 0.999f)
        {
            //Victory
            SceneManager.LoadScene("Victory");
        }

    }

    public void GiveTreasure(Treasure t)
    {
        if(
            DesiredTreasure1 == t ||
            DesiredTreasure2 == t ||
            DesiredTreasure3 == t
            )
        {
            GoldCoins += GoldCoinsPerTreasue;
        }
        else
        {
            Rage += RagePerUnwantedTreasue;
        }
    }

    public void GenerateNewDesire()
    {
        //Push new desire to queue
        var oldDesire = DesiredTreasure3;

        DesiredTreasure3 = DesiredTreasure2;
        DesiredTreasure2 = DesiredTreasure1;
        DesiredTreasure1 = GameBoard.GetRandomTreasure();

        //check if last removed desire is no longer wanted
        Treasure unwantedTreasure = Treasure.None;
        if(
            oldDesire != Treasure.None &&
            oldDesire != DesiredTreasure1 &&
            oldDesire != DesiredTreasure2 &&
            oldDesire != DesiredTreasure3
            )
        {
            unwantedTreasure = oldDesire;
        }

        Treasure wantedTresure = Treasure.None;
        if(
            DesiredTreasure1 != DesiredTreasure2 && 
            DesiredTreasure1 != DesiredTreasure3 &&
            DesiredTreasure1 != oldDesire
            )
        {
            wantedTresure = DesiredTreasure1;
        }

        //Display thought bubble for the new desire or new undesired treasure (or both)
        if (_ThoughtBubbleCorutine == null && (unwantedTreasure != Treasure.None || wantedTresure != Treasure.None))
        {
            _ThoughtBubbleCorutine = ThoughtBubbleCorutine(wantedTresure, unwantedTreasure);
            StartCoroutine(_ThoughtBubbleCorutine);
        }
    }

    public Sprite getSpriteForTreasure(Treasure t)
    {
        switch (t)
        {
            case Treasure.Gold:
                return Treasue01;
            case Treasure.Diamond:
                return Treasue02;
            case Treasure.Emerald:
                return Treasue03;
            case Treasure.None:
            default:
                return null;
        }
    }
    
    IEnumerator _ThoughtBubbleCorutine;
    IEnumerator ThoughtBubbleCorutine(Treasure t, Treasure undesired)
    {
        float alphaPerSec;

        if (undesired != Treasure.None)
        {
            //Show a thought buble for unwanted treasure first
            ThoughtBubble.gameObject.SetActive(true);
            ThoughtBubbleXMark.gameObject.SetActive(true);
            ThoughtBubbleTreasure.sprite = getSpriteForTreasure(undesired);

            ThoughtBubble.color = new Color(1f, 1f, 1f, 0f);
            ThoughtBubbleTreasure.color = new Color(1f, 1f, 1f, 0f);

            alphaPerSec = (1f / ThoughtBubleFadeInDuration) * 3f;
            while (ThoughtBubble.color.a < 1f)
            {
                ThoughtBubble.color += new Color(0f, 0f, 0f, alphaPerSec * Time.deltaTime);
                ThoughtBubbleXMark.color = ThoughtBubbleTreasure.color = ThoughtBubble.color;
                yield return null;
            }

            yield return new WaitForSeconds(ThoughtBubleStayDuration / 3f);

            alphaPerSec = (1f / ThoughtBubleFadeOutDuration) * 3f;
            while (ThoughtBubble.color.a > 0f)
            {
                ThoughtBubble.color -= new Color(0f, 0f, 0f, alphaPerSec * Time.deltaTime);
                ThoughtBubbleXMark.color = ThoughtBubbleTreasure.color = ThoughtBubble.color;
                yield return null;
            }

            ThoughtBubble.gameObject.SetActive(false);
            ThoughtBubbleXMark.gameObject.SetActive(false);
        }
        
        if(t != Treasure.None)
        {
            ThoughtBubble.gameObject.SetActive(true);
            ThoughtBubbleTreasure.sprite = getSpriteForTreasure(t);

            ThoughtBubble.color = new Color(1f, 1f, 1f, 0f);
            ThoughtBubbleTreasure.color = new Color(1f, 1f, 1f, 0f);

            alphaPerSec = 1f / ThoughtBubleFadeInDuration;
            while (ThoughtBubble.color.a < 1f)
            {
                ThoughtBubble.color += new Color(0f, 0f, 0f, alphaPerSec * Time.deltaTime);
                ThoughtBubbleTreasure.color = ThoughtBubble.color;
                yield return null;
            }

            yield return new WaitForSeconds(ThoughtBubleStayDuration);

            alphaPerSec = 1f / ThoughtBubleFadeOutDuration;
            while (ThoughtBubble.color.a > 0f)
            {
                ThoughtBubble.color -= new Color(0f, 0f, 0f, alphaPerSec * Time.deltaTime);
                ThoughtBubbleTreasure.color = ThoughtBubble.color;
                yield return null;
            }

            ThoughtBubble.gameObject.SetActive(false);
        }

        _ThoughtBubbleCorutine = null;
    }
}
