using Unity.Netcode;
using UnityEngine;

public class EventManager : NetworkSingleton<EventManager>
{
    #region Delegates

    public delegate void PlayerDataDelegate(ulong playerId, string playerName);

    public delegate void DefaultDelegeate();
    public delegate void PlayerIdDelegate(ulong playerId);
    public delegate void PlayerJoin(string playerName, bool isStarting);

    public delegate void AmountDelegate(int amount);
    public delegate void PlayerAmountDelegate(ulong palyerId, int amount);

    public delegate void HardResetDelegate(string playerName, string opponentName, int playerAmount, int opponentAmount, bool yourTurn, int currentTurn);

    public delegate void EndGameDelegate(bool isWinning);

    #endregion

    #region GameMaster Events

    public event PlayerDataDelegate joinGameMasterEvent;

    public event PlayerIdDelegate passTurnEvent;

    public event PlayerAmountDelegate increaseCurrencyEvent;

    #endregion

    #region PlayerView Events

    public event PlayerJoin updClientJoinedGameMaster;

    public event HardResetDelegate updClientHardResetEvent;

    public event DefaultDelegeate updClientPassTurnEvent;

    public event AmountDelegate updClientCurrencyEvent;

    public event EndGameDelegate updEndGameEvent;

    #endregion

    #region Server RPCs

    [ServerRpc(RequireOwnership = false)]
    public void JoinGameMasterServerRpc(ulong playerId, string playerName)
    {
        this.joinGameMasterEvent?.Invoke(playerId, playerName);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PassTurnServerRpc(ulong playerId)
    {
        passTurnEvent?.Invoke(playerId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void IncreaseCurrencyServerRpc(ulong playerId, int amount)
    {
        Debug.Log("EventManager invokes the increaseCurrency process!");
        increaseCurrencyEvent?.Invoke(playerId, amount);
    }

    #endregion

    #region Client RPCs

    [ClientRpc]
    public void JoinGameMasterClientRpc(string opponentName, bool isStarting, ClientRpcParams clientRpcParams = default)
    {
        this.updClientJoinedGameMaster?.Invoke(opponentName, isStarting);
    }

    [ClientRpc]
    public void HardResetPlayerClientRpc(string playerName, string opponentName, int playerAmount, int opponentAmount, bool yourTurn, int currentTurn, ClientRpcParams clientRpcParams = default)
    {
        this.updClientHardResetEvent?.Invoke(playerName, opponentName, playerAmount, opponentAmount, yourTurn, currentTurn);
    }

    [ClientRpc]
    public void PassTurnClientRpc(ClientRpcParams clientRpcParams = default)
    {
        this.updClientPassTurnEvent?.Invoke();
    }

    [ClientRpc]
    public void UpdateOpponentCurrencyClientRpc(int amount, ClientRpcParams clientRpcParams = default)
    {
        this.updClientCurrencyEvent?.Invoke(amount);
    }

    [ClientRpc]
    public void EndGameClientRpc(bool isWinning, ClientRpcParams clientRpcParams = default)
    {
        this.updEndGameEvent?.Invoke(isWinning);
    }

    #endregion

    #region Debug RPCs

    [ServerRpc(RequireOwnership = false)]
    public void LogMessageServerRpc(string message)
    {
        Debug.Log("CLIENT: " + message);
    }

    [ClientRpc]
    public void LogMessageClientRpc(string message)
    {
        Debug.Log("SERVER: " + message);
    }


    #endregion
}
