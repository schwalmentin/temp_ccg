using System;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DuplicateCardServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card)
    {
        // Create a copy token
        int freeFields = serverEngine.ServerData[playerId].Field.Cast<Card>().Count(fieldCard => fieldCard == null);

        Vector2Int position = Vector2Int.zero;
        int uniqueId = 0;
        int id = 0;

        if (freeFields > 0)
        {
            int randomField = Random.Range(0, freeFields);
            uniqueId = serverEngine.GetUniqueId();
            id = card.Id;

            position = this.SetCard(randomField, serverEngine.ServerData[playerId].Field, uniqueId, id);
        }
        
        // Prepare action params for client
        DuplicateCardParams duplicateCardParams = new DuplicateCardParams(position, id, uniqueId);
        string jsonParams = JsonUtility.ToJson(duplicateCardParams);
        card.ActionParams = jsonParams;
        
        // Set opponent flag
        card.PerformOpponent = true;
    }
    
    private Vector2Int SetCard(int randomField, Card[,] field, int uniqueId, int id)
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
                
                field[x,y] = DatabaseManager.Instance.GetCardById(id, uniqueId);
                field[x, y].CardState = CardState.Field;
                return new Vector2Int(x, y);
            }
        }

        throw new Exception("This should not be possible to trigger!");
    }
}
