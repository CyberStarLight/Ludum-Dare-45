using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MobileLogDisplay : SmartBeheviourSingleton<MobileLogDisplay>
{
    public static List<LogEntry> LogHistory = new List<LogEntry>();

    public Canvas mainCanvas;
    public TextMeshProUGUI LogEntryPrefab;
    public Transform LogEntriesContainer;
    public RectTransform DisplayPanel;
    public GameObject OpenButton;
    public GameObject CloseButton;
    public GameObject ClearButton;

    protected override MobileLogDisplay GetSingletonInstance()
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

    public override void Awake()
    {
        base.Awake();

        Application.logMessageReceived += HandleLog;
    }

    private void Update()
    {
        //make sure to re-attach to main camera after switching scene
        if(mainCanvas.worldCamera == null && Camera.main != null)
            mainCanvas.worldCamera = Camera.main;
    }

    public override void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
        base.OnDestroy();
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        var newEntry = new LogEntry(logString, stackTrace, type);
        AddToLog(newEntry);
    }
    
    public void AddToLog(LogEntry entry)
    {
        LogHistory.Add(entry);
        var entryObj = Instantiate(LogEntryPrefab, LogEntriesContainer.gameObject.transform);
        entryObj.text = entry.ToString();
    }

    //UI triggers
    public void OpenLog()
    {
        DisplayPanel.sizeDelta = new Vector2(0f, 500f);
        CloseButton.SetActive(true);
        OpenButton.SetActive(false);
        LogEntriesContainer.gameObject.SetActive(true);
    }

    public void CloseLog()
    {
        DisplayPanel.sizeDelta = new Vector2(0f, 0f);
        CloseButton.SetActive(false);
        OpenButton.SetActive(true);
        LogEntriesContainer.gameObject.SetActive(false);
    }

    public void ClearLog()
    {
        LogHistory.Clear();

        var existingMessages = LogEntriesContainer.GetChildren().ToArray();
        for (int i = 0; i < existingMessages.Length; i++)
        {
            Destroy(existingMessages[i].gameObject);
        }
    }

}

[System.Serializable]
public class LogEntry
{
    private static int nextGeneratedId = 0;

    public int ID { get; set; }
    public DateTime Time { get; set; }
    public string Text { get; set; }
    public LogType Type { get; set; }
    public string StackTrace { get; set; }

    public LogEntry(string logString, string stackTrace, LogType type)
    {
        ID = nextGeneratedId;
        nextGeneratedId++;

        Time = DateTime.Now;
        Text = logString;
        StackTrace = stackTrace;
        Type = type;
    }

    public override string ToString()
    {
        return string.Format("{0}-{1}: {2}", Time.ToLongTimeString(), Type, Text);
    }
}