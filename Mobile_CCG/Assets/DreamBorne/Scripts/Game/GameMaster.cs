using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    #region Variables

    private int maxTurnCount = 9;
    private int turnCount;
    
    private Dictionary<ulong, PlayerData> playerData;
    private Dictionary<ulong, ClientRpcParams> clientRpcParams;

    private ulong currentPlayerId;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            Destroy(this.gameObject);
            return;
        }

        this.playerData = new Dictionary<ulong, PlayerData>();
        this.clientRpcParams = new Dictionary<ulong, ClientRpcParams>();

        EventManager.Instance.joinGameMasterEvent += this.JoinGameMaster;
        EventManager.Instance.passTurnEvent += this.PassTurn;
        EventManager.Instance.increaseCurrencyEvent += this.IncreaseCurrency;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            EventManager.Instance.LogMessageClientRpc("Hello World!");
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.joinGameMasterEvent -= this.JoinGameMaster;
        EventManager.Instance.passTurnEvent -= this.PassTurn;
        EventManager.Instance.increaseCurrencyEvent -= this.IncreaseCurrency;
    }

    #endregion

    #region GameMaster Methods

    /// <summary>
    /// Returns the playerId of the another player of a given playerId
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns></returns>
    private ulong GetOtherPlayer(ulong playerId)
    {
        if (this.playerData.Count == 1)
        {
            Debug.LogWarning("There is only one player in the game!");
            return playerId;
        }

        if (this.playerData.Count > 2)
        {
            Debug.LogWarning("There are more than 2 players in the game!");
        }

        foreach (ulong item in playerData.Keys)
        {
            if (item != playerId)
            {
                return item;
            }
        }

        return playerId;
    }

    /// <summary>
    /// Completely resets the status of a player.
    /// </summary>
    /// <param name="playerId"></param>
    private void HardResetPlayer(ulong playerId)
    {
        EventManager.Instance.HardResetPlayerClientRpc(
            playerData[playerId].PlayerName,
            playerData[GetOtherPlayer(playerId)].PlayerName,
            playerData[playerId].CashAmount,
            playerData[GetOtherPlayer(playerId)].CashAmount,
            false,
            this.turnCount,
            clientRpcParams[playerId]);
    }

    /// <summary>
    /// Informs the players the game has ended and closes the lobby.
    /// </summary>
    private void EndGame()
    {
        ulong otherPlayerId = this.GetOtherPlayer(this.currentPlayerId);
        int currentPlayerCash = this.playerData[this.currentPlayerId].CashAmount;
        int otherPlayerCash = this.playerData[otherPlayerId].CashAmount;

        EventManager.Instance.EndGameClientRpc(currentPlayerCash >= otherPlayerCash, this.clientRpcParams[this.currentPlayerId]);
        EventManager.Instance.EndGameClientRpc(otherPlayerCash >= currentPlayerCash, this.clientRpcParams[otherPlayerId]);
        LobbyManager.Instance.CloseMatch();
    }

    #endregion

    #region Incoming Events

    /// <summary>
    /// Informs the GameMaster that a certain player wants to join the game.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="playerName"></param>
    private void JoinGameMaster(ulong playerId, string playerName)
    {
        if (this.playerData.ContainsKey(playerId))
        {
            Debug.LogWarning($"Player with the player ID {playerId} does already exist");
            return;
        }

        this.clientRpcParams.Add(playerId, new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerId }
            }
        });
        this.playerData.Add(playerId, new PlayerData(playerName));

        if (this.playerData.Count < 2)
        {
            return;
        }

        ulong otherPlayerId = this.GetOtherPlayer(playerId);
        string otherPlayerName = this.playerData[otherPlayerId].PlayerName;

        Debug.Log($"playerId = {playerId} | otherPlayerId = {otherPlayerId}");
        this.currentPlayerId = playerId;
        EventManager.Instance.JoinGameMasterClientRpc(otherPlayerName, true, this.clientRpcParams[playerId]);
        EventManager.Instance.JoinGameMasterClientRpc(playerName, false, this.clientRpcParams[otherPlayerId]);
    }

    /// <summary>
    /// Informs the GameMaster that a certain player wants to pass their turn.
    /// </summary>
    /// <param name="playerId"></param>
    private void PassTurn(ulong playerId)
    {
        // Check if the player is authorized
        if (playerId != currentPlayerId)
        {
            Debug.LogWarning($"Wrong player with id {playerId} tried to pass the turn!");
            this.HardResetPlayer(playerId);
            return;
        }

        // Pass the turn
        this.currentPlayerId = GetOtherPlayer(playerId);

        // Increase turn count
        this.turnCount++;

        if (this.turnCount > this.maxTurnCount)
        {
            this.EndGame();
            return;
        }

        // Inform the other player
        EventManager.Instance.PassTurnClientRpc(this.clientRpcParams[this.currentPlayerId]);
    }

    /// <summary>
    /// Informs the GameMaster that a certain player wants to increase their currency.
    /// </summary>
    /// <param name="playerId"></param>
    /// <param name="amount"></param>
    private void IncreaseCurrency(ulong playerId, int amount)
    {
        // Check if the player is authorized
        if (playerId != currentPlayerId)
        {
            Debug.LogWarning($"Wrong player with id {playerId} tried to increase their currency! Current player has the id {this.currentPlayerId}!");
            this.HardResetPlayer(playerId);
            return;
        }

        // Check if the player exists
        if (!this.playerData.ContainsKey(playerId))
        {
            Debug.LogWarning($"The player with id {playerId} does not exist in the current game!");
            return;
        }

        // Increase the currency
        this.playerData[playerId].CashAmount += amount;

        // Inform the other player
        int cashAmount = this.playerData[this.currentPlayerId].CashAmount;
        ClientRpcParams rpcParams = this.clientRpcParams[this.GetOtherPlayer(this.currentPlayerId)];

        Debug.Log("The Gamemaster updates the opponents currency now!");
        EventManager.Instance.UpdateOpponentCurrencyClientRpc(cashAmount, rpcParams);
    }

    #endregion
}
