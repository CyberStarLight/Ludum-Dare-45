using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GameSaveManager : SmartBeheviourSingleton<GameSaveManager>
{
    public const string SAVE_FOLDER_NAME = "SaveData";
    public const string PLAYER_DATA_FILE_NAME = "Player.sav";

    public PlayerData CurrentPlayerData = new PlayerData();
    public static PlayerData PlayerData { get { return Instance.CurrentPlayerData; } }

    public string SaveFolderPath { get; private set; }
    public string PlayerFilePath { get; private set; }

    public override void Awake()
    {
        base.Awake();

        SaveFolderPath = Files.BuildPath(Application.persistentDataPath, SAVE_FOLDER_NAME);
        print(SaveFolderPath);
        Directory.CreateDirectory(SaveFolderPath);

        PlayerFilePath = Files.BuildPath(SaveFolderPath, PLAYER_DATA_FILE_NAME);

        RobustSerializer.Initialize(SaveFolderPath);

        if (File.Exists(PlayerFilePath))
            LoadFromDisk();
        else
            SaveToDisk();
    }

    protected override GameSaveManager GetSingletonInstance()
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

    public static void LoadFromDisk()
    {
        if(File.Exists(Instance.PlayerFilePath))
        {
            var bytes = File.ReadAllBytes(Instance.PlayerFilePath);

            using (RobustSerializer serializer = new RobustSerializer(bytes))
            {
                Instance.CurrentPlayerData = serializer.readCompoundObject<PlayerData>();
            }
        }
    }

    public static void SaveToDisk()
    {
        using (RobustSerializer serializer = new RobustSerializer())
        {
            serializer.writeCompoundObject(PlayerData);
            serializer.WriteToFile(Instance.PlayerFilePath);
        }
    }

    public override void OnDestroy()
    {
        SaveToDisk();
        base.OnDestroy();
    }

}

//Save files
public class PlayerData
{
    //Levels
    public Dictionary<string, LevelBestRecords> Levels = new Dictionary<string, LevelBestRecords>();

    //Items
    public uint Item_ClearScreen_Amount = 0;
    public uint Item_DoubleTreasure_Amount = 0;
    public uint Item_FastScore_Amount = 0;

    public class LevelBestRecords
    {
        public uint Score = 0;
        public int Stars = 0;
        public float Time = float.MaxValue;
        public float TreasueTrashRatio { get { return TrashCollected / TreasureCollected; } }
        public int TreasureCollected = 0;
        public int TrashCollected = 0;
        public int FireballIncorrect = int.MaxValue;
    }
}