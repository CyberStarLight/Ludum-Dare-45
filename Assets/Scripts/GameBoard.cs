﻿using System;
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
    [Header("Level Settings")]
    public string LevelID;
    public string NextLevelScene;
    public LevelConfigOverride LevelOverride;

    [HideInInspector]
    public float CurrentScore;

    [HideInInspector]
    public float SpeedItemMultiplier { get { return speedItemActive ? GameSettings.Instance.Item_FastScore_FollowerSpeedMultiplier : 1f; } }
    public float SpeedScoreMultiplier { get { return speedItemActive ? GameSettings.Instance.Item_FastScore_ScoreMultiplier : 1f; } }

    private int _goldCoins;
    public int GoldCoins
    {
        get { return Mathf.Clamp(_goldCoins, 0, Mathf.Min(GoldCoinCap, GameSettings.LevelConfig.Gold_Max)); }
        set { _goldCoins = Mathf.Clamp(value, 0, Mathf.Min(GoldCoinCap, GameSettings.LevelConfig.Gold_Max)); }
    }
    public int GoldCoinCap { get { return Mathf.Clamp((MineCount + 1) * GameSettings.LevelConfig.Mine_CapBonus, 0, GameSettings.LevelConfig.Gold_Max); } }
    public float CapRatio { get { return (float)GoldCoinCap / (float)GameSettings.LevelConfig.Gold_Max; } }
    public float GoldRatio { get { return GameSettings.LevelConfig.IsEndless ? (float)(GoldCoins % 1000000) / 1000000f : (float)GoldCoins / (float)GameSettings.LevelConfig.Gold_Max; } }
    public uint TotalGoldCollected { get; set; }

    public bool HasGameEnded { get; set; }
    public bool ProgressPause { get; set; }
    public int NextMineCost { get { return GameSettings.LevelConfig.MineCosts[Mathf.Min(MineCount, GameSettings.LevelConfig.MineCosts.LastIndex())]; } }
    public bool CanAffordMine { get { return GoldCoins >= NextMineCost; } }
    public bool BuildingDisabeld { get; set; }
    public bool ItemsDisabeld { get; set; }
    public bool DesiresDisabeld { get; set; }
    public float ScoreMultiplier { get { return 1f + MineCount; } }

    [Header("References")]
    public Dragon CenterDragon;
    public Road[] Roads;
    public HashSet<MineController> Mines =  new HashSet<MineController>();
    public Transform[] SpawnPoints;
    public Camera GameCamera;
    public Camera ScreenCamera;
    public SpriteRenderer Crosshair;
    public SpriteRenderer MineCursor;
    public RawImage GameScreen;
    public RawImage GameScreenArea;
    public GameUI MainGameUI;

    public uint ClearScreenItemAmount { get { return GameSaveManager.PlayerData.Item_ClearScreen_Amount; } }
    public uint SpeedItemAmount { get { return GameSaveManager.PlayerData.Item_FastScore_Amount; } }
    public uint DoubleTreasureItemAmount { get { return GameSaveManager.PlayerData.Item_DoubleTreasure_Amount; } }

    [HideInInspector]
    public bool IsBuildingAMine;

    [HideInInspector]
    public int MineCount = 0;

    public List<FollowerController> ExistingFollowers { get; set; }
    public bool IsSpawnDisabled { get; set; }
    public bool IsFireballDisabled { get; set; }
    public float NoSpawnBeforeTime { get; set; }
    public float NoClearBeforeTime { get; set; }

    private bool wasPanicMusic;
    private bool speedItemActive;
    private bool doubleTreasureItemActive;
    private float nextFollowerSpawn = 0f;
    private StageClearScreen.ClearStageParameters StageTrackers = new StageClearScreen.ClearStageParameters();

    public Vector3? CurrentPointerPosition { get; private set; }

    //UI raycast
    PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1 };
    List<RaycastResult> results = new List<RaycastResult>();

    private void Awake()
    {
        GameSettings.LevelConfig.LevelOverride = LevelOverride;
        ExistingFollowers = new List<FollowerController>();
    }

    private void Start()
    {

        FMODManager.Play(Music.GameMusic);

        StageTrackers.LevelID = LevelID;
    }

    private void FixedUpdate()
    {
        foreach (var mine in Mines)
        {
            mine.FixedUpdate_Managed();
        }
    }

    private void Update()
    {
        ExistingFollowers.RemoveAll(x => x == null);
        
        //score
        if(!HasGameEnded && !ProgressPause)
        {
            float scoreToAdd = Time.deltaTime * (GameSettings.LevelConfig.ScorePerSecond * (speedItemActive ? SpeedScoreMultiplier : 1f));
            scoreToAdd *= ScoreMultiplier;
            CurrentScore += scoreToAdd;
        }

        //Spawn followers
        if (Time.time >= NoSpawnBeforeTime && !IsSpawnDisabled && Time.time >= nextFollowerSpawn)
        {
            TreasureInfo followerTresure = Random.Range(0f, 1f) > GameSettings.LevelConfig.FollowerTrashRatio ? GetRandomTreasure() : GetRandomTrash();
            Vector3 spawnPos = SpawnPoints.GetRandom().position;
            spawnFollower(spawnPos, followerTresure);

            float spawnDelay = Random.Range(GameSettings.LevelConfig.FollowerSpawnDelay_Min, GameSettings.LevelConfig.FollowerSpawnDelay_Max);
            spawnDelay *= 1f - (CenterDragon.PanicRatio * 0.5f);
            nextFollowerSpawn = Time.time + spawnDelay;
        }

        CurrentPointerPosition = GetGameScreenPointerPosition();

        //Handle mouse clicks
        if (!IsTouchScreen())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool isMouseOverUI = results.Count == 0 || results.First().gameObject.name != "GameScreen";
            
            if (!HasGameEnded && CurrentPointerPosition.HasValue && !isMouseOverUI)
            {
                Cursor.visible = false;

                if (IsBuildingAMine)
                {
                    MineCursor.gameObject.SetActive(true);
                    Crosshair.gameObject.SetActive(false);
                    MineCursor.transform.position = CurrentPointerPosition.Value;
                    
                    if (AbleToPlaceMineAt(CurrentPointerPosition.Value))
                        MineCursor.color = Color.white;
                    else
                        MineCursor.color = Color.red;
                }
                else
                {
                    Crosshair.gameObject.SetActive(true);
                    MineCursor.gameObject.SetActive(false);
                    Crosshair.transform.position = CurrentPointerPosition.Value;
                }
            }
            else
            {
                Cursor.visible = true;
                Crosshair.gameObject.SetActive(false);
                MineCursor.gameObject.SetActive(false);
            }
        }

        if (
            !IsTouchScreen() &&
            Input.GetKeyDown(KeyCode.Mouse0) &&
            CurrentPointerPosition.HasValue &&
            IsBuildingAMine
            )
        {
            if (AbleToPlaceMineAt(CurrentPointerPosition.Value, Input.mousePosition))
                PlaceMine();
            else
                FMODManager.Play(Sounds.BuildFailed);
        }

        if(Input.GetKeyDown(KeyCode.Mouse1))
        {
            IsBuildingAMine = false;
        }

        //Panic music
        if(!wasPanicMusic && CenterDragon.RageRatio >= 0.75)
        {
            wasPanicMusic = true;

            FMODManager.Play(Music.GameMusicFast);
        }

        //Tracking
        if (!HasGameEnded && !ProgressPause)
        {
            StageTrackers.Time += Time.deltaTime;
        }

        //Cheats XD
        if (Input.GetKeyDown(KeyCode.G))
        {
            GoldCoins += 10000;
            GameSaveManager.PlayerData.Item_ClearScreen_Amount += 1; 
            GameSaveManager.PlayerData.Item_DoubleTreasure_Amount += 1; 
            GameSaveManager.PlayerData.Item_FastScore_Amount += 1;
            GameSaveManager.SaveToDisk();
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            MineCount = GameSettings.LevelConfig.Gold_RequiredMineCount;
            GoldCoins = GameSettings.LevelConfig.Gold_Max;
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            CenterDragon.Rage += GameSettings.Instance.Dragon_MaxRage * 0.1f;
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            CenterDragon.Panic += GameSettings.Instance.Dragon_MaxPanic * 0.1f;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            GameSaveManager.Instance.CurrentPlayerData = new PlayerData();
            GameSaveManager.SaveToDisk();
        }
    }

    private void OnDestroy()
    {
        Cursor.visible = true;
    }

    public bool IsTouchScreen()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.LinuxEditor:
            case RuntimePlatform.LinuxPlayer:
            case RuntimePlatform.OSXEditor:
            case RuntimePlatform.OSXPlayer:
            case RuntimePlatform.WebGLPlayer:
                return false;
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                return true;
            case RuntimePlatform.PS4:
            case RuntimePlatform.XboxOne:
            case RuntimePlatform.tvOS:
            case RuntimePlatform.Switch:
            case RuntimePlatform.Lumin:
            case RuntimePlatform.BJM:
            default:
                return false;
        }
    }

    public Vector3? GetGameScreenPointerPosition(Touch? touch = null, Vector3? offsetInInches = null)
    {
        Vector3 offset = Vector3.zero;
        Vector2 offsetV2 = Vector2.zero;
        if (offsetInInches.HasValue)
        {
            var offsetInPixels = offsetInInches.Value * (float)DisplayMetricsAndroid.DensityDPI;
            offset = offsetInPixels;
            offsetV2 = offset;
        }

        pointerData.position = Input.mousePosition + offset;
        EventSystem.current.RaycastAll(pointerData, results);
        pointerData.position = Input.mousePosition;

        bool isMouseDirectlyOverGameScreen = results.Count != 0 && results.Last().gameObject == GameScreen.gameObject;
        if (!isMouseDirectlyOverGameScreen)
            return null;

        Vector2 pointerPosition;

        if(IsTouchScreen())
        {
            if (Input.touches.Length == 0)
                return null;
            
            if (!touch.HasValue)
            {
                var touches = Input.touches.Where(x => x.phase == TouchPhase.Began).ToArray();
                if (touches.Length == 0)
                    return null;

                touch = touches[0];
            }

            pointerPosition = touch.Value.position + offsetV2;
        }
        else
        {
            pointerPosition = Input.mousePosition + offset;
        }

        var screenWorldPos = ScreenCamera.ScreenToWorldPoint(pointerPosition);
        
        Vector3[] corners = new Vector3[4];
        GameScreen.rectTransform.GetWorldCorners(corners);
        float imageWidth = corners[2].x - corners[0].x;
        float imageHeight = corners[2].y - corners[0].y;

        var relativeWorldPointerX = screenWorldPos.x - corners[0].x;
        var relativeWorldPointerY = corners[2].y - screenWorldPos.y;

        float pointX = relativeWorldPointerX / imageWidth;
        float pointY = relativeWorldPointerY / imageHeight;

        int gameScreenX = Mathf.RoundToInt(1920 * pointX);
        int gameScreenY = Mathf.RoundToInt(1080 - (1080 * pointY));

        var pointerPos = new Vector2(gameScreenX, gameScreenY);

        var worlPointerPos = GameCamera.ScreenToWorldPoint(pointerPos);
        worlPointerPos = new Vector3(worlPointerPos.x, worlPointerPos.y, 0f);

        results.Clear();
        return worlPointerPos;
    }

    public void PlaceMine()
    {
        if (!CurrentPointerPosition.HasValue)
            return;

        spawnMine(CurrentPointerPosition.Value);
    }
    public FollowerController spawnFollower(Vector3 position, TreasureInfo tresureHeld)
    {
        var followerPrefab = doubleTreasureItemActive ? GameSettings.Instance.FollowerDoublePrefab : GameSettings.Instance.FollowerPrefab;
        var newFollower = Instantiate(followerPrefab, position, Quaternion.identity, null);

        var searchResult = GetClosestPoint(newFollower.transform.position);
        newFollower.currentRoad = searchResult.ParentRoad;
        searchResult.ParentRoad.RegisterWalker(newFollower, searchResult.PointIndex);
        newFollower.TreasureHeld = tresureHeld;
        newFollower.TreasureRenderer.sprite = tresureHeld.Sprite;
        if(newFollower.TreasureRenderer2 != null)
            newFollower.TreasureRenderer2.sprite = tresureHeld.Sprite;

        newFollower.Master = CenterDragon;

        ExistingFollowers.Add(newFollower);

        return newFollower;
    }

    public bool IsPointOverUI(Vector3 screenPos)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current) { pointerId = -1, };
        pointerData.position = screenPos;
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        return results.Count == 0 || results.First().gameObject.name != "GameScreen";
    }

    public bool AbleToPlaceMineAt(Vector2 pos, Vector3? screenPos = null, Vector3? screenOffsetInInches = null)
    {
        Vector3 offset = Vector3.zero;
        Vector2 offsetV2 = Vector2.zero;
        if (screenOffsetInInches.HasValue)
        {
            var offsetInPixels = screenOffsetInInches.Value * (float)DisplayMetricsAndroid.DensityDPI;
            offset = offsetInPixels;
            offsetV2 = offset;
        }

        bool ableToPlace = true;

        if (screenPos.HasValue && IsPointOverUI(screenPos.Value + offset))
            return false;

        foreach (var G in FindObjectsOfType<MineController>())
            if (((Vector2)G.transform.position - pos).magnitude < .9f)
                ableToPlace = false;
        
        //Check the player has enough gold to pay
        if (!CanAffordMine)
        {
            ableToPlace = false;
        }

        //Check that the mine is not too close to a road
        var mineOffset = new Vector2(0f, -0.2f);
        if (GetClosestPoint(pos + mineOffset).Distance < GameSettings.Instance.MinRoadDistanceForMines)
        {
            ableToPlace = false;
        }

        return ableToPlace;
    }

    public void spawnMine(Vector2 mousePos)
    {
        bool ableToPlace = AbleToPlaceMineAt(mousePos);

        if (ableToPlace)
        {
            var newMine = Instantiate(GameSettings.Instance.MinePrefab, mousePos, Quaternion.identity, null).GetComponent<MineController>();
            //newMine.ore = brush.treasureState;
            newMine.isRandom = true;

            GoldCoins -= NextMineCost;

            Invoke("StopBuildingMine", 0.1f);

            FMODManager.Play(Sounds.MinePlaced);
        }
        else
        {
            CenterDragon.PlayNegativeBuild();
        }
    }

    public Road.PointSearchResult GetClosestPoint(Vector3 position)
    {
        var closestPointOnEachRoad = Roads.Select(x => x.GetClosestPoint(position));
        var closestRoadPoint = closestPointOnEachRoad.OrderBy(x => x.Distance).FirstOrDefault();

        return closestRoadPoint;
    }

    public void WrongTreasureDestroyed()
    {
        StageTrackers.FireballIncorrectAmount++;
    }

    public void WrongTreasureCollected()
    {
        StageTrackers.FireballIncorrectAmount++;
    }

    public void TreasureCollected()
    {
        StageTrackers.TreasureAmount++;
    }

    public void CollectedTrash()
    {
        StageTrackers.TrashAmount++;
    }

    public TreasureInfo GetTreasureToSpawn()
    {

        var goodTreasure = GameSettings.LevelConfig.Treasures.Where(x => !x.IsTrash).ToArray();

        return goodTreasure[Random.Range(0, goodTreasure.Length)];
    }

    public TreasureInfo GetRandomTreasure()
    {
        var goodTreasure = GameSettings.LevelConfig.Treasures.Where(x => !x.IsTrash).ToArray();

        return goodTreasure[Random.Range(0, goodTreasure.Length)];
    }

    public TreasureInfo GetRandomTrash()
    {
        TreasureInfo result;

        float rand = Random.value;
        if (rand < GameSettings.LevelConfig.FollowerTrashRatio)
            result = GetRandomTrash();
        else if (rand < GameSettings.LevelConfig.FollowerUnwantedRatio)
            result = GetUnwantedTreasure();
        else
            result = GetWantedTreasure();

        TreasureInfo followerTresure = Random.Range(0f, 1f) > GameSettings.LevelConfig.FollowerTrashRatio ? GetRandomTreasure() : GetRandomTrash();
        var trashTreasure = GameSettings.LevelConfig.Treasures.Where(x => x.IsTrash).ToArray();

        return trashTreasure[Random.Range(0, trashTreasure.Length)];
    }

    public TreasureInfo GetWantedTreasure()
    {
        var wantedTreasures = GameSettings.LevelConfig.Treasures.Where(x => !x.IsTrash && x != CenterDragon.ThoughtBubbleAnimation.undesired && CenterDragon.GetDesireLevel(x) > 0).ToArray();

        if (wantedTreasures.Length == 0)
            return GetRandomTrash();
        else
            return wantedTreasures.GetRandom();
    }

    public TreasureInfo GetUnwantedTreasure()
    {
        var unwantedTreasures = GameSettings.LevelConfig.Treasures.Where(x => !x.IsTrash && x != CenterDragon.ThoughtBubbleAnimation.desired && CenterDragon.GetDesireLevel(x) == 0).ToArray(); 

        if (unwantedTreasures.Length == 0)
            return GetRandomTrash();
        else
            return unwantedTreasures[Random.Range(0, unwantedTreasures.Length)];
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
        if (IsTouchScreen())
            return;

        if (!IsBuildingAMine && MineCount < GameSettings.LevelConfig.Gold_RequiredMineCount)
        {
            if (GoldCoins >= GameSettings.LevelConfig.MineCost)
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

    public void PickUpMineMobile()
    {
        if (!IsTouchScreen() || !MainGameUI.BuildMineButton.interactable)
            return;
        
        if (!IsBuildingAMine && MineCount < GameSettings.LevelConfig.Gold_RequiredMineCount)
        {
            if (GoldCoins >= GameSettings.LevelConfig.MineCost)
            {
                var touches = Input.touches.Where(x => x.phase == TouchPhase.Began).ToArray();
                if (touches.Length == 0)
                    return;

                int fingerId = touches.First().fingerId;

                _MineDragCoroutine = MineDragCoroutine(fingerId);
                StartCoroutine(_MineDragCoroutine);
            }
            else
            {
                CenterDragon.PlayNegativeTreasure();
            }
        }
    }

    //Inventory
    public void UseItem_ClearScreen()
    {
        if(ClearScreenItemAmount > 0)
        {
            GameSaveManager.PlayerData.Item_ClearScreen_Amount--;
            GameSaveManager.SaveToDisk();
            FMODManager.Play(Sounds.Item_ClearScreen);

            CenterDragon.ClearScreenAttack();
        }
    }

    public void UseItem_SpeedUp()
    {
        if (SpeedItemAmount > 0)
        {
            GameSaveManager.PlayerData.Item_FastScore_Amount--;
            GameSaveManager.SaveToDisk();
            FMODManager.Play(Sounds.Item_SpeedUp);

            _SpeedItemCoroutine = SpeedItemCoroutine();
            StartCoroutine(_SpeedItemCoroutine);
        }
    }

    public void UseItem_DoubleTreasure()
    {
        if (DoubleTreasureItemAmount > 0)
        {
            GameSaveManager.PlayerData.Item_DoubleTreasure_Amount--;
            GameSaveManager.SaveToDisk();
            FMODManager.Play(Sounds.Item_DoubleTreasure);

            //TODO: activate item here
            _DoubleTreasureItemCoroutine = DoubleTreasureItemCoroutine();
            StartCoroutine(_DoubleTreasureItemCoroutine);

        }
    }

    //Win and Lose
    public void GameOver()
    {
        if(GameSettings.LevelConfig.IsEndless)
        {
            Victory();
            return;
        }

        HasGameEnded = true;
        FMODManager.StopMusic();

        FMODManager.Play(Sounds.LoseFanfare, () => {
            SceneManager.LoadScene("GameOver");
        });
    }

    public void Victory()
    {
        HasGameEnded = true;
        FMODManager.StopMusic();
        FMODManager.Play(Sounds.WinFanfare);

        StageTrackers.ScoreAmount = Mathf.RoundToInt(CurrentScore);
        StageTrackers.TotalGoldCollected = TotalGoldCollected;

        if (GameSettings.LevelConfig.IsEndless)
        {
            if (GoldCoins > 10000000)
                StageTrackers.Stars = 3;
            else if (GoldCoins > 5000000)
                StageTrackers.Stars = 2;
            else
                StageTrackers.Stars = 1;
        }
        else
        {
            if (StageTrackers.TrashAmount == 0 && StageTrackers.FireballIncorrectAmount == 0)
                StageTrackers.Stars = 3;
            else if (StageTrackers.FireballIncorrectAmount == 0)
                StageTrackers.Stars = 2;
            else
                StageTrackers.Stars = 1;
        }

        //Save Highscore to save file
        PlayerData.LevelBestRecords record = null;
        if (!GameSaveManager.PlayerData.Levels.TryGetValue(LevelID, out record))
        {
            record = new PlayerData.LevelBestRecords();
            if(GameSettings.LevelConfig.IsEndless)
            {
                record.Time = float.MinValue;
            }
            GameSaveManager.PlayerData.Levels.Add(LevelID, record);
        }

        //Display stage clear screen
        MainGameUI.ClearScreen.DisplayScreen(StageTrackers);
    }

    public void VictoryContinue()
    {
        SceneManager.LoadScene(NextLevelScene);
    }

    public void VictoryRetry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    IEnumerator _MineDragCoroutine;
    IEnumerator MineDragCoroutine(int fingerId)
    {
        if (IsBuildingAMine)
            yield break;
        
        IsBuildingAMine = true;
        //MainGameUI.BuildMineButton.SetInteractable(false);
        MainGameUI.BuildMineButton.SetInteractable(false);
        MainGameUI.BuildMineButton_Icon.gameObject.SetActive(false);
        MineCursor.gameObject.SetActive(true);

        Vector3? lastValidPosition = null;
        while (true)
        {
            var touches = Input.touches.Where(x => x.fingerId == fingerId).ToArray();
            if (touches.Length == 0)
                break;

            var touch = touches.First();

            var fingerOffset = new Vector3(-0.25f, 0.25f, 0f);

            var pointerPosition = GetGameScreenPointerPosition(touch, fingerOffset);
            if (!pointerPosition.HasValue)
            {
                yield return null;
                continue;
            }

            //pointerPosition += new Vector3(-1f, 1f, 0f);
            //var pointerScreenPosition = touch.position + new Vector2(-1f, 1f);

            bool isValidPos = AbleToPlaceMineAt(pointerPosition.Value, touch.position, fingerOffset);
            if(isValidPos)
            {
                lastValidPosition = pointerPosition.Value;
            }
            else
            {
                lastValidPosition = null;
            }

            MineCursor.transform.position = pointerPosition.Value;

            var closestRoadPos = GetClosestPoint(MineCursor.transform.position).PointPosition;

            //Flip mine
            if (closestRoadPos.x < MineCursor.transform.position.x)
            {
                MineCursor.flipX = true;
            }
            else
            {
                MineCursor.flipX = false;
            }

            if (isValidPos)
                MineCursor.color = Color.white;
            else
                MineCursor.color = Color.red;

            yield return null;
        }

        if(lastValidPosition.HasValue)
            spawnMine(lastValidPosition.Value);

        MineCursor.color = Color.white;
        MineCursor.flipX = false;
        IsBuildingAMine = false;
        MainGameUI.BuildMineButton_Icon.gameObject.SetActive(true);
        MainGameUI.BuildMineButton.SetInteractable(true);
        MainGameUI.BuildMineButton_Icon.color = Color.white;
        MineCursor.gameObject.SetActive(false);
    }

    IEnumerator _SpeedItemCoroutine;
    IEnumerator SpeedItemCoroutine()
    {
        speedItemActive = true;

        float startTime = Time.time;

        //FMODManager.StopMusic();
        FMODManager.Play(Sounds.Item_SpeedUp);

        yield return new WaitForSeconds(1f);

        FMODManager.Play(Music.FastScoreMusic);

        while (Time.time < startTime + GameSettings.Instance.Item_FastScore_Duration)
        {
            yield return null;
        }

        FMODManager.Play(Music.GameMusic);
        speedItemActive = false;
    }

    IEnumerator _DoubleTreasureItemCoroutine;
    IEnumerator DoubleTreasureItemCoroutine()
    {
        doubleTreasureItemActive = true;

        yield return new WaitForSeconds(GameSettings.Instance.Item_DoubleTreasure_Duration);

        doubleTreasureItemActive = false;
    }
}