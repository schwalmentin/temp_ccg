using System;
using UnityEngine;

public class CreateTwoSheepClient : IActionClient
{
    public void Execute(PlayerEngine playerEngine, string jsonParams, bool isOpponent)
    {
        try
        {
            // Get params
            CreateSheepParams createSheepParams = JsonUtility.FromJson<CreateSheepParams>(jsonParams);
            
            // Place sheep
            for (int i = 0; i < createSheepParams.positions.Length; i++)
            {
                Card sheep = DatabaseManager.Instance.GetCardById(16, createSheepParams.uniqueIds[i]);
                sheep.CardState = CardState.Field;

                CardSlot[,] field = isOpponent ? playerEngine.PlayerData.OpponentField : playerEngine.PlayerData.PlayerField;
                CardSlot cardSlot = field[createSheepParams.positions[i].x, createSheepParams.positions[i].y];
                
                cardSlot.Card = sheep;
                sheep.transform.position = cardSlot.transform.position;
            }
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
public struct CreateSheepParams
{
    public Vector2Int[] positions;
    public int[] uniqueIds;

    public CreateSheepParams(Vector2Int[] positions, int[] uniqueIds)
    {
        this.positions = positions;
        this.uniqueIds = uniqueIds;
    }
}