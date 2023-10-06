using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : MonoBehaviour
{
    private string username;
    private int currency;
    [SerializeField] private TurnManager turnManager;

    public int Currency { get { return this.currency; } }
    public string Username { get { return this.username; } }

    public Player(string username, int currency)
    {
        this.username = username;
        this.currency = currency;
    }

    private void Start()
    {
        turnManager.JoinAsPlayer(this);
    }
}
