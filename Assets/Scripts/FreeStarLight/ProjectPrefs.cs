using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using Common.Cryptography;
using System;

namespace Plugins
{
    public static class ProjectPrefs
    {
        public static string PASS = "FriendshipIsMagicButEncryptionIsBetter!";
        public const string FILE_NAME = "Project.Pref";
        public const string IV = "IvysaurIsTheBest";
        private static string FOLDER_PATH = null;

        public static SaveFile Save { get; private set; }
        private static bool IsInitialized = false;
        
        public static string FullSavePath
        {
            get
            {
                if (FOLDER_PATH == null)
                    return (Application.dataPath + "/" + FILE_NAME).Replace('/', '\\');
                else
                    return Path.Combine(FOLDER_PATH, FILE_NAME);
            }
        }

        static ProjectPrefs()
        {
            if (!IsInitialized)
                Initialize();
        }

        private static void Initialize()
        {
            Debug.Log(FullSavePath);
            if (File.Exists(FullSavePath))
            {
                Debug.Log("File exists");
                LoadFromFile();
            }
            else
            {
                Debug.Log("File doesn't exists");
                Save = new SaveFile();
                SaveToFile();
            }
        }

        public static void SaveToFile()
        {
            var saveData = Save.ToString();

            File.WriteAllBytes(
                FullSavePath,
                AES.Encrypt(Encoding.UTF8.GetBytes(saveData), PASS, IV)
                );
        }

        public static void LoadFromFile()
        {
            byte[] saveData = File.ReadAllBytes(FullSavePath);

            byte[] saveDataBytes = AES.Decrypt(saveData, PASS, IV);
            string saveDataStr = Encoding.UTF8.GetString(saveDataBytes);
            Save = SaveFile.FromString(saveDataStr);
        }

        public static void ClearSave()
        {
            File.Delete(Path.Combine(Application.persistentDataPath, FILE_NAME));
            Save = new SaveFile();
        }

        //Manage bools
        public static bool BoolExists(string key)
        {
            return Save.BoolExists(key);
        }

        public static void SetBool(string key, bool value, bool overrideIfExists = true, bool saveFile = true)
        {
            Save.SetBool(key, value, overrideIfExists);
            SaveToFile();
        }

        public static bool GetBool(string key, bool? setIfEmpty = null)
        {
            return Save.GetBool(key, setIfEmpty);
        }

        //Manage ints
        public static bool IntExists(string key)
        {
            return Save.IntExists(key);
        }

        public static void SetInt(string key, int value, bool overrideIfExists = true, bool saveFile = true)
        {
            Save.SetInt(key, value, overrideIfExists);

            if (saveFile)
                SaveToFile();
        }

        public static int GetInt(string key, int? setIfEmpty = null)
        {
            return Save.GetInt(key, setIfEmpty);
        }

        //Manage floats
        public static bool FloatExists(string key)
        {
            return Save.FloatExists(key);
        }

        public static void SetFloat(string key, float value, bool overrideIfExists = true, bool saveFile = true)
        {
            Save.SetFloat(key, value, overrideIfExists);

            if (saveFile)
                SaveToFile();
        }

        public static float GetFloat(string key, float? setIfEmpty = null)
        {
            return Save.GetFloat(key, setIfEmpty);
        }

        //Manage strings
        public static bool StringExists(string key)
        {
            return Save.StringExists(key);
        }

        public static void SetString(string key, string value, bool overrideIfExists = true, bool saveFile = true)
        {
            Save.SetString(key, value, overrideIfExists);

            if (saveFile)
                SaveToFile();
        }

        public static string GetString(string key, string setIfEmpty = null)
        {
            return Save.GetString(key, setIfEmpty);
        }
    }

    public class SaveFile
    {
        public Dictionary<string, bool> SavedBools = new Dictionary<string, bool>();
        public Dictionary<string, int> SavedInts = new Dictionary<string, int>();
        public Dictionary<string, float> SavedFloats = new Dictionary<string, float>();
        public Dictionary<string, string> SavedStrings = new Dictionary<string, string>();

