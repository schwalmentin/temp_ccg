using Unity.VisualScripting;
using UnityEngine;
using ColorUtility = UnityEngine.ColorUtility;

public static class Logger
{
    private static string errorColor = "#f74e40";
    private static string warningColor = "#f2b705";

    private static string serverColor = "#7b9be2";
    private static string clientColor = "#7cbb6d";
    
    private static string actionColor = "#f982ce";
    
    private static bool isLogging = true;
    
    public static void LogError(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"CLIENT ERROR: {message}"
            .Bold()
            .Color(errorColor)
            .Size(16));
    }
    public static void LogWarning(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"Warning: {message}"
            .Bold()
            .Color(warningColor)
            .Size(16));
    }

    public static void LogServerInfo(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"Server Info: {message}"
            .Bold()
            .Color(serverColor)
            .Size(14));
    }
    public static void LogClientInfo(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"Client Info: {message}"
            .Bold()
            .Color(clientColor)
            .Size(14));
    }
    
    public static void LogServerRpc(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"Server Rpc: {message}"
            .Italic()
            .Color(serverColor)
            .Size(14));
    }
    public static void LogClientRpc(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"Client Rpc: {message}"
            .Italic()
            .Color(clientColor)
            .Size(14));
    }

    public static void LogAction(string message)
    {
        if (!isLogging) return;
        
        Debug.Log($"Action: {message}"
            .Bold()
            .Color(actionColor)
            .Size(14));
    }

    public static void LogEndGame(bool won)
    {
        if (!isLogging) return;
        
        Debug.Log($"You {(won ? "WON" : "LOST")} the game!"
            .Bold()
            .Color(won ? warningColor : errorColor)
            .Size(24));
    }

    public static void LogDefault(string message)
    {
        if (!isLogging) return;

        Debug.Log($"Default: {message}"
            .Bold());
    }
}

public static class StringExtension
{
    public static string Bold(this string str) => $"<b>{str}</b>";
    public static string Color(this string str, string clr) => $"<color={clr}>{str}</color>";
    public static string Italic(this string str) => $"<i>{str}</i>";
    public static string Size(this string str, int size) => $"<size={size}>{str}</size>";
}
