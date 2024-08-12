using System;
using UnityEngine;

public class DrawThreeCardsClient : IActionClient
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
            
            Logger.LogAction("Draw three cards!");
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
public struct DrawMultipleCardsParams
{
    public DrawCardParams[] drawCardParams;
    
    public DrawMultipleCardsParams(DrawCardParams[] drawCardParams)
    {
        this.drawCardParams = drawCardParams;
    }
}
