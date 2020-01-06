using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        if (Instance != this)
            return;

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
            Instance.CurrentPlayerData = PlayerData.FromBytes(bytes);

            //using (RobustSerializer serializer = new RobustSerializer(bytes))
            //{
            //    Instance.CurrentPlayerData = serializer.readCompoundObject<PlayerData>();
            //}
        }
    }

    public static void SaveToDisk()
    {
        //using (RobustSerializer serializer = new RobustSerializer())
        //{
        //    serializer.writeCompoundObject(PlayerData);
        //    serializer.WriteToFile(Instance.PlayerFilePath);
        //}

        File.WriteAllBytes(Instance.PlayerFilePath, PlayerData.ToBytes());
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

    //Flags
    public bool Flag_TutorialGiftRecieved;

    public class LevelBestRecords
    {
        public uint Score = 0;
        public int Stars = 0;
        public float Time = float.MaxValue;
        public float TreasueTrashRatio { get { return TrashCollected / TreasureCollected; } }
        public int TreasureCollected = 0;
        public int TrashCollected = 0;
        public int FireballIncorrect = int.MaxValue;

        public byte[] ToBytes()
        {
            using (SimpleByteWriter writer = new SimpleByteWriter())
            {
                writer.Write(Score);
                writer.Write(Stars);
                writer.Write(Time);
                writer.Write(TreasureCollected);
                writer.Write(TrashCollected);
                writer.Write(FireballIncorrect);

                return writer.ToArray();
            }
        }

        public static LevelBestRecords FromBytes(byte[] bytes)
        {
            using (SimpleByteWriter writer = new SimpleByteWriter(bytes))
            {
                return new LevelBestRecords()
                {
                    Score = writer.ReadUInt(),
                    Stars = writer.ReadInt(),
                    Time = writer.ReadFloat(),
                    TreasureCollected = writer.ReadInt(),
                    TrashCollected = writer.ReadInt(),
                    FireballIncorrect = writer.ReadInt(),
                };
            }
        }
    }

    public byte[] ToBytes()
    {
        var levels_byteDic = new Dictionary<string, byte[]>();
        foreach (var level in Levels)
        {
            levels_byteDic.Add(level.Key, level.Value.ToBytes());
        }

        using (SimpleByteWriter writer = new SimpleByteWriter())
        {
            writer.Write((IDictionary)levels_byteDic);

            writer.Write(Item_ClearScreen_Amount);
            writer.Write(Item_DoubleTreasure_Amount);
            writer.Write(Item_FastScore_Amount);

            writer.Write(Flag_TutorialGiftRecieved);

            return writer.ToArray();
        }
    }

    public static PlayerData FromBytes(byte[] bytes)
    {
        PlayerData result = new PlayerData();

        using (SimpleByteWriter writer = new SimpleByteWriter(bytes))
        {
            Dictionary<string, byte[]> levels_byteDic = (Dictionary<string, byte[]>)writer.ReadDictionary(typeof(Dictionary<string, byte[]>));
            
            if(levels_byteDic != null)
            {
                result.Levels = new Dictionary<string, LevelBestRecords>();
                foreach (var level in levels_byteDic)
                {
                    result.Levels.Add(level.Key, LevelBestRecords.FromBytes(level.Value));
                }
            }

            result.Item_ClearScreen_Amount = writer.ReadUInt();
            result.Item_DoubleTreasure_Amount = writer.ReadUInt();
            result.Item_FastScore_Amount = writer.ReadUInt();

            result.Flag_TutorialGiftRecieved = writer.ReadBool();
        }

        return result;
    }
}