        public static SaveFile FromString(string str)
        {
            var result = new SaveFile();

            if (string.IsNullOrEmpty(str))
                return result;

            string[] lineSeperator = new string[] { "\r\n" };
            var lines = str.Split(lineSeperator, System.StringSplitOptions.RemoveEmptyEntries);

            char[] seperator = new char[] { ',' };
            foreach (var line in lines)
            {
                var parts = line.Split(seperator, 3);

                if (parts.Length != 3)
                    continue;

                switch (parts[0])
                {
                    case "bool":
                        result.SavedBools[parts[1]] = (parts[2] == "1" ? true : false);
                        continue;
                    case "int":
                        result.SavedInts[parts[1]] = int.Parse(parts[2]);
                        continue;
                    case "float":
                        result.SavedFloats[parts[1]] = float.Parse(parts[2]);
                        continue;
                    case "string":
                        result.SavedStrings[parts[1]] = parts[2];
                        continue;
                    default:
                        continue;
                }
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();

            foreach (var pair in SavedBools)
            {
                builder.AppendLine(string.Format("{0},{1},{2}", "bool", pair.Key, (pair.Value ? "1" : "0")));
            }

            foreach (var pair in SavedInts)
            {
                builder.AppendLine(string.Format("{0},{1},{2}", "int", pair.Key, pair.Value));
            }

            foreach (var pair in SavedFloats)
            {
                builder.AppendLine(string.Format("{0},{1},{2}", "float", pair.Key, pair.Value));
            }

            foreach (var pair in SavedStrings)
            {
                builder.AppendLine(string.Format("{0},{1},{2}", "string", pair.Key, pair.Value));
            }

            return builder.ToString();
        }

        //Manage bools
        public bool BoolExists(string key)
        {
            return SavedBools.ContainsKey(key);
        }

        public void SetBool(string key, bool value, bool overrideIfExists = true)
        {
            if (!overrideIfExists && SavedBools.ContainsKey(key))
                return;

            SavedBools[key] = value;
        }

        public bool GetBool(string key, bool? setIfEmpty = null)
        {
            bool value;
            if (SavedBools.TryGetValue(key, out value))
            {
                return value;
            }
            else if (setIfEmpty.HasValue)
            {
                SavedBools[key] = setIfEmpty.Value;
                return SavedBools[key];
            }
            else
            {
                throw new System.ArgumentException("Bool Key: \"" + key + "\" was not found in save file!", "key");
            }
        }

        //Manage ints
        public bool IntExists(string key)
        {
            return SavedInts.ContainsKey(key);
        }

        public void SetInt(string key, int value, bool overrideIfExists = true)
        {
            if (!overrideIfExists && SavedInts.ContainsKey(key))
                return;

            SavedInts[key] = value;
        }

        public int GetInt(string key, int? setIfEmpty = null)
        {
            int value;
            if (SavedInts.TryGetValue(key, out value))
            {
                return value;
            }
            else if (setIfEmpty.HasValue)
            {
                SavedInts[key] = setIfEmpty.Value;
                return SavedInts[key];
            }
            else
            {
                throw new System.ArgumentException("Int Key: \"" + key + "\" was not found in save file!", "key");
            }
        }

        //Manage floats
        public bool FloatExists(string key)
        {
            return SavedFloats.ContainsKey(key);
        }

        public void SetFloat(string key, float value, bool overrideIfExists = true)
        {
            if (!overrideIfExists && SavedFloats.ContainsKey(key))
                return;

            SavedFloats[key] = value;
        }

        public float GetFloat(string key, float? setIfEmpty = null)
        {
            float value;
            if (SavedFloats.TryGetValue(key, out value))
            {
                return value;
            }
            else if (setIfEmpty.HasValue)
            {
                SavedFloats[key] = setIfEmpty.Value;
                return SavedFloats[key];
            }
            else
            {
                throw new System.ArgumentException("Float Key: \"" + key + "\" was not found in save file!", "key");
            }
        }

        //Manage strings
        public bool StringExists(string key)
        {
            return SavedStrings.ContainsKey(key);
        }

        public void SetString(string key, string value, bool overrideIfExists = true)
        {
            if (!overrideIfExists && SavedStrings.ContainsKey(key))
                return;

            SavedStrings[key] = value;
        }

        public string GetString(string key, string setIfEmpty = null)
        {
            string value;
            if (SavedStrings.TryGetValue(key, out value))
            {
                return value;
            }
            else if (setIfEmpty != null)
            {
                SavedStrings[key] = setIfEmpty;
                return SavedStrings[key];
            }
            else
            {
                throw new System.ArgumentException("String Key: \"" + key + "\" was not found in save file!", "key");
            }
        }
    }
}
