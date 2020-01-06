using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameBoard MainBoard;
    public GameObject Background;
    public Transform TutorialUIOverlay;
    public Button SkipTutorialButton;
    public Transform FirstTrashFollowerOrigin;
    public Transform FirstTreasureFollowerOrigin;

    public float DelayBeforeTreasureMessage = 7f;
    public float DelayBeforeDestroyMessage = 10f;

    //tutorial

    private TutorialMessage currentMessage;
    private FollowerController FirstTreasureFollower;
    private FollowerController FirstTrashFollower;

    [Header("Tutorial Messages")]
    public TutorialMessage Message_DestroyFollowerPrefab;
    public TutorialMessage Message_GetReasurePrefab;
    public GameObject Message_WantedTreasure;
    public GameObject Message_Rage;
    public GameObject Message_Hoard;
    public GameObject Message_Mines;
    public GameObject Message_Items;

    public bool Finishedtutorial { get; set; }
    public bool FinishMessage { get; set; }

    private void Awake()
    {
        SkipTutorialButton.gameObject.SetActive(true);
        SkipTutorialButton.onClick.AddListener(stopTutorial);
    }

    private void Start()
    {
        MainBoard.IsSpawnDisabled = true;

        _TutorialCorutine = TutorialCorutine();
        StartCoroutine(_TutorialCorutine);

        MainBoard.GetUnwantedTreasure();

        if(!GameSaveManager.PlayerData.Flag_TutorialGiftRecieved)
        {
            GameSaveManager.PlayerData.Item_ClearScreen_Amount++;
            GameSaveManager.PlayerData.Item_DoubleTreasure_Amount++;
            GameSaveManager.PlayerData.Item_FastScore_Amount++;
            GameSaveManager.PlayerData.Flag_TutorialGiftRecieved = true;
            GameSaveManager.SaveToDisk();
        }
    }

    private void Update()
    {
        if (!Finishedtutorial)
        {
            if (GotClick())
            {
                FinishMessage = true;
            }
        }
    }

    public void stopTutorial()
    {
        if (Finishedtutorial)
            return;

        print("stopTutorial()");
        Finishedtutorial = true;
        StopCoroutine(_TutorialCorutine);

        if(currentMessage != null)
            Destroy(currentMessage.gameObject);

        Background.SetActive(false);
        TutorialUIOverlay.gameObject.SetActive(false);
        SkipTutorialButton.gameObject.SetActive(false);

        MainBoard.IsSpawnDisabled = false;
        MainBoard.IsFireballDisabled = false;
        MainBoard.ProgressPause = false;
        Time.timeScale = 1f;
    }

    private void startTutorialPause(bool stopTime = true)
    {
        Background.SetActive(true);
        TutorialUIOverlay.gameObject.SetActive(true);

        if (stopTime)
            Time.timeScale = 0f;
        else
            MainBoard.ProgressPause = true;
    }

    private void stopTutorialPause()
    {
        Background.SetActive(false);
        TutorialUIOverlay.gameObject.SetActive(false);
        Time.timeScale = 1f;
        MainBoard.ProgressPause = false;
    }

    IEnumerator _TutorialCorutine;
    IEnumerator TutorialCorutine()
    {
        while (MainBoard.CenterDragon.DesiredTreasure1 == null || MainBoard.CenterDragon.DesiredTreasure1.Value == Treasure.None)
        {
            yield return null;
        }

        //Wanted treasure message
        startTutorialPause();
        Message_WantedTreasure.SetActive(true);

        FinishMessage = false;
        while (!FinishMessage)
            yield return null;
        FinishMessage = false;

        Message_WantedTreasure.SetActive(false);
        stopTutorialPause();

        //Get treasure message
        MainBoard.IsFireballDisabled = true;
        FirstTreasureFollower = MainBoard.spawnFollower(FirstTreasureFollowerOrigin.position, MainBoard.CenterDragon.DesiredTreasure1);

        yield return new WaitForSeconds(DelayBeforeTreasureMessage);

        startTutorialPause(false);
        TutorialUIOverlay.gameObject.SetActive(false);
        currentMessage = Instantiate(Message_GetReasurePrefab);
        currentMessage.SetTarget(FirstTreasureFollower.transform.position);

        FinishMessage = false;
        while (!FinishMessage)
            yield return null;
        FinishMessage = false;

        stopTutorialPause();
        Destroy(currentMessage.gameObject);

        while (FirstTreasureFollower != null)
        {
            yield return null;
        }

        //Hoard message
        Message_Hoard.SetActive(true);
        startTutorialPause();

        FinishMessage = false;
        while (!FinishMessage)
            yield return null;
        FinishMessage = false;

        Message_Hoard.SetActive(false);
        stopTutorialPause();

        //Mines message
        Message_Mines.SetActive(true);
        startTutorialPause(false);

        FinishMessage = false;
        while (!FinishMessage)
            yield return null;
        FinishMessage = false;

        Message_Mines.SetActive(false);
        stopTutorialPause();

        //Destroy trash message
        FirstTrashFollower = MainBoard.spawnFollower(new Vector3(-10f, -5f, 0f), MainBoard.GetUnwantedTreasure());

        yield return new WaitForSeconds(DelayBeforeDestroyMessage);

        startTutorialPause(false);

        TutorialUIOverlay.gameObject.SetActive(false);
        currentMessage = Instantiate(Message_DestroyFollowerPrefab);
        currentMessage.SetTarget(FirstTrashFollower.transform.position);

        MainBoard.IsFireballDisabled = false;

        FinishMessage = false;
        while (!FinishMessage && FirstTrashFollower != null)
        {
            yield return new WaitForSeconds(0.2f);
        }
        FinishMessage = false;

        Destroy(currentMessage.gameObject);
        stopTutorialPause();

        while (FirstTrashFollower != null)
        {
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(2f);

        //Rage message
        Message_Rage.SetActive(true);
        startTutorialPause();

        FinishMessage = false;
        while (!FinishMessage)
            yield return null;
        FinishMessage = false;

        Message_Rage.SetActive(false);
        stopTutorialPause();

        //Items message
        Message_Items.SetActive(true);
        startTutorialPause();

        FinishMessage = false;
        while (!FinishMessage)
            yield return null;
        FinishMessage = false;

        Message_Items.SetActive(false);
        stopTutorialPause();

        //Tutorial End
        stopTutorial();
    }

    private bool GotClick()
    {
        if (MainBoard.IsTouchScreen())
        {
            if (Input.touches.Length == 0)
                return false;

            return Input.touches.Any(x => x.phase == TouchPhase.Began);
        }
        else
        {
            return Input.GetKeyDown(KeyCode.Mouse0);
        }
    }
}
