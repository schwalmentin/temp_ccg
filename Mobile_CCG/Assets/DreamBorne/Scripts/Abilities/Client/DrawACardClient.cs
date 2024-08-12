using System;
using UnityEngine;

public class DrawACardClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent)
    {
        try
        {
            // Get params
            DrawCardParams drawCardParams = JsonUtility.FromJson<DrawCardParams>(jsonParams);
            
            // Draw card
            playerEngine.DrawCard(drawCardParams);
            Logger.LogAction("Draw a card!");
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
