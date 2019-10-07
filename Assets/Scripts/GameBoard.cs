using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameBoard : MonoBehaviour
{
    public float MinRoadDistanceForMines = 1f;
    public Road[] Roads;
    public FollowerController followerPrefab;
    [SerializeField] GameObject[] placableObjects;
    public Dragon CenterDragon;
    public Image MineButtonIcon;
    public int MineCost = 10000;
    public int MineCapBonus = 100000;

    [Header("Treasures")]
    public TreasureInfo[] Treasures;
    [Header("FollowerSpanner")]
    [SerializeField] float spawnRate;
    [SerializeField] [Range(0, 1)] float trashRatio;
    [SerializeField] float distanceFromDragon;

    public bool HasGameEnded { get; set; }

    [Header("References")]
    public AudioSource MusicAudioSource;
    public AudioSource SoundEffectsSource;
    public AudioClip MusicIntro;
    public AudioClip Music;
    public AudioClip GameOverFanfare;
    public AudioClip VictoryFanfare;
    public SpriteRenderer Crosshair;
    public SpriteRenderer MineCursor;

    [HideInInspector]
    public bool IsBuildingAMine;

    [HideInInspector]
    public int MineCount = 0;

    private void Start()
    {
        //Reset brush
        brush.content = 2;
        setBrushContentState(0);
        StartCoroutine(randomFollowersSpawnCycle());

        MusicAudioSource.clip = MusicIntro;
        MusicAudioSource.Play();

        Invoke("PlayNormalMusic", MusicIntro.length - 0.01f);
    }

    private void Update()
    {
        var mousPos = Camera.main.ScreenToWorldPoint(Input.mousePosition); ;
        bool isMouseOverUI = EventSystem.current.IsPointerOverGameObject();
        
        if(!HasGameEnded && !isMouseOverUI)
        {
            Cursor.visible = false;

            if (IsBuildingAMine)
            {
                MineCursor.gameObject.SetActive(true);
                Crosshair.gameObject.SetActive(false);
                MineCursor.transform.position = (Vector2)mousPos;
            }
            else
            {
                Crosshair.gameObject.SetActive(true);
                MineCursor.gameObject.SetActive(false);
                Crosshair.transform.position = (Vector2)mousPos;
            }
        }
        else
        {
            Cursor.visible = true;
            Crosshair.gameObject.SetActive(false);
            MineCursor.gameObject.SetActive(false);
        }

        if (
            Input.GetKeyDown(KeyCode.Mouse0) &&
            !isMouseOverUI &&
            IsBuildingAMine
            )
        {
            UpdateSelf();
        }

        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            IsBuildingAMine = false;
        }

        //Cheats XD
        if(Input.GetKeyDown(KeyCode.G))
        {
            CenterDragon.GoldCoins += 10000;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            CenterDragon.Rage += CenterDragon.MaxRage * 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            CenterDragon.Panic += CenterDragon.MaxPanic * 0.1f;
        }
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    public void UpdateSelf()
    {
        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        switch (brush.content)
        {
            case 0: break;
            //case 1: spawnFollower(mousePos); break;
            case 2: spawnMine(mousePos); break;

            default: break;
        }
    }
    public void spawnFollower(Vector2 mousePos, TreasureInfo tresureHeld)
    {
        var newFollower = Instantiate(followerPrefab, mousePos, Quaternion.identity, null);
        var searchResult = GetClosestPoint(newFollower.transform.position);
        newFollower.currentRoad = searchResult.ParentRoad;
        searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        newFollower.TreasureHeld = tresureHeld;
        newFollower.TreasureRenderer.sprite = tresureHeld.Sprite;
        newFollower.Master = CenterDragon;
    }
    public void spawnMine(Vector2 mousePos)
    {
        bool ableToPlace = true;
        foreach (var G in FindObjectsOfType<MineController>())
            if (((Vector2)G.transform.position - mousePos).magnitude < .9f)
                ableToPlace = false;
        //if (((Vector2)GetClosestPoint(mousePos).PointPosition - mousePos).magnitude < 0.5f)
        //    ableToPlace = false;

        //Check the player has enough gold to pay
        if (CenterDragon.GoldCoins < MineCost)
        {
            ableToPlace = false;
            CenterDragon.PlayNegativeBuild();
        }

        //Check that the mine is not too close to a road
        var offset = new Vector2(0f, -0.2f);
        if(GetClosestPoint(mousePos + offset).Distance < MinRoadDistanceForMines)
        {
            ableToPlace = false;
            CenterDragon.PlayNegativeBuild();
        }

        if (ableToPlace)
        {
            var newMine = Instantiate(placableObjects[0], mousePos, Quaternion.identity, null).GetComponent<MineController>();
            newMine.ore = brush.treasureState;
            newMine.isRandom = true;

            CenterDragon.GoldCoins -= MineCost;

            Invoke("StopBuildingMine", 0.1f);
        }
    }

    public Road.PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPointOnEachRoad = Roads.Select(x => x.GetClosestPoint(position));
        var closestRoadPoint = closestPointOnEachRoad.OrderBy(x => x.Distance).FirstOrDefault();

        return closestRoadPoint;
    }

    public void setBrushContent(int value)
    {
        brush.content = value;
    }

    public void setBrushContentState(int value)
    {
        var goodTreasure = Treasures.Where(x => !x.IsTrash).ToArray();

        brush.contentState = Mathf.Clamp(value, 0, goodTreasure.Length - 1);
        brush.treasureState = goodTreasure[value];

        MineButtonIcon.sprite = brush.treasureState.UISprite;
    }

    public void SetBrushToTreasure(TreasureInfo t)
    {
        brush.contentState = -1;
        brush.treasureState = t;
        MineButtonIcon.sprite = t.UISprite;
    }

    public void ToggleBrushState()
    {
        var goodTreasure = Treasures.Where(x => !x.IsTrash).ToArray();

        brush.contentState += 1;
        if (brush.contentState >= goodTreasure.Length)
            brush.contentState = 0;

        brush.treasureState = goodTreasure[brush.contentState];
        MineButtonIcon.sprite = brush.treasureState.UISprite;
    }

    private static class brush
    {
        public static int content;
        public static int contentState;
        public static TreasureInfo treasureState;
        static Vector2 position;
    }

    public TreasureInfo GetRandomTreasure()
    {
        var goodTreasure = Treasures.Where(x => !x.IsTrash).ToArray();

        return goodTreasure[Random.Range(0, goodTreasure.Length)];
    }

    public TreasureInfo GetRandomTrash()
    {
        var trashTreasure = Treasures.Where(x => x.IsTrash).ToArray();

        return trashTreasure[Random.Range(0, trashTreasure.Length)];
    }
    
    private Vector2 randomVector()
    {
        return new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
    }

    private void StopBuildingMine()
    {
        IsBuildingAMine = false;
    }

    public void PickUpMine()
    {
        if(!IsBuildingAMine)
        {
            if(CenterDragon.GoldCoins >= MineCost)
            {
                IsBuildingAMine = true;
            }
            else
            {
                CenterDragon.PlayNegativeTreasure();
            }
        }
        else
        {
            IsBuildingAMine = false;
        }
    }

    public void PlayNormalMusic()
    {
        MusicAudioSource.clip = Music;
        MusicAudioSource.Play();
    }

    public void GameOver()
    {
        HasGameEnded = true;
        StartCoroutine("gameOverCoroutine");
    }

    public void Victory()
    {
        HasGameEnded = true;
        StartCoroutine("victoryCoroutine");
    }

    private IEnumerator randomFollowersSpawnCycle()
    {
        while (true)
        {
            TreasureInfo followerTresure = Random.Range(0f, 1f) > trashRatio ? GetRandomTreasure() : GetRandomTrash();
            Vector2 followerOffset = randomVector().normalized * distanceFromDragon;
            Vector2 followerPosition = (Vector2)CenterDragon.transform.position + followerOffset;
            spawnFollower(followerPosition, followerTresure);
            yield return new WaitForSeconds(60 / spawnRate);
        }
    }
    
    private IEnumerator gameOverCoroutine()
    {
        SoundEffectsSource.PlayOneShot(GameOverFanfare);

        MusicAudioSource.Stop();

        yield return new WaitForSeconds(4.2f);
        
        SceneManager.LoadScene("GameOver");
    }

    private IEnumerator victoryCoroutine()
    {
        SoundEffectsSource.PlayOneShot(VictoryFanfare);

        MusicAudioSource.Stop();

        yield return new WaitForSeconds(4.4f);
        
        SceneManager.LoadScene("Victory");
    }
}