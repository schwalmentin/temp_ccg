using System;
using System.Linq;
using UnityEngine;

public class SacrificeUnitsClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent)
    {
        try
        {
            // Get params
            SacrificeUnitsParams sacrificeUnitsParams = JsonUtility.FromJson<SacrificeUnitsParams>(jsonParams);
            
            // Place card
            CardSlot[,] field = isOpponent ? playerEngine.PlayerData.OpponentField : playerEngine.PlayerData.PlayerField;
            Card card = field.Cast<CardSlot>().ToList().Find(x => x.Card?.UniqueId == sacrificeUnitsParams.uniqueId).Card;

            foreach (CardSlot cardSlot in field)
            {
                if (cardSlot.Card == null || cardSlot.Card == card) continue;
                
                cardSlot.Card.gameObject.SetActive(false);
                cardSlot.Card = null;
            }

            card.Power = sacrificeUnitsParams.power;
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
public struct SacrificeUnitsParams
{
    public int power;
    public int uniqueId;

    public SacrificeUnitsParams(int power, int uniqueId)
    {
        this.power = power;
        this.uniqueId = uniqueId;
    }
}
