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

    private int laneToAttack;
    private PlayedCard currentGuard;

    private void Awake()
    {
        this.SetField(new Vector2Int(6, 2), this.invaderField);
        this.SetField(new Vector2Int(6, 4), this.wardenField);
        this.SetField(new Vector2Int(3, 1), this.captureField);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 6; x++)
                {
                    print(wardenField[new Vector2Int(x, y)]);
                }
                print("----------:----------");
            }
        }
    }

    private void SetField(Vector2Int size, Dictionary<Vector2Int, Card> field)
    {
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                field[new Vector2Int(x, y)] = null;
            }
        }
    }

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

    private PlayedCard GetGuardToAttack(int lane)
    {
        int minX = (lane - 1) * 2;
        int maxX = minX + 1;

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = 0; y < 4; y++)
            {
                Card card = this.wardenField[new Vector2Int(x, y)];

                if (card == null) { continue; }
                if (defeatedGuards.Contains(card)) { continue; }
                return new PlayedCard(new Vector2Int(x,y), 0, card.CardId);
            }
        }
        return new PlayedCard(new Vector2Int(0, 0), 0, 0);
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
        
        EventManager.Instance.JoinedMatchClientRpc(startingHand, this.players[playerId].rpcParams);
    }

    private void EndTurnDeployment(ulong playerId, PlayedCard[] opponentPlayedCards)
    {
        EventManager.Instance.EndTurnDeploymentClientRpc(opponentPlayedCards, this.players[playerId].rpcParams);
    }

    // Inform About Lane

    private void InformAboutLane(int attackedLane)
    {
        PlayedCard playedCard = GetGuardToAttack(attackedLane);
        this.currentGuard = playedCard;

        if (playedCard.cardId != 0)
        {
            EventManager.Instance.InformAboutLaneClientRpc(attackedLane, playedCard);
            return;
        }

        this.EndCombat(true);
    }

    // Inform Combat

    private void InformCombat(bool hasAttacked, PlayedCard nightmare, PlayedCard attackedGuard, PlayedCard newGuard)
    {
        this.currentGuard = newGuard;
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
            0,
            new PlayedCard[0]);

        this.players.Add(playerId, player);
        Debug.Log($"{this.players.Count} players joined the match!");

        // Check
        if (this.players.Count == 2)
        {
            this.JoinedMatch(playerId);
            this.JoinedMatch(this.GetOpponent(playerId));
        }
    }

    private void PassTurnDeployment(PlayedCard[] playedCards, ServerRpcParams serverRpcParams)
    {
        // Get player and opponent id
        ulong playerId = serverRpcParams.Receive.SenderClientId;
        ulong opponentPlayerId = GetOpponent(playerId);

        Player player = this.players[playerId];
        Player opponent = this.players[opponentPlayerId];

        // Set played cards and player state for palyer
        player.playedCards = this.CopyPlayedCards(playedCards).ToArray();
        player.state = PlayerState.COMBAT;

        ////// Play cards on the battlefield
        ////for (int i = 0; i < playedCards.Length; i++)
        ////{
        ////    Card card = MappingManager.Instance.CreateCard(playedCards[i].card.CardId, false);
        ////    Vector2Int fieldPosition = playedCards[i].fieldPosition;
        ////    int cardSlotAmount = playedCards[i].cardSlotAmount;

        ////    player.playedCards[i] = new PlayedCard(card, fieldPosition, cardSlotAmount);

        ////    Dictionary<Vector2Int, Card> field = GetFieldByCard(card);
        ////    field.Add(playedCards[i].fieldPosition, card);

        ////}

        // End turn if both players are ready
        if (opponent.state == PlayerState.COMBAT)
        {
            EndTurnDeployment(playerId, this.CopyPlayedCards(opponent.playedCards).ToArray());
            EndTurnDeployment(opponentPlayerId, this.CopyPlayedCards(playedCards).ToArray());
            Debug.Log("2 Player passed the turn!");
            return;
        }

        Debug.Log("1 Player passed the turn!");
    }

    private List<PlayedCard> CopyPlayedCards(PlayedCard[] playedCardsO)
    {
        // Set played cards and player state for palyer
        var playedCards = new List<PlayedCard>();

        // Play cards on the battlefield
        for (int i = 0; i < playedCardsO.Length; i++)
        {
            Card card = MappingManager.Instance.CreateCard(playedCardsO[i].cardId, false);
            Vector2Int fieldPosition = playedCardsO[i].fieldPosition;
            int cardSlotAmount = playedCardsO[i].cardSlotAmount;

            playedCards.Add(new PlayedCard(fieldPosition, cardSlotAmount, card.CardId));

            // field[playedCards[i].fieldPosition] = card;
            this.PlacePrivateCard(card, playedCards[i].fieldPosition);
        }

        return playedCards;      
    }

    private void PlacePrivateCard(Card card, Vector2Int fieldPosition)
    {
        // Check if a card is played and a valid cardslot is selected
        if (card == null) { return; }

        // Get cards
        Dictionary<Vector2Int, Card> field = GetFieldByCard(card);
        Card[] cards = card.GetType() == typeof(Guardian) ? ((Guardian)card).Guards.ToArray() : new Card[] { card };

        // Place card on board

        for (int i = 0; i < cards.Length; i++)
        {
            Vector2Int placement = new Vector2Int(fieldPosition.x + i, fieldPosition.y);

            if (field[placement] != null)
            {
                // field[placement].gameObject.SetActive(false);
                // Move card into graveyard
            }

            field[placement] = cards[i];
            cards[i].CardState = CardState.Field;
        }
    }

    private void ChooseLaneToAttack(int laneToAttack, ServerRpcParams serverRpcParams)
    {
        Debug.Log($"Lane {laneToAttack} was successfully chosen!");
        InformAboutLane(laneToAttack);
        this.laneToAttack = laneToAttack;
    }

    private void Combat(Card nightmareR, ServerRpcParams serverRpcParams)
    {
        Debug.Log($"The invader attacked with the nightmare {nightmareR.CardName}!");
        Card nightmare = this.invaderField.Where(x => x.Value?.CardId == nightmareR.CardId)?.FirstOrDefault().Value;

        if (nightmare == null)
        {
            Debug.LogError("Fischer fritzt fische frische!");
            return;
        }

        // Combat
        Card guard = this.wardenField[this.currentGuard.fieldPosition];

        if (guard == null)
        {
            Debug.LogError("Why is guard null HUH?");
        }

        int nightmareDamage = ((Nightmare)nightmare).Power;
        int guardDamage = ((Guard)guard).Power;
        nightmare.GetDamage(guardDamage);
        guard.GetDamage(nightmareDamage);

        if (((Nightmare)nightmare).Power <= 0)
        {
            // Delete nightmare
            this.invaderField[this.invaderField.Where(x => x.Value == nightmare).FirstOrDefault().Key] = null;
        }

        if (((Guard)guard).Power <= 0)
        {
            // Delete guard
            this.wardenField[this.wardenField.Where(x => x.Value == guard).FirstOrDefault().Key] = null;
        }

        // TryInformingCombat
        PlayedCard playedNightmare = new PlayedCard(this.invaderField.FirstOrDefault(x => x.Value == nightmare).Key, 0, nightmare.CardId);
        this.TryInformingCombat(true, playedNightmare);
    }

    private void BypassGuardian(ServerRpcParams serverRpcParams)
    {
        Debug.Log($"The invader skipped the guardian");

        // Skipping
        this.defeatedGuards.Add(this.wardenField[this.currentGuard.fieldPosition]);

        // TryInformingCombat
        this.TryInformingCombat(true, new PlayedCard(new Vector2Int(0,0), 0, 0));
    }

    private void TryInformingCombat(bool hasAttacked, PlayedCard nightmare)
    {
        PlayedCard newGuard = this.GetGuardToAttack(this.laneToAttack);

        if (newGuard.cardId == 0)
        {
            this.EndCombat(true);
            return;
        }

        this.InformCombat(hasAttacked, nightmare, this.currentGuard, newGuard);
    }

    private void ChooseInteraction(bool interactWithHiddenCard, ServerRpcParams serverRpcParams)
    {
        if (interactWithHiddenCard)
        {
            Debug.Log("Interact with the Hidden Card");
        }
        else
        {
            Debug.Log("Interact with the Base");
        }

        foreach (var item in this.players)
        {
            EventManager.Instance.EndTurnCombatClientRpc(item.Value.library.Pop(), 0, 0, item.Value.rpcParams);
        }
    }

    #endregion
}

public class Player
{
    public bool isInvader;
    public Card[] deck;
    public Stack<Card> library;
    public Stack<Card> graveyard;
    public List<Card> hand;
    public ClientRpcParams rpcParams;
    public PlayerState state;
    public int scorePoints;
    public PlayedCard[] playedCards;

    public Player(
        bool isInvader,
        Card[] deck,
        Stack<Card> library,
        Stack<Card> graveyard,
        List<Card> hand,
        ClientRpcParams rpcParams,
        PlayerState state,
        int scorePoints,
        PlayedCard[] playedCards)
    {
        this.isInvader = isInvader;
        this.deck = deck;
        this.library = library;
        this.graveyard = graveyard;
        this.hand = hand;
        this.rpcParams = rpcParams;
        this.state = state;
        this.scorePoints = scorePoints;
        this.playedCards = playedCards;
    }
}

public enum PlayerState
{
    DEPOLYMENT,
    COMBAT
}