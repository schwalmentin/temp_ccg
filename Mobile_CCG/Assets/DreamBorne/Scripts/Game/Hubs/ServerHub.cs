using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class ServerHub : MonoBehaviour
{

    public Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();
    private int turnCount = 0;

    private void OnEnable()
    {
        EventManager.Instance.p_joinMatchEvent += this.JoinMatch;
        EventManager.Instance.p_passTurnDeploymentEvent += PassTurnDeployment;
        EventManager.Instance.p_ChooseLaneToAttackEvent += this.ChooseLaneToAttack;
        EventManager.Instance.p_combatEvent += this.Combat;
        EventManager.Instance.p_byPassGuardianEvent += this.BypassGuardian;
        EventManager.Instance.p_chooseInteractionEvent += this.ChooseInteraction; 

    }

    private void OnDisable()
    {
        EventManager.Instance.p_joinMatchEvent -= this.JoinMatch;
        EventManager.Instance.p_passTurnDeploymentEvent -= PassTurnDeployment;
        EventManager.Instance.p_ChooseLaneToAttackEvent -= this.ChooseLaneToAttack;
        EventManager.Instance.p_combatEvent -= this.Combat;
        EventManager.Instance.p_byPassGuardianEvent -= this.BypassGuardian;
        EventManager.Instance.p_chooseInteractionEvent -= this.ChooseInteraction;
    }

    #region EventManager Invokations

    private void JoinedMatch()
    {
        Card[] handCardsUniqueId = new Card[0]; // nur test
        ulong clientId = 0; // nur test

        ClientRpcParams clientRpcParams = new ClientRpcParams // @nico die sollten nicht jedes mal auf neue erstellt werden.. anstatt dessen oben dem dictionary (mehr infos siehe joinmatch) statisch zuweisen... 
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        EventManager.Instance.JoinedMatchClientRpc(handCardsUniqueId, clientRpcParams);

        Card[] startingHand = new Card[0];
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

        EventManager.Instance.EndTurnDeploymentClientRpc(opponentPlayedCards, clientRpcParams);
    }

    // Inform About Lane

    // Inform Combat

    // End Turn Combat

    private void MatchResult()
    {
        
    }

    #endregion

    #region EventManager Observation

    private void JoinMatch(Card[] deck, ServerRpcParams serverRpcParams)
    {
        // @nico am besten oben ein player objekt mit allen infos wie deck etc (musst neu erstellen glaub ich) in einem Dictionary der ulong id des spielers erstellen

        // validate player and add to player list
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        // validate deck and add to player deck

        if(Players .ContainsKey(playerId)) { }
    }

    private void PassTurnDeployment(PlayedCard[] playedCards, ServerRpcParams serverRpcParams)
    {
        // validate player id
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        // validate played cards and add to battlefield
    }

    private void ChooseLaneToAttack(int laneToAttack, ServerRpcParams serverRpcParams)
    {
        //EventManager.ChooseLaneToAttackLane(laneToAttack)
    }

    private void Combat(Card nightmare, ServerRpcParams serverRpcParams)
    {

    }

    private void BypassGuardian(ServerRpcParams serverRpcParams)
    {

    }

    private void ChooseInteraction(bool interactWithHiddenCard, ServerRpcParams serverRpcParams)
    {

    }

    #endregion
}

public struct Player
{
    public bool isInvader;
    public Card[] deck;
    public Stack<Card> library;
    public Stack<Card> graveyard;
    public List<Card> hand;
    public ClientRpcParams rpcParams;
    public PlayerState state;
    public int scorePoints;
}

public enum PlayerState
{
    DEPOLYMENT,
    COMBAT
}