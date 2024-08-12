using System.Collections.Generic;
using UnityEngine;

public class DrawCardsEqualsHandServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card)
    {
        // Perform action
        List<DrawCardParams> drawCardParams = new List<DrawCardParams>();
        int cardsToDraw = serverEngine.ServerData[playerId].Hand.Count;
        for (int i = 0; i < cardsToDraw; i++)
        {
            Card drawnCard = serverEngine.DrawCard(playerId);
            drawCardParams.Add(new DrawCardParams(drawnCard.Id, drawnCard.UniqueId));
        }
        
        // Prepare parameters for client
        DrawMultipleCardsParams drawMultipleCardsParams = new DrawMultipleCardsParams(drawCardParams.ToArray());
        card.ActionParams = JsonUtility.ToJson(drawMultipleCardsParams);
        
        // Set opponent flag
        card.PerformOpponent = false;
    }
}
