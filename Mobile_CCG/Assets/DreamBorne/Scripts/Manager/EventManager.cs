using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EventManager : NetworkSingleton<EventManager>
{
    #region Delegates

    public delegate void PassTurnDeploymentDelegate(Dictionary<Vector2Int, uint> playedCards, ServerRpcParams serverRpcParams);

    public delegate void EndTurnDeploymentDelegate(Dictionary<Vector2Int, uint> playedCards);


    #endregion

    #region ServerHub Events

    public event PassTurnDeploymentDelegate s_passTurnDeploymentEvent;

    #endregion

    #region PlayerHub Events

    public event EndTurnDeploymentDelegate p_endTurnDeploymentEvent;

    #endregion

    #region Server RPCs

    //[ServerRpc(RequireOwnership = false)]
    //public void PassTurnDeploymentServerRpc(Dictionary<Vector2Int, uint> playedCards, ServerRpcParams serverRpcParams = default)
    //{
    //    this.s_passTurnDeploymentEvent.Invoke(playedCards, serverRpcParams);
    //}

    #endregion

    #region Client RPCs

    //[ClientRpc]
    //public void EndTurnClientRpc(Dictionary<Vector2Int, uint> playedCard)
    //{
    //    this.p_endTurnDeploymentEvent.Invoke(playedCard);
    //}

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
