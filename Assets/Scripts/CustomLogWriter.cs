using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CustomLogWriter : MonoBehaviour {

    private StreamWriter writer;
    private bool isInitialized;

    public string filePath = "Default.txt";

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        CleanupFile();
    }

    private void InitializeFile()
    {
        string fullFilePath = filePath + ".log";
        if (File.Exists(fullFilePath))
        {
            File.Delete(fullFilePath);
        }
        writer = File.CreateText(fullFilePath);
        if (writer != null)
        {
            isInitialized = true;
        }
    }

    public void Write(string text)
    {
        if (!isInitialized)
        {
            InitializeFile();
        }
        writer.WriteLine(text);
    }

    private void CleanupFile()
    {
        if (writer != null)
        {
            writer.Close();
        }
    }
}
