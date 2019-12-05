using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StageClearScreen : MonoBehaviour
{
    [Header("DisplaySettings")]
    public float ScaleContainerDuration = 3f;
    public float StarCountDuration = 3f;
    public float ScoreCountDuration = 3f;
    public float AccuracyCountDuration = 1.5f;
    public float TimeCountDuration = 1.5f;

    [Header("References")]
    public Image Container;
    public Image Title;
    public Image Star1;
    public Image Star2;
    public Image Star3;
    public Sprite StarFullSprite;
    public Sprite StarEmptySprite;
    public TextMeshProUGUI ScoreAmountText;
    public TextMeshProUGUI AccuracyText;
    public TextMeshProUGUI TimeText;

    public Transform UnmovingElementsContainer;
    public GameObject StarsNewRecord;
    public GameObject ScoreNewRecord;
    public GameObject AccuracyNewRecord;
    public GameObject TimeNewRecord;

    public void DisplayScreen(ClearStageParameters parameters, Action Callback = null)
    {
        gameObject.SetActive(true);
        _DisplayScreenCoroutine = DisplayScreenCoroutine(parameters, Callback);
        StartCoroutine(_DisplayScreenCoroutine);
    }

    IEnumerator _DisplayScreenCoroutine;
    IEnumerator DisplayScreenCoroutine(ClearStageParameters p, Action Callback = null)
    {
        Container.transform.localScale = new Vector3(1f, 0f, 1f);
        Container.gameObject.SetActive(true);

        var items = Container.transform.GetChildren();
        foreach (var item in items)
        {
            item.gameObject.SetActive(false);
        }

        while (Container.transform.localScale.y < 0.999f)
        {
            Container.transform.localScale += new Vector3(0f, Time.deltaTime / ScaleContainerDuration, 0f);
            yield return null;
        }

        UnmovingElementsContainer.gameObject.SetActive(true);

        PlayerData.LevelBestRecords levelRecord = null;
        GameSaveManager.PlayerData.Levels.TryGetValue(p.LevelID, out levelRecord);

        //Stars
        float timePerStar = StarCountDuration / 3f;
        if (p.Stars > 0)
        {
            yield return new WaitForSeconds(timePerStar);
            Star1.gameObject.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }
        if (p.Stars > 1)
        {
            yield return new WaitForSeconds(timePerStar);
            Star2.gameObject.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }
        if (p.Stars > 2)
        {
            yield return new WaitForSeconds(timePerStar);
            Star3.gameObject.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }

        if(levelRecord != null && levelRecord.Stars < p.Stars)
        {
            levelRecord.Stars = p.Stars;
            StarsNewRecord.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }

        //Score
        float currentScore = 0;
        ScoreAmountText.text = Mathf.FloorToInt(currentScore).ToString();
        ScoreAmountText.gameObject.SetActive(true);

        float ratioPerSecond = 1f / ScoreCountDuration;
        float endTime = Time.time + ScoreCountDuration;
        while (Time.time <= endTime)
        {
            currentScore += ((float)p.ScoreAmount * ratioPerSecond) * Time.deltaTime;
            ScoreAmountText.text = Mathf.FloorToInt(currentScore).ToString();
            yield return null;
        }
        ScoreAmountText.text = p.ScoreAmount.ToString();

        if (levelRecord != null && levelRecord.Score < p.ScoreAmount)
        {
            levelRecord.Score = (uint)p.ScoreAmount;
            ScoreNewRecord.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }

        //Accuracy
        float currentAccuracy = 0;
        AccuracyText.text = string.Format("{0} misses", Mathf.FloorToInt(currentAccuracy));
        AccuracyText.gameObject.SetActive(true);

        ratioPerSecond = 1f / AccuracyCountDuration;
        endTime = Time.time + AccuracyCountDuration;
        while (Time.time <= endTime)
        {
            currentAccuracy += ((float)p.FireballIncorrectAmount * ratioPerSecond) * Time.deltaTime;
            AccuracyText.text = string.Format("{0} misses", Mathf.FloorToInt(currentAccuracy));
            yield return null;
        }
        AccuracyText.text = string.Format("{0} misses", Mathf.FloorToInt(p.FireballIncorrectAmount));

        if (levelRecord != null && levelRecord.FireballIncorrect > p.FireballIncorrectAmount)
        {
            levelRecord.FireballIncorrect = p.FireballIncorrectAmount;
            AccuracyNewRecord.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }

        //Time
        float currentTime = 0;
        TimeText.text = string.Format("{0}m {1}s", Mathf.FloorToInt(currentTime/60f), Mathf.FloorToInt(currentTime % 60f));
        TimeText.gameObject.SetActive(true);

        ratioPerSecond = 1f / TimeCountDuration;
        endTime = Time.time + TimeCountDuration;
        while (Time.time <= endTime)
        {
            currentTime += ((float)p.Time * ratioPerSecond) * Time.deltaTime;
            TimeText.text = string.Format("{0}m {1}s", Mathf.FloorToInt(currentTime / 60f), Mathf.FloorToInt(currentTime % 60f));
            yield return null;
        }
        TimeText.text = string.Format("{0}m {1}s", Mathf.FloorToInt(p.Time / 60f), Mathf.FloorToInt(p.Time % 60f));

        if (levelRecord != null && levelRecord.Time > p.Time)
        {
            levelRecord.Time = p.Time;
            TimeNewRecord.SetActive(true);
            FMODManager.Play(Sounds.PositiveTreasure);
        }

        //Save highscores to file
        GameSaveManager.SaveToDisk();

        Callback?.Invoke();
    }

    public class ClearStageParameters
    {
        public string LevelID;
        public int Stars;
        public int ScoreAmount;
        public int FireballAmount;
        public int FireballCorrectAmount;
        public int FireballIncorrectAmount;
        public int TrashAmount;
        public int TreasureAmount;
        public float Time;
    }
}
