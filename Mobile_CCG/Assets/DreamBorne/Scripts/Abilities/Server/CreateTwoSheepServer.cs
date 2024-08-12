using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreateTwoSheepServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card)
    {
        // Create two sheep tokens
        int freeFields = serverEngine.ServerData[playerId].Field.Cast<Card>().Count(fieldCard => fieldCard == null);

        List<Vector2Int> positions = new List<Vector2Int>();
        List<int> uniqueIds = new List<int>();
        
        for (int i = 0; i < 2; i++)
        {
            if (freeFields < 1) break;

            int randomField = Random.Range(0, freeFields);
            int uniqueId = serverEngine.GetUniqueId();
            
            positions.Add(this.SetSheep(randomField, serverEngine.ServerData[playerId].Field, uniqueId));
            uniqueIds.Add(uniqueId);

            freeFields--;
        }
        
        // Prepare action params for client
        CreateSheepParams createSheepParams = new CreateSheepParams(positions.ToArray(), uniqueIds.ToArray());
        string jsonParams = JsonUtility.ToJson(createSheepParams);
        card.ActionParams = jsonParams;
        
        // Set opponent flag
        card.PerformOpponent = true;
    }

    private Vector2Int SetSheep(int randomField, Card[,] field, int uniqueId)
    {
        int currentField = 0;

        for (int x = 0; x < field.GetLength(0); x++)
        {
            for (int y = 0; y < field.GetLength(1); y++)
            {
                if (field[x,y] != null) continue;

                if (currentField != randomField)
                {
                    currentField++;
                    continue;
                }
                
                field[x,y] = DatabaseManager.Instance.GetCardById(16, uniqueId);
                field[x, y].CardState = CardState.Field;
                return new Vector2Int(x, y);
            }
        }

        throw new Exception("This should not be possible to trigger!");
    }
}
