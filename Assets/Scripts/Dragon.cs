using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Dragon : MonoBehaviour
{
    private float _rage;
    public float Rage
    {
        get { return _rage; }
        set { _rage = Mathf.Clamp(value, 0f, GameSettings.Instance.Dragon_MaxRage); }
    }
    public float RageRatio { get { return Rage / GameSettings.Instance.Dragon_MaxRage; } }

    public float PanicEditorDisplay; //for testing
    private float _panic;
    public float Panic
    {
        get { return _panic; }
        set { _panic = Mathf.Clamp(value, MinPanic, GameSettings.Instance.Dragon_MaxPanic); }
    }
    public float MinPanic { get { return GameSettings.Instance.Dragon_MaxPanic * (RageRatio * 0.5f); } }
    public float PanicRatio { get { return Panic / GameSettings.Instance.Dragon_MaxPanic; } }

    public TreasureInfo DesiredTreasure1;
    public TreasureInfo DesiredTreasure2;
    public TreasureInfo DesiredTreasure3;

    [HideInInspector]
    public TreasureInfo UndesiredInThoughtBubble;

    [Header("References")]
    public GameBoard MainGameBoard;
    public Transform FireballOrigin;
    public Transform ClearScreenOrigin;
    public Animator BubbleAnimator;
    public Fireball FireballPrefab;
    public GameObject[] GoldPiles;
    public Animator DragonAnimator;
    public float ThoughtBubleFadeInDuration;
    public float ThoughtBubleStayDuration;
    public float ThoughtBubleFadeOutDuration;
    public SpriteRenderer ThoughtBubble;
    public SpriteRenderer ThoughtBubbleXMark;
    public SpriteRenderer ThoughtBubbleTreasure;
    public ThoughtBubble ThoughtBubbleAnimation;
    public Animator DragonHuff;
    public Transform[] ClearScreenDummyTargets;

    //State variables
    private float nextDesireChangeTime = 0f;
    private Transform _fireballTarget;
    private bool fireballInProgress;
    private bool thoughtInProgress;

    private void Start()
    {
        DesiredTreasure1 = DesiredTreasure2 = DesiredTreasure3 = null;
    }

    void Update()
    {
        if (MainGameBoard.HasGameEnded)
            return;

        if(Input.GetKeyDown(KeyCode.F))
        {
            ClearScreenAttack();
        }

        bool hasClick;
        if(MainGameBoard.IsTouchScreen())
            hasClick = MainGameBoard.CurrentPointerPosition.HasValue;
        else
            hasClick = Input.GetKeyDown(KeyCode.Mouse0);

        //Shoot fire ball when right clicking on followers within range
        if (
            hasClick && 
            !MainGameBoard.IsFireballDisabled &&
            !MainGameBoard.IsBuildingAMine &&
            !fireballInProgress
            )
        {
            //var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //var mousePos = MainGameBoard.CurrentPointerPosition;
            var pointerPosition = MainGameBoard.CurrentPointerPosition;

            if (pointerPosition.HasValue)
            {
                RaycastHit2D hit = Physics2D.Raycast(new Vector2(pointerPosition.Value.x, pointerPosition.Value.y), Vector2.zero, 0f, LayerMask.GetMask("Follower"));
                
                if (hit)
                {
                    var target = hit.transform;

                    if(target.gameObject.tag == "Follower")
                        target.GetComponent<FollowerController>().OnTargeted();
                    
                    //Flip dragon in the right direction
                    transform.localScale = pointerPosition.Value.x < transform.position.x ? new Vector3(-1f, 1f, 1f) : new Vector3(1f, 1f, 1f);

                    _fireballTarget = target;
                    fireballInProgress = true;
                    
                    DragonAnimator.SetTrigger("StartFireball");
                }
            }
        }

        //Update gold piles
        int pileCount = Mathf.FloorToInt(MainGameBoard.GoldRatio * 10f);
        for (int i = 0; i < 10; i++)
        {
            GoldPiles[i].SetActive(i < pileCount);
        }

        //Update desires
        if(!MainGameBoard.DesiresDisabeld && !MainGameBoard.ProgressPause && Time.time >= nextDesireChangeTime && !thoughtInProgress)
        {
            GenerateNewDesire();
            nextDesireChangeTime = Time.time + Random.Range(GameSettings.LevelConfig.Dragon_DesireIntervalMin, GameSettings.LevelConfig.Dragon_DesireIntervalMax);
        }

        //Update panic
        if(Panic < MainGameBoard.CapRatio * GameSettings.Instance.Dragon_MaxPanic)
        {
            Panic += 0.4f * Time.deltaTime;
        }
        //Panic = CapRatio * MaxPanic;
        PanicEditorDisplay = Panic;
        //Panic -= PanicDownPerSec * Time.deltaTime;

        //Check victory / defeat
        if (RageRatio > 0.999f)
        {
            //Game over
            MainGameBoard.GameOver();
        }
        else if (MainGameBoard.GoldRatio > 0.999f)
        {
            //Victory
            MainGameBoard.Victory();
        }

    }

    public void GiveTreasure(TreasureInfo t, bool isDouble)
    {
        int desireLevel = GetDesireLevel(t);

        if (!t.IsTrash && desireLevel > 0)
        {
            MainGameBoard.GoldCoins += GameSettings.LevelConfig.Treasure_GoldBonus * desireLevel * (isDouble ? 2 : 1);
            PlayPositiveTreasure();
            MainGameBoard.TreasureCollected();
        }
        else
        {
            Rage += (t.IsTrash ? GameSettings.LevelConfig.Treasure_TrashRagePenalty * (isDouble ? 2 : 1) : GameSettings.LevelConfig.Treasure_RagePenalty * (isDouble ? 2 : 1));
            PlayNegativeTreasure();
            DragonHuff.SetTrigger("Show");

            if(t.IsTrash)
                MainGameBoard.CollectedTrash();
            else
                MainGameBoard.WrongTreasureCollected();
        }
    }

    public void TreasureDestroyed(TreasureInfo t)
    {
        int desireLevel = GetDesireLevel(t);

        if (!t.IsTrash && desireLevel > 0 && t != UndesiredInThoughtBubble)
        {
            PlayNegativeTreasure();
            Rage += GameSettings.LevelConfig.Treasure_RagePenalty / 2f;
            MainGameBoard.WrongTreasureDestroyed();
        }
    }
    
    public void DesireChanged()
    {
        PlayDesireChangedSound();
    }

    public int GetDesireLevel(TreasureInfo t)
    {
        int desire = 0;

        if (DesiredTreasure1 != null && DesiredTreasure1.Value == t.Value)
            desire++;

        if (DesiredTreasure2 != null && DesiredTreasure2.Value == t.Value)
            desire++;

        if (DesiredTreasure3 != null && DesiredTreasure3.Value == t.Value)
            desire++;

        return desire;
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
        TreasureInfo repeatedDesire = null;
        if(
            (DesiredTreasure2 == null || DesiredTreasure1.Value != DesiredTreasure2.Value) && 
            (DesiredTreasure3 == null || DesiredTreasure1.Value != DesiredTreasure3.Value) &&
            (oldDesire == null || DesiredTreasure1.Value != oldDesire.Value)
            )
        {
            wantedTresure = DesiredTreasure1;
        }
        else if (
            newDesire != null && newDesire.Value != Treasure.None &&
            (newDesire == DesiredTreasure2 || newDesire == DesiredTreasure3)
            )
        {
            //desire for an existing treasure
            repeatedDesire = newDesire;
        }


        //Display thought bubble for the new desire or new undesired treasure (or both)
        if (_ThoughtBubbleCorutine == null && (unwantedTreasure != null || wantedTresure != null))
        {
            thoughtInProgress = true;

            ThoughtBubbleAnimation.Show(wantedTresure, unwantedTreasure, repeatedDesire);
            DesiredTreasure1 = DesiredTreasure2;
            DesiredTreasure2 = DesiredTreasure3;
            DesiredTreasure3 = oldDesire;
        }
    }
    
    public void ClearScreenAttack()
    {
        if (MainGameBoard.ExistingFollowers.Count(x => x != null) == 0)
            return;

        PlayFireballSound();

        foreach (var follower in MainGameBoard.ExistingFollowers.Where(x => !x.IsTargeted))
        {
            var newFireball = Instantiate(FireballPrefab, FireballOrigin.position, Quaternion.identity, null);
            newFireball.Target = follower.transform;
            newFireball.LastKnownPosition = follower.transform.position;
            follower.OnTargeted(true);
            newFireball.SetInvisible();
            newFireball.Speed *= 0.85f;
        }

        int numberOfObjects = 16;
        float angleOffset = 360f / numberOfObjects;
        float radius = 10;
        for (int i = 0; i < numberOfObjects; i++)
        {
            Vector3 pos = transform.position + (Quaternion.AngleAxis(i * angleOffset, Vector3.forward) * (Vector3.right * radius));
            var newFireball = Instantiate(FireballPrefab, ClearScreenOrigin.position, Quaternion.identity, null);
            //newFireball.MainSpriteRenderer.sortingOrder = 32767;
            newFireball.Target = null;
            newFireball.LastKnownPosition = pos;
            newFireball.transform.localScale *= 1.5f;
            newFireball.Speed *= 0.85f;
        }

        MainGameBoard.NoClearBeforeTime = MainGameBoard.NoSpawnBeforeTime = Time.time + 3f;
    }

    //Corutine
    IEnumerator _ThoughtBubbleCorutine;
    IEnumerator ThoughtBubbleCorutine(TreasureInfo desired, TreasureInfo undesired)
    {
        float alphaPerSec;

        if (undesired != null)
        {
            UndesiredInThoughtBubble = undesired;

            //Show a thought buble for unwanted treasure first
            ThoughtBubble.gameObject.SetActive(true);
            ThoughtBubbleXMark.gameObject.SetActive(true);
            ThoughtBubbleTreasure.sprite = undesired.UISprite;

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

            UndesiredInThoughtBubble = null;
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
        if (_fireballTarget == null)
            return;

        var newFireball = Instantiate(FireballPrefab, FireballOrigin.position, Quaternion.identity, null);
        newFireball.Target = _fireballTarget;
        newFireball.LastKnownPosition = _fireballTarget.position;

        PlayFireballSound();
    }

    private void fireballAnimationFinished()
    {
        fireballInProgress = false;
        _fireballTarget = null;
    }

    public void ThoughtEnded()
    {
        thoughtInProgress = false;
    }

    //Sounds
    public void PlayPositiveTreasure()
    {
        FMODManager.Play(Sounds.PositiveTreasure);
    }

    public void PlayNegativeTreasure()
    {
        FMODManager.Play(Sounds.NegativeTreasure);
    }

    public void PlayNegativeBuild()
    {
        FMODManager.Play(Sounds.BuildFailed);
    }

    public void PlayFireballSound()
    {
        FMODManager.Play(Sounds.FireballShoot);
    }

    public void PlayFireballHitSound()
    {
        FMODManager.Play(Sounds.FireballExplode);
    }

    public void PlayDesireChangedSound()
    {
        FMODManager.Play(Sounds.DragonChoiceChange);
    }

    public void PlayMoleDeath()
    {
        PlayFireballHitSound();

        Invoke("PlayDelayedMoleDeath", 0.25f);
    }

    private void PlayDelayedMoleDeath()
    {
        FMODManager.Play(Sounds.MoleDeath);
    }
}
