using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ReflectionSerializer;
using System.IO;
using System.Linq;

public interface ISaveManager
{
    bool IsInitialized { get; }
    string PathToSaveFolder { get; }

    void Save(SaveFile file);
}

public static class SaveManager
{
    public static bool IsInitialized { get; private set; }
    public static string PathToSaveFolder { get; private set; }
    
    private static string saveFolderName;
    private static string GetSaveFolderName()
    {
        string[] folders = Directory.GetDirectories(PathToSaveFolder);
        string saveFolder = folders.FirstOrDefault(x => x.Split(Path.DirectorySeparatorChar).Last().StartsWith("" + 0 + 0 + 0));

        string name;
        if (saveFolder == null)
        {
            name = "" + 0 + 0 + 0 + RandomCryptoManager.GetString(10, RandomCryptoManager.ASCII_ALPHA_NUMERIC_CHARS);
            saveFolder = Utilities.BuildPathFromParts(PathToSaveFolder, name);
            Directory.CreateDirectory(saveFolder);
        }
        else
        {
            name = saveFolder.Split(Path.DirectorySeparatorChar).Last();
        }

        return name;
    }

    private static string getGarbledData()
    {
        return HashManager.SHA256(saveFolderName + (98 * 65 - 12));
    }
    
    private static string getPathToFile(SaveFile file, bool ensurePathExists = true)
    {
        string pathToFolder = Utilities.BuildPathFromParts(PathToSaveFolder, saveFolderName, file.FileCategory);

        if(ensurePathExists)
        {
            Directory.CreateDirectory(pathToFolder);
        }

        return Utilities.BuildPathFromParts(pathToFolder, file.FileName + file.FileExtension);
    }

    public static void Initialize(string absolutePathToSaveFolder)
    {
        PathToSaveFolder = absolutePathToSaveFolder;
        saveFolderName = GetSaveFolderName();

        //string pathTosaveFiles = Utilities.BuildPathFromParts(null, PathToSaveFolder, saveFolderName);

        //var files = Files.GetAllFiles();
        //foreach (var file in files)
        //{
        //    if (file == null)
        //        continue;

        //    string pathToCategory = Utilities.BuildPathFromParts(null, pathTosaveFiles, file.FileCategory);
        //    if (!Directory.Exists(pathToCategory))
        //        Directory.CreateDirectory(pathToCategory);

        //    string filePath = Utilities.BuildPathFromParts(null, pathToCategory, file.FileName + file.FileExtension);
        //    if(!File.Exists(filePath))
        //    {
        //        Save(file);
        //    }
        //    else
        //    {
        //        Load(file);
        //    }
        //}
        
        IsInitialized = true;
    }
    
    public static void Save(SaveFile file)
    {
        if (file == null)
            return;

        string path = getPathToFile(file);
        byte[] rawData = file.GetRawData();
        Debug.Log("GZipCompress()");
        byte[] zippeddata = Utilities.GZipCompress(rawData);
        Debug.Log("GZipCompress() - done");

        byte[] randomIV = RandomCryptoManager.GetBytes(32);
        byte[] encryptedData = AES.Encrypt(zippeddata, getGarbledData(), randomIV);
        byte[] fileData = randomIV.CombinedWith(encryptedData);

        File.WriteAllBytes(path, fileData);
    }

    public static void Load(SaveFile file)
    {
        if (file == null)
            return;

        string path = getPathToFile(file);
        if(!File.Exists(path))
        {
            file.LoadFromRawData(null);
            return;
        }

        byte[] rawData = File.ReadAllBytes(path);

        byte[] iv = rawData.GetRange(0, 31);
        byte[] encryptedData = rawData.GetRange(32);

        byte[] decryptedData = AES.Decrypt(encryptedData, getGarbledData(), iv);
        Debug.Log("GZipDecompress()");
        byte[] decompressedData = Utilities.GZipDecompress(decryptedData);
        Debug.Log("GZipDecompress() - done");

        file.LoadFromRawData(decompressedData);
    }
}

//public interface ISaveFiles
//{
//    ISaveManager Parent { get; }
//    SaveFile[] GetAllFiles();
//    void SaveAllFiles();
//    void LoadAllFiles();
//}

public abstract class SaveFile
{
    public abstract string FileName { get; set; }
    public abstract string FileExtension { get; set; }
    public abstract string FileCategory { get; set; }

    public abstract byte[] GetRawData();
    public abstract void LoadFromRawData(byte[] bytes);
}

public abstract class SaveFile<T> : SaveFile
{
    public T Data;

    public SaveFile(T _Data = default)
    {
        Data = _Data;
    }
    
    public override byte[] GetRawData()
    {
        if (Data == null)
            return new byte[0];

        using (ByteWriter writer = new ByteWriter())
        {
            writer.Write(Data);
            return writer.ToArray();
        }
    }

    public override void LoadFromRawData(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
        {
            Data = default;
            return;
        }

        using (ByteWriter writer = new ByteWriter(bytes))
        {
            Data = writer.Read<T>();
        }
    }

    public void Save()
    {
        SaveManager.Save(this);
    }

    public void Load()
    {
        SaveManager.Load(this);
    }
}