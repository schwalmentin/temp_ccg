using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Random = System.Random;

public class ServerData
{
    public Stack<int> Library { get; private set; }
    public string Name { get; private set; }
    public List<Card> Hand { get; private set; }
    public Card[,] Field { get; private set; }
    public ClientRpcParams ClientRpcParams { get; private set; }

    public int Points { get; set; }
    public int Turn { get; set; }
    public int Mana { get; set; }
    
    public ServerData(string name, ulong playerId, int[] deckIds)
    {
        Random rnd = new Random();
        List<int> deckList = deckIds.ToList();
        this.Library = new Stack<int>(deckList.OrderBy(x => rnd.Next()));
        
        this.Name = name;
        this.Hand = new List<Card>();
        this.Field =  new Card[2, 3];
        this.ClientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerId }
            }
        };
        
        this.Points = 0;
        this.Turn = 1;
        this.Mana = 1;
    }
}
