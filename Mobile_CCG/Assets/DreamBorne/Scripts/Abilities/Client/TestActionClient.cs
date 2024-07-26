using System;
using UnityEngine;

public class TestActionClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams)
    {
        try
        {
            // Get params
            TestActionParams testActionParams = JsonUtility.FromJson<TestActionParams>(jsonParams);
            
            // Log message
            Logger.LogAction(testActionParams.testMessage);
        }
        catch (ArgumentException e)
        {
            Logger.LogError(e.Message);
            // Request a server update of the current game state
            // playerEngine.HardReset
            throw;
        }
    }
}

[System.Serializable]
public struct TestActionParams
{
    public string testMessage;

    public TestActionParams(string testMessage)
    {
        this.testMessage = testMessage;
    }
}
