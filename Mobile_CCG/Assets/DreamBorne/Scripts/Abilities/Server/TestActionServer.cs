using UnityEngine;

public class TestActionServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ServerData serverData, Card card)
    {
        // Execute action
        Logger.LogAction($"This is a TestAction processed by the Server by {serverData.Name}");
        
        // Prepare action parameters for client
        TestActionParams testActionParams = new TestActionParams($"This is a TestAction message invoked by {serverData.Name}!");
        string jsonParams = JsonUtility.ToJson(testActionParams);
        card.ActionParams = jsonParams;
        
        // Set opponent flag
        card.PerformOpponent = false;
    }
}
