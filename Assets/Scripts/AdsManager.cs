using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : SmartBeheviourSingleton<AdsManager>, IUnityAdsListener
{
    public const string GOOGLE_STORE_GAME_ID = "3326259";
    public const string APPLE_STORE_GAME_ID = "3326258";

    public const string REWARDED_ADS_PLACEMENT_ID = "rewardedVideo";

    public static bool RewardedAdReady { get { return Advertisement.IsReady(REWARDED_ADS_PLACEMENT_ID); }  }

    public GameObject DisplayAdCanvas;

    private bool isTestMode;

    private bool isShowingAd;
    private Action successCallback;
    private Action failureCallback;

    void Start()
    {
        isTestMode = Application.platform == RuntimePlatform.WindowsEditor;
        Advertisement.AddListener(this);

        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Advertisement.Initialize(APPLE_STORE_GAME_ID, isTestMode);
        }
        else
        {
            Advertisement.Initialize(GOOGLE_STORE_GAME_ID, isTestMode);
        }
    }

    //Specific ad actions
    public static void RewardAd_ClearScreen()
    {
        Instance.ShowRewardedVideo(() => {
            GameSaveManager.PlayerData.Item_ClearScreen_Amount++; 
            GameSaveManager.SaveToDisk(); 
        });
    }
    public static void RewardAd_DoubleTreasure()
    {
        Instance.ShowRewardedVideo(() => {
            GameSaveManager.PlayerData.Item_DoubleTreasure_Amount++; 
            GameSaveManager.SaveToDisk(); 
        });
    }
    public static void RewardAd_FastScore()
    {
        Instance.ShowRewardedVideo(() => {
            GameSaveManager.PlayerData.Item_FastScore_Amount++; 
            GameSaveManager.SaveToDisk(); 
        });
    }

    public void ShowRewardedVideo(Action _successCallback, Action _failureCallback = null)
    {
        if (isShowingAd)
            return;

        DisplayAdCanvas.SetActive(true);
        isShowingAd = true;
        successCallback = _successCallback;
        failureCallback = _failureCallback;

        Advertisement.Show(REWARDED_ADS_PLACEMENT_ID);
    }

    //Events
    public void OnUnityAdsDidFinish(string placementId, ShowResult showResult)
    {
        if (showResult == ShowResult.Finished)
        {
            successCallback?.Invoke();
            finishAdDisplay();
        }
        else if (showResult == ShowResult.Skipped)
        {
            failureCallback?.Invoke();
            finishAdDisplay();
        }
        else if (showResult == ShowResult.Failed)
        {
            failureCallback?.Invoke();
            finishAdDisplay();
        }
    }

    private void finishAdDisplay()
    {
        DisplayAdCanvas.SetActive(false);
        successCallback = null;
        failureCallback = null;
        isShowingAd = false;
    }

    public void OnUnityAdsDidError(string message)
    {
        failureCallback?.Invoke();
        finishAdDisplay();
    }

    public void OnUnityAdsDidStart(string placementId)
    {
        DisplayAdCanvas.SetActive(false);
    }

    public void OnUnityAdsReady(string placementId)
    {
    }

    //Singleton
    protected override AdsManager GetSingletonInstance()
    {
        return this;
    }

    protected override bool IsDestroyedOnLoad()
    {
        return false;
    }

    protected override bool OverridePreviousSingletons()
    {
        return false;
    }
}
