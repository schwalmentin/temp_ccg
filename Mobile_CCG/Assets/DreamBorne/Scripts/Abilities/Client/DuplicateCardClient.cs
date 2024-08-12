using System;
using UnityEngine;

public class DuplicateCardClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent)
    {
        try
        {
            // Get params
            DuplicateCardParams createSheepParams = JsonUtility.FromJson<DuplicateCardParams>(jsonParams);
            
            // Place card
            if (createSheepParams.id == 0) return;
            
            Card card = DatabaseManager.Instance.GetCardById(createSheepParams.id, createSheepParams.uniqueId);
            card.CardState = CardState.Field;

            CardSlot[,] field = isOpponent ? playerEngine.PlayerData.OpponentField : playerEngine.PlayerData.PlayerField;
            CardSlot cardSlot = field[createSheepParams.position.x, createSheepParams.position.y];
            
            cardSlot.Card = card;
            card.transform.position = cardSlot.transform.position;
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
public struct DuplicateCardParams
{
    public Vector2Int position;
    public int id;
    public int uniqueId;

    public DuplicateCardParams(Vector2Int position, int id, int uniqueId)
    {
        this.position = position;
        this.id = id;
        this.uniqueId = uniqueId;
    }
}
