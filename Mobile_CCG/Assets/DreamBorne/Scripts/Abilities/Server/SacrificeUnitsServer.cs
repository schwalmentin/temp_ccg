using UnityEngine;

public class SacrificeUnitsServer : IActionServer
{
    public void Execute(ServerEngine serverEngine, ulong playerId, Card card)
    {
        // Sacrifice units
        int sacrificedUnits = 0;
        Card[,] field = serverEngine.ServerData[playerId].Field;
        
        for (int x = 0; x < field.GetLength(0); x++)
        {
            for (int y = 0; y < field.GetLength(1); y++)
            {
                if (field[x,y] == null || field[x,y] == card) continue;

                sacrificedUnits++;
                field[x, y] = null;
            }
        }

        card.Power += sacrificedUnits * 4;
        
        // Prepare action params for client
        SacrificeUnitsParams sacrificeUnitsParams = new SacrificeUnitsParams(card.Power, card.UniqueId);
        string jsonParams = JsonUtility.ToJson(sacrificeUnitsParams);
        card.ActionParams = jsonParams;
        
        // Set opponent flag
        card.PerformOpponent = true;
    }
}
