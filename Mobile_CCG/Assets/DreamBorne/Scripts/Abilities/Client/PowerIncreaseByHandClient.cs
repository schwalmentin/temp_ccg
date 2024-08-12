using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PowerIncreaseByHandClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent)
    {
        try
        {
            // Get params
            PowerIncreaseByHandParams powerIncreaseByHandParams = JsonUtility.FromJson<PowerIncreaseByHandParams>(jsonParams);
            
            // Get card
            Card card = null;
            

            card = this.FindCardById(powerIncreaseByHandParams.uniqueId, playerEngine.PlayerData.PlayerField);

            if (card == null)
            {
                card = this.FindCardById(powerIncreaseByHandParams.uniqueId, playerEngine.PlayerData.OpponentField);
                
                if (card == null) return;
            }
            
            // Increase power
            card.Power += powerIncreaseByHandParams.handAmount;
        }
        catch (ArgumentException e)
        {
            Logger.LogError(e.Message);
            // Request a server update of the current game state
            // playerEngine.HardReset
            throw;
        }
    }

    private Card FindCardById(int uniqueId, CardSlot[,] field)
    {
        foreach (CardSlot cardSlot in field)
        {
            Card card = cardSlot.Card;
                
            if (!card) continue;
                
            if (card.UniqueId == uniqueId) return card;
        }
        
        return null;
    }
}

[System.Serializable]
public struct PowerIncreaseByHandParams
{
    public int uniqueId;
    public int handAmount;

    public PowerIncreaseByHandParams(int uniqueId, int handAmount)
    {
        this.uniqueId = uniqueId;
        this.handAmount = handAmount;
    }
}