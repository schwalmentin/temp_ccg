using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.CloudSave.Models;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class ServerHub : MonoBehaviour
{

    public Dictionary<ulong, Player> players = new Dictionary<ulong, Player>();
    private int turnCount = 0;

    private Dictionary<Vector2Int, Card> invaderField = new Dictionary<Vector2Int, Card>();
    
    private Dictionary<Vector2Int, Card> wardenField = new Dictionary<Vector2Int, Card>();

    private Dictionary<Vector2Int, Card> captureField = new Dictionary<Vector2Int, Card>();

    private List<Card> defeatedGuards = new List<Card>();

    private PlayedCard[] playedCardsWarden;

    private PlayedCard[] playedCardsInvader;

    private int laneToAttack;

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

    private Card GetGuardToAttack()
    {
        for (int x = 0; x < 6; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Card card = this.wardenField[new Vector2Int(x, y)];

                if (card == null) { continue; }
                if (defeatedGuards.Contains(card)) { continue; }
                return card;
            }

        }
        return null;
    }

    private Dictionary<Vector2Int, Card> GetFieldByCard(Card card)
    {
        return card.GetType() == typeof(Nightmare) ? this.invaderField :
            card.GetType() == typeof(Guardian) ? this.wardenField : this.captureField;
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

    private void EndTurnDeployment(ulong playerId, PlayedCard[] playedCards)
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

    private void InformAboutLane(int attackedLane)
    {
        Card card = GetGuardToAttack();
        if (card != null)
        {
            EventManager.Instance.InformAboutLaneClientRpc(attackedLane, card);
            return;
        }
        this.EndCombat(true);
    }

    // Inform Combat

    private void InformCombat(bool hasAttacked, Card nightmare, Card attackedGuard, Card newGuard)
    {
        Player defender = players.FirstOrDefault(x => !x.Value.isInvader).Value;
        EventManager.Instance.InformCombatClientRpc(hasAttacked, nightmare, attackedGuard, newGuard);
    }

    private void EndCombat(bool successfulAttack)
    {
        EventManager.Instance.EndCombatClientRpc(successfulAttack);
        defeatedGuards.Clear();
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

        if (this.players[playerId].isInvader) { this.playedCardsInvader = playedCards; } else { this.playedCardsWarden = playedCards; }

        foreach (PlayedCard playedCard in playedCards)
        {
            Dictionary<Vector2Int, Card> field = GetFieldByCard(playedCard.card);
            field.Add(playedCard.fieldPosition, MappingManager.Instance.CreateCard(playedCard.card.CardId, false));
        }

        Player player = this.players[playerId];
        player.state = PlayerState.COMBAT;

        if (this.players[GetOpponent(playerId)].state == PlayerState.COMBAT)
        {
            EndTurnDeployment(playerId, playedCards);
        }
    }

    private void ChooseLaneToAttack(int laneToAttack, ServerRpcParams serverRpcParams)
    {
        InformAboutLane(laneToAttack);
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