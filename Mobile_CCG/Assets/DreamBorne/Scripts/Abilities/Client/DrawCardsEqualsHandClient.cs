using System;
using UnityEngine;

public class DrawCardsEqualsHandClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent)
    {
        try
        {
            // Get params
            DrawMultipleCardsParams drawMultipleCardParams = JsonUtility.FromJson<DrawMultipleCardsParams>(jsonParams);
            
            // Draw card
            foreach (DrawCardParams drawCardParams in drawMultipleCardParams.drawCardParams)
            {
                playerEngine.DrawCard(drawCardParams);
            }
            
            Logger.LogAction("Draw cards equals hand!");
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
