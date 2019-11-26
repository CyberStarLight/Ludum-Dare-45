using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static class Files
{
    public static string PathToEXE()
    {
        return Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
    }

    public static string BuildPath(params string[] parts)
    {
        return BuildPath(null, parts);
    }

    public static string BuildPath(char? pathSeperator, params string[] parts)
    {
        StringBuilder result = new StringBuilder();

        char seperator = pathSeperator.HasValue ? pathSeperator.Value : Path.DirectorySeparatorChar;

        Func<string, string> replaceWrongSeperator;
        if (seperator == '\\')
            replaceWrongSeperator = (string str) => { return str.Replace('/', '\\'); };
        else if (seperator == '/')
            replaceWrongSeperator = (string str) => { return str.Replace('\\', '/'); };
        else
            replaceWrongSeperator = null;

        char? lastChar = null;
        foreach (var part in parts)
        {
            if (string.IsNullOrWhiteSpace(part))
                continue;

            char first = part[0];

            if (lastChar.HasValue && lastChar.Value != seperator && first != seperator)
                result.Append(seperator);

            string fixedPart = replaceWrongSeperator != null ? replaceWrongSeperator(part) : part;
            result.Append(fixedPart);

            lastChar = part[part.Length - 1];
        }

        return result.ToString();
    }

    public static string AutoIncrementFileName(string pathToFolder, string fileName, string fileExtension)
    {
        string PathPrefix = BuildPath(pathToFolder, fileName);
        string PathSuffix = "." + fileExtension;

        string currPath = PathPrefix + PathSuffix;
        if (!File.Exists(currPath))
            return currPath;
        
        for (int i = 2; i < int.MaxValue; i++)
        {
            currPath = PathPrefix + " " + i + PathSuffix;
            if (!File.Exists(currPath))
                return currPath;
        }

        return null;
    }
    public static string AutoIncrementFileName(string pathToFolder, string fileName, int number, string fileExtension)
    {
        string versionPart = number <= 1 ? String.Empty : " " + number;
        return BuildPath(pathToFolder, fileName + versionPart + "." + fileExtension);
    }

    public static string[] GetAutoIncrementedFiles(string pathToFolder, string fileName, string fileExtension)
    {
        List<string> result = new List<string>();

        string PathPrefix = BuildPath(pathToFolder, fileName);
        string PathSuffix = "." + fileExtension;

        string currPath = PathPrefix + PathSuffix;
        if (File.Exists(currPath))
            result.Add(currPath);
        else
            return new string[0];

        for (int i = 2; i < int.MaxValue; i++)
        {
            currPath = PathPrefix + " " + i + PathSuffix;
            if (File.Exists(currPath))
                result.Add(currPath);
            else
                break;
        }

        return result.ToArray();
    }

    public static string[] FilesFromAllSubfolders(string directory)
    {
        return FilesFromAllSubfolders(directory);
    }

    public static string[] FilesFromAllSubfolders(string directory, string extension = "*")
    {
        return Directory.GetFiles(directory, "*." + extension, SearchOption.AllDirectories);
    }
}
