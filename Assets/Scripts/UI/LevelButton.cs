using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    public string LevelID;
    public string PreviousLevelID;
    public Button MainButton;
    public TextMeshProUGUI LevelText;
    public Image LockImage;
    public Image Star1;
    public Image Star2;
    public Image Star3;

    private void Awake()
    {
        if(string.IsNullOrWhiteSpace(PreviousLevelID))
        {
            Unlock();
        }
        else
        {
            PlayerData.LevelBestRecords levelRecords;
            if (!GameSaveManager.PlayerData.Levels.TryGetValue(PreviousLevelID, out levelRecords))
                levelRecords = new PlayerData.LevelBestRecords();

            if (levelRecords.Stars >= 1)
                Unlock();
            else
                Lock();
        }
    }

    public void Lock()
    {
        MainButton.interactable = false;

        LockImage.gameObject.SetActive(true);
        Star1.gameObject.SetActive(false);
        Star2.gameObject.SetActive(false);
        Star3.gameObject.SetActive(false);
        LevelText.gameObject.SetActive(false);
    }

    public void Unlock()
    {
        MainButton.interactable = true;

        LockImage.gameObject.SetActive(false);
        LevelText.gameObject.SetActive(true);

        PlayerData.LevelBestRecords levelRecords;
        if (!GameSaveManager.PlayerData.Levels.TryGetValue(LevelID, out levelRecords))
            levelRecords = new PlayerData.LevelBestRecords();

        Star1.gameObject.SetActive(levelRecords.Stars >= 1 ? true : false);
        Star2.gameObject.SetActive(levelRecords.Stars >= 2 ? true : false);
        Star3.gameObject.SetActive(levelRecords.Stars >= 3 ? true : false);
    }

    public void PlayLevel()
    {
        SceneManager.LoadScene(LevelID);
    }
}
