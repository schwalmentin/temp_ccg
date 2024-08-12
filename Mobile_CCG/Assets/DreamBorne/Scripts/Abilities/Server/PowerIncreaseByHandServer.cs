using UnityEngine;

public class PowerIncreaseByHandServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card)
    {
        // Execute action
        int handAmount = serverEngine.ServerData[playerId].Hand.Count;
        card.Power += handAmount;
        
        // Prepare action parameters for client
        PowerIncreaseByHandParams powerIncreaseByHandParams = new PowerIncreaseByHandParams(card.UniqueId, handAmount);
        string jsonParams = JsonUtility.ToJson(powerIncreaseByHandParams);
        card.ActionParams = jsonParams;
        
        // Set opponent flag
        card.PerformOpponent = true;
    }
}
