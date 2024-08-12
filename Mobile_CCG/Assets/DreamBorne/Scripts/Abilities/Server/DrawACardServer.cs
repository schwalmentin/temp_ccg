using UnityEngine;

public class DrawACardServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card)
    {
        // Perform action
        Card drawnCard = serverEngine.DrawCard(playerId);
        
        // Prepare parameters for client
        DrawCardParams drawCardParams = new DrawCardParams(drawnCard.Id, drawnCard.UniqueId);
        card.ActionParams = JsonUtility.ToJson(drawCardParams);
        
        // Set opponent flag
        card.PerformOpponent = false;
    }
}
