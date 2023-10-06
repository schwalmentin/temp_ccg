using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    private NetworkVariable<List<Player>> players;

    public void JoinAsPlayer(Player player)
    {
        // this.players.Value.Add(player);
    }
    private void Awake()
    {
        players = new NetworkVariable<List<Player>>();
        players.Value = new List<Player>();
        players.Value.Add(new Player("name 1", 0));
    }

    private void Update()
    {
        print("------------------------------------------------");
        foreach (var player in this.players.Value)
        {
            print(player.Username);
        }
    }
}
