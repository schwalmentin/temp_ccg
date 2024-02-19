using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EventManager : NetworkSingleton<EventManager>
{
    #region Delegates

    public delegate void JoinMatchDelegate(uint[] deckIds, ServerRpcParams serverRpcParams);
    public delegate void JoinedMatchDelegate(uint[] startingHandUniqueIds);

    public delegate void PassTurnDeploymentDelegate(PlayedCard[] playedCards, ServerRpcParams serverRpcParams);

    public delegate void EndTurnDeploymentDelegate(PlayedCard[] playedCardsOpponent);


    #endregion

    #region Player Events

    public event PassTurnDeploymentDelegate p_passTurnDeploymentEvent;
    public event JoinMatchDelegate p_joinMatchEvent;

    #endregion

    #region Server Events

    public event EndTurnDeploymentDelegate s_endTurnDeploymentEvent;
    public event JoinedMatchDelegate s_joinedMatchEvent;

    #endregion

    #region Server RPCs

    [ServerRpc(RequireOwnership = false)]
    public void PassTurnDeploymentServerRpc(PlayedCard[] playedCards, ServerRpcParams serverRpcParams = default)
    {
        this.p_passTurnDeploymentEvent.Invoke(playedCards, serverRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void JoinMatchServerRpc(uint[] deckIds, ServerRpcParams serverRpcParams = default)
    {
        this.p_joinMatchEvent.Invoke(deckIds, serverRpcParams);
    }

    #endregion

    #region Client RPCs

    [ClientRpc]
    public void EndTurnClientRpc(PlayedCard[] playedCardsOpponent, ClientRpcParams clientRpcParams)
    {
        this.s_endTurnDeploymentEvent.Invoke(playedCardsOpponent);
    }

    [ClientRpc]
    public void JoinedMatchClientRpc(uint[] startingHandUniqueIds, ClientRpcParams clientRpcParams)
    {
        this.s_joinedMatchEvent.Invoke(startingHandUniqueIds);
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
