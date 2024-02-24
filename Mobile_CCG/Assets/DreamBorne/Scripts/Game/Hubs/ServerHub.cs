using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class ServerHub : MonoBehaviour
{

    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
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

    private ulong GetOpponent(ulong playerId)
    {
        return this.players.FirstOrDefault(x => x.Key != playerId).Key;
    }

    #region EventManager Invokations

    private void JoinedMatch(ulong playerId)
    {
        Card[] startingHand = new Card[4];

        for (int i = 0; i < startingHand.Length; i++)
        {
            Card card = this.players[playerId].library.Pop();
            this.players[playerId].hand.Add(card);
            startingHand[i] = card;
        }
        Debug.Log($"Inform player they joined");
        EventManager.Instance.JoinedMatchClientRpc(startingHand, this.players[playerId].rpcParams);
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

    private void InformAboutLane(int attackedLane, Card guardToAttack)
    {
        Player defender = players.FirstOrDefault(x => !x.Value.isInvader).Value;
        EventManager.Instance.InformAboutLaneClientRpc(attackedLane, guardToAttack);
    }

    // Inform Combat

    private void InformCombat(bool hasAttacked, Card nightmare, Card attackedGuard, Card newGuard)
    {
        Player defender = players.FirstOrDefault(x => !x.Value.isInvader).Value;
        EventManager.Instance.InformCombatClientRpc(hasAttacked, nightmare, attackedGuard, newGuard);
    }

    // End Turn Combat

    private void EndTurnCombat(Card cardToDraw, int earnedAttackerPoitns, int earnedDefenderPoints)
    {
        foreach (Player player in players.Values)
        {
            EventManager.Instance.EndTurnCombatClientRpc(cardToDraw, earnedAttackerPoitns, earnedDefenderPoints);
        }
    }

    private void MatchResult(bool hasWon)
    {
        
    }

    #endregion

    #region EventManager Observation

    private void JoinMatch(Card[] deck, bool isInvader, ServerRpcParams serverRpcParams)
    {
        // Get player id
        ulong playerId = serverRpcParams.Receive.SenderClientId;

        if(players.ContainsKey(playerId) || players.Count >= 2) { return; }

        // Get player data
        Stack<Card> library = new Stack<Card>();
        deck.ToList().ForEach(card => { library.Push(MappingManager.Instance.CreateCard(card.CardId, false)); });

        ClientRpcParams clientRpcParams = new ClientRpcParams 
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { playerId }
            }
        };

        // Join player
        Player player = new Player(
            isInvader,
            deck,
            library,
            new Stack<Card>(),
            new List<Card>(),
            clientRpcParams,
            PlayerState.DEPOLYMENT,
            0);

        this.players.Add(playerId, player);
        Debug.Log($"{this.players.Count} currently in the match!");

        // Check
        if (this.players.Count == 2)
        {
            this.JoinedMatch(playerId);
            this.JoinedMatch(this.GetOpponent(playerId));
        }
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

    public Player(
        bool isInvader,
        Card[] deck,
        Stack<Card> library,
        Stack<Card> graveyard,
        List<Card> hand,
        ClientRpcParams rpcParams,
        PlayerState state,
        int scorePoints)
    {
        this.isInvader = isInvader;
        this.deck = deck;
        this.library = library;
        this.graveyard = graveyard;
        this.hand = hand;
        this.rpcParams = rpcParams;
        this.state = state;
        this.scorePoints = scorePoints;
    }
}

public enum PlayerState
{
    DEPOLYMENT,
    COMBAT
}