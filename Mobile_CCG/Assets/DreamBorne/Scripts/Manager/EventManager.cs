using Unity.Netcode;
using UnityEngine;

public class EventManager : NetworkSingleton<EventManager>
{
    #region Delegates

    public delegate void PassTurnDelegate(ulong clientId, int cardId, Vector2 cardPosition);

    #endregion

    #region ServerHub Events

    public event PassTurnDelegate passTurnEvent;

    #endregion

    #region PlayerHub Events

    #endregion

    #region Server RPCs

    [ServerRpc(RequireOwnership = false)]
    public void PassTurnServerRpc(int cardId, Vector2 cardPosition, ServerRpcParams serverRpcParams = default)
    {
        this.passTurnEvent.Invoke(serverRpcParams.Receive.SenderClientId, cardId, cardPosition);
    }

    #endregion

    #region Client RPCs

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
