using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugManager : MonoBehaviour
{
    string myLog = "*begin log";
    string filename = "";
    bool doShow = true;
    int kChars = 700;

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable() { Application.logMessageReceived += this.Log; }

    void OnDisable() { Application.logMessageReceived -= this.Log; }

    void Update() { if (Input.GetKeyDown(KeyCode.Space)) {
        this.doShow = !this.doShow; } }

    public void Log(string logString, string stackTrace, LogType type)
    {
        // for onscreen...
        this.myLog = this.myLog + " " + logString + "\n \n";
        if (this.myLog.Length > this.kChars) {
            this.myLog = this.myLog.Substring(this.myLog.Length - this.kChars); }

        // for the file ...
        /*
        if (this.filename == "")
        {
            string d = System.Environment.GetFolderPath(
               System.Environment.SpecialFolder.Desktop) + "/YOUR_LOGS";
            System.IO.Directory.CreateDirectory(d);
            string r = Random.Range(1000, 9999).ToString();
            this.filename = d + "/log-" + r + ".txt";
        }
        try
        {
            System.IO.File.AppendAllText(this.filename, logString + " "); 
        }
        catch { }
        */
    }

    void OnGUI()
    {
        if (!this.doShow) { return; }
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity,
           new Vector3(0.4f, 0.5f, 1.0f));
        GUI.TextArea(new Rect(10, 10, 540, 370), this.myLog);
    }
}
