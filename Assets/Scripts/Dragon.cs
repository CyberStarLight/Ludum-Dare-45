using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Dragon : MonoBehaviour
{
    [Header("Testing")]
    public int StartingGold;

    private int _goldCoins;
    public int GoldCoins
    {
        get { return _goldCoins; }
        set { _goldCoins = Mathf.Clamp(value, 0, MaxGoldCoins); }
    }

    [Header("Config")]
    public int MaxGoldCoins;
    public float GoldRatio { get { return (float)GoldCoins / (float)MaxGoldCoins; } }

    private float _rage;
    public float Rage
    {
        get { return _rage; }
        set { _rage = Mathf.Clamp(value, 0f, MaxRage); }
    }
    public float MaxRage;
    public float RageRatio { get { return Rage / MaxRage; } }

    private float _panic;
    public float Panic
    {
        get { return _panic; }
        set { _panic = Mathf.Clamp(value, 0f, MaxPanic); }
    }
    public float MaxPanic;
    public float PanicRatio { get { return Panic / MaxPanic; } }

    public float DesireIntervalMin = 1f;
    public float DesireIntervalMax = 5f;

    public float PanicDownPerSec = 0.5f;
    public int GoldCoinsPerTreasue = 1000;
    public int RagePerUnwantedTreasue = 10;
    public int RagePerTrash = 20;
    public TreasureInfo DesiredTreasure1;
    public TreasureInfo DesiredTreasure2;
    public TreasureInfo DesiredTreasure3;

    [Header("References")]
    public GameBoard MainGameBoard;
    public Transform FireballOrigin;
    public CircleCollider2D FireballArea;
    public Fireball FireballPrefab;
    public GameObject[] GoldPiles;
    public Animator DragonAnimator;
    public float ThoughtBubleFadeInDuration;
    public float ThoughtBubleStayDuration;
    public float ThoughtBubleFadeOutDuration;
    public SpriteRenderer ThoughtBubble;
    public SpriteRenderer ThoughtBubbleXMark;
    public SpriteRenderer ThoughtBubbleXMark2;
    public SpriteRenderer ThoughtBubbleTreasure;


    [Header("Audio")]
    public AudioSource SoundEffectsSource;
    public AudioClip PositiveTreasureSound;
    public AudioClip NegativeTreasureSound;
    public AudioClip FireballSound;

    //State variables
    private float nextDesireChangeTime = 0f;
    private Transform _fireballTarget;
    private bool fireballInProgress;

    private void Start()
    {
        GoldCoins = StartingGold;
        DesiredTreasure1 = DesiredTreasure2 = DesiredTreasure3 = null;
    }

    void Update()
    {
        //Shoot fire ball when right clicking on followers within range
        if (Input.GetKeyDown(KeyCode.Mouse0) && !fireballInProgress)
        {
            var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if(FireballArea.OverlapPoint(mousePos))
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(mousePos.x, mousePos.y), Vector2.zero, 0f, LayerMask.GetMask("Follower", "Mine"));

                if (hit)
                {
                    var target = hit.transform;

                    if(target.gameObject.tag == "Follower")
                        target.GetComponent<FollowerController>().OnTargeted();
                    else if(target.gameObject.tag == "Mine")
                        target.GetComponent<MineController>().OnTargeted();


                    //Flip dragon in the right direction
                    transform.localScale = mousePos.x < 0 ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f);

                    _fireballTarget = target;
                    fireballInProgress = true;
                    DragonAnimator.Play("Dragon Fireball", 0);
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

        //Update panic
        Panic -= PanicDownPerSec * Time.deltaTime;

        //Check victory / defeat
        if (RageRatio > 0.999f)
        {
            //Game over
            SceneManager.LoadScene("GameOver");
        }

        if (GoldRatio > 0.999f)
        {
            //Victory
            SceneManager.LoadScene("Victory");
        }

    }

    public void GiveTreasure(TreasureInfo t)
    {
        if(
            !t.IsTrash &&
            (
            (DesiredTreasure1 != null && DesiredTreasure1.Value == t.Value) ||
            (DesiredTreasure2 != null && DesiredTreasure2.Value == t.Value) || 
            (DesiredTreasure3 != null && DesiredTreasure3.Value == t.Value)
            )
            )
        {
            GoldCoins += GoldCoinsPerTreasue;
            PlayPositiveTreasure();
        }
        else
        {
            Rage += (t.IsTrash ? RagePerTrash : RagePerUnwantedTreasue);
            PlayNegativeTreasure();
        }
    }

    public void GenerateNewDesire()
    {
        //Push new desire to queue
        var oldDesire = DesiredTreasure3;
        var newDesire = MainGameBoard.GetRandomTreasure();

        DesiredTreasure3 = DesiredTreasure2;
        DesiredTreasure2 = DesiredTreasure1;
        DesiredTreasure1 = newDesire;

        //check if last removed desire is no longer wanted
        TreasureInfo unwantedTreasure = null;
        if(
            oldDesire != null &&
            oldDesire.Value != DesiredTreasure1.Value &&
            oldDesire.Value != DesiredTreasure2.Value &&
            oldDesire.Value != DesiredTreasure3.Value
            )
        {
            unwantedTreasure = oldDesire;
        }

        TreasureInfo wantedTresure = null;
        if(
            (DesiredTreasure2 == null || DesiredTreasure1.Value != DesiredTreasure2.Value) && 
            (DesiredTreasure3 == null || DesiredTreasure1.Value != DesiredTreasure3.Value) &&
            (oldDesire == null || DesiredTreasure1.Value != oldDesire.Value)
            )
        {
            wantedTresure = DesiredTreasure1;
        }

        //Display thought bubble for the new desire or new undesired treasure (or both)
        if (_ThoughtBubbleCorutine == null && (unwantedTreasure != null || wantedTresure != null))
        {
            _ThoughtBubbleCorutine = ThoughtBubbleCorutine(wantedTresure, unwantedTreasure);
            StartCoroutine(_ThoughtBubbleCorutine);
        }
    }

    //public Sprite getSpriteForTreasure(Treasure t)
    //{
    //    switch (t)
    //    {
    //        case Treasure.Gem_Green:
    //            break;
    //        case Treasure.Gem_Red:
    //            break;
    //        case Treasure.Gem_Purple:
    //            break;
    //        case Treasure.Metal_Gold:
    //            break;
    //        case Treasure.Metal_Silver:
    //            break;
    //    }

    //    return null;
    //}
    
    IEnumerator _ThoughtBubbleCorutine;
    IEnumerator ThoughtBubbleCorutine(TreasureInfo desired, TreasureInfo undesired)
    {
        float alphaPerSec;

        if (undesired != null)
        {
            //Show a thought buble for unwanted treasure first
            ThoughtBubble.gameObject.SetActive(true);
            ThoughtBubbleXMark.gameObject.SetActive(true);
            ThoughtBubbleXMark2.gameObject.SetActive(true);
            ThoughtBubbleTreasure.sprite = undesired.UISprite;

            ThoughtBubble.color = new Color(1f, 1f, 1f, 0f);
            ThoughtBubbleTreasure.color = new Color(1f, 1f, 1f, 0f);

            alphaPerSec = (1f / ThoughtBubleFadeInDuration) * 3f;
            while (ThoughtBubble.color.a < 1f)
            {
                ThoughtBubble.color += new Color(0f, 0f, 0f, alphaPerSec * Time.deltaTime);
                ThoughtBubbleXMark.color = ThoughtBubbleXMark2.color = ThoughtBubbleTreasure.color = ThoughtBubble.color;
                yield return null;
            }

            yield return new WaitForSeconds(ThoughtBubleStayDuration / 3f);

            alphaPerSec = (1f / ThoughtBubleFadeOutDuration) * 3f;
            while (ThoughtBubble.color.a > 0f)
            {
                ThoughtBubble.color -= new Color(0f, 0f, 0f, alphaPerSec * Time.deltaTime);
                ThoughtBubbleXMark.color = ThoughtBubbleXMark2.color = ThoughtBubbleTreasure.color = ThoughtBubble.color;
                yield return null;
            }

            ThoughtBubble.gameObject.SetActive(false);
            ThoughtBubbleXMark.gameObject.SetActive(false);
            ThoughtBubbleXMark2.gameObject.SetActive(false);
        }
        
        if(desired != null)
        {
            ThoughtBubble.gameObject.SetActive(true);
            ThoughtBubbleTreasure.sprite = desired.UISprite;

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

    //Animation
    private void actuallyShootFireball()
    {
        var newFireball = Instantiate(FireballPrefab, FireballOrigin.position, Quaternion.identity, null);
        newFireball.Target = _fireballTarget;
        newFireball.LastKnownPosition = _fireballTarget.position;

        PlayFireballSound();
    }

    private void fireballAnimationFinished()
    {
        fireballInProgress = false;
    }

    //Sounds
    public void PlayPositiveTreasure()
    {
        SoundEffectsSource.PlayOneShot(PositiveTreasureSound, 3f);
    }

    public void PlayNegativeTreasure()
    {
        SoundEffectsSource.PlayOneShot(NegativeTreasureSound, 5f);
    }

    public void PlayFireballSound()
    {
        SoundEffectsSource.PlayOneShot(FireballSound, 3f);
    }
}
