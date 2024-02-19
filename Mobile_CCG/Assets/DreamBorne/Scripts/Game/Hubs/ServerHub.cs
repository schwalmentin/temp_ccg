using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class ServerHub : MonoBehaviour
{
    private void OnEnable()
    {
        EventManager.Instance.p_joinMatchEvent += this.JoinMatch;
        EventManager.Instance.p_passTurnDeploymentEvent += PassTurn;
    }

    private void OnDisable()
    {
        EventManager.Instance.p_joinMatchEvent -= this.JoinMatch;
        EventManager.Instance.p_passTurnDeploymentEvent -= PassTurn;
    }

    #region EventManager Invokations

    private void JoinedMatch()
    {
        uint[] handCardsUniqueId = new uint[0]; // nur test
        ulong clientId = 0; // nur test

        ClientRpcParams clientRpcParams = new ClientRpcParams // @nico die sollten nicht jedes mal auf neue erstellt werden.. anstatt dessen oben dem dictionary (mehr infos siehe joinmatch) statisch zuweisen... 
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        EventManager.Instance.JoinedMatchClientRpc(handCardsUniqueId, clientRpcParams);
    }

    private void EndTurnDeployment()
    {
        PlayedCard[] opponentPlayedCards = new PlayedCard[0]; // nur test
        ulong clientId = 0; // nur test

        ClientRpcParams clientRpcParams = new ClientRpcParams // @nico die sollten nicht jedes mal auf neue erstellt werden.. anstatt dessen oben dem dictionary (mehr infos siehe joinmatch) statisch zuweisen... 
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        EventManager.Instance.EndTurnClientRpc(opponentPlayedCards, clientRpcParams);
    }

    #endregion

    #region EventManager Observation

    private void JoinMatch(uint[] deckIds, ServerRpcParams serverRpcParams)
    {
        // @nico am besten oben ein player objekt mit allen infos wie deck etc (musst neu erstellen glaub ich) in einem Dictionary der ulong id des spielers erstellen

        // validate player and add to player list
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        // validate deck and add to player deck
    }

    private void PassTurn(PlayedCard[] playedCards, ServerRpcParams serverRpcParams)
    {
        // validate player id
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        // validate played cards and add to battlefield
    }

    #endregion
}
