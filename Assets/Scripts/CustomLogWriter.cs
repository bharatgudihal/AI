using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class CustomLogWriter : MonoBehaviour {

    private StreamWriter writer;
    private bool isInitialized;

    [SerializeField]
    private string filePath = "Default.txt";

    private void OnEnable()
    {
        InitializeFile();
    }

    private void OnDisable()
    {
        CleanupFile();
    }

    private void InitializeFile()
    {
        if (!File.Exists(filePath))
        {
            writer = File.CreateText(filePath);
        }
        else
        {
            writer = new StreamWriter(filePath);
        }
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
