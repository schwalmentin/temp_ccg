using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerEngine : MonoBehaviour
{
    #region Variables

        // Model Reference
        public PlayerData PlayerData { get; private set; }

        // Actions
        private Dictionary<string, IActionClient> actions;
        
        // Handcard Rendering
        [Header("Handcard Rendering")]
        [SerializeField] private Slider cardSlider;
        [SerializeField] private Transform handTransform;
        [FormerlySerializedAs("maxHandWidth")] [SerializeField] private float handRadius;
        [SerializeField] private float cardRadius;

    #endregion

    #region Unity Methods

        private void Awake()
        {
            this.PlayerData = FindObjectOfType<PlayerData>();

            this.actions = new Dictionary<string, IActionClient>();

            EventManager.Instance.s_startMatch += this.StartMatch;
            EventManager.Instance.s_syncPlayer += this.SyncPlayer;
            EventManager.Instance.s_syncOpponent += this.SyncOpponent;
            EventManager.Instance.s_endTurn += this.EndTurn;
            EventManager.Instance.s_endGame += this.EndGame;
        }
        
        private void Start()
        {
            // Join Match
            JoinMatchParams joinMatchParams = new JoinMatchParams(this.PlayerData.Deck, this.PlayerData.PlayerName);
            string jsonParams = JsonUtility.ToJson(joinMatchParams);
            EventManager.Instance.JoinMatchServerRpc(jsonParams);
        }

        private void OnDestroy()
        {
            EventManager.Instance.s_startMatch -= this.StartMatch;
            EventManager.Instance.s_syncPlayer -= this.SyncPlayer;
            EventManager.Instance.s_syncOpponent -= this.SyncOpponent;
            EventManager.Instance.s_endTurn -= this.EndTurn;
            EventManager.Instance.s_endGame -= this.EndGame;
        }

    #endregion

    #region Action Methods

        /// <summary>
        /// Invokes an action based on its corresponding id.
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="jsonParams"></param>
        private void InvokeAction(string actionId, string jsonParams, bool isOpponent)
        {
            if (!this.actions.ContainsKey(actionId))
            {
                IActionClient action = (IActionClient) Assembly.GetExecutingAssembly().CreateInstance(actionId + "Client");
            
                if (action == null)
                {
                    return;
                }

                this.actions.Add(actionId, action);
            }
        
            this.actions[actionId].Execute(this, jsonParams, isOpponent);
        }

    #endregion

    #region PlayerEngine Methods

        /// <summary>
        /// Converts the played cards into json params and invokes the server rpc.
        /// </summary>
        /// <warning>Invokes a server rpc!</warning>
        public void PassTurn()
        {
            // Disable buttons
            this.PlayerData.UndoButton.interactable = false;
            this.PlayerData.PassTurnButton.interactable = false;
            
            // Get json params
            PassTurnParams passTurnParams = new PassTurnParams(
                this.PlayerData.PlayedCards.Select(x => x.Card.UniqueId).Reverse().ToArray(),
                this.PlayerData.PlayedCards.Select(x => x.FieldPosition).Reverse().ToArray());
            
            string jsonParams = JsonUtility.ToJson(passTurnParams);
            
            // Reset played cards
            this.PlayerData.PlayedCards.Clear();
            
            // Update player phase
            this.PlayerData.PlayerPhase = PlayerPhase.Synchronize;
            
            // Invoke pass turn event
            EventManager.Instance.PassTurnServerRpc(jsonParams);
        }

        /// <summary>
        /// Undoes the last played card, by removing it from the battlefield and adding it to the hand.
        /// </summary>
        public void UndoCard()
        {
            // Get last played card
            CardSlot cardSlot = this.PlayerData.PlayedCards.Pop();
            if (cardSlot == null) return;
            
            // Add card to hand
            this.PlayerData.Hand.Add(cardSlot.Card);
            cardSlot.Card.CardState = CardState.Hand;
            this.ArrangeHand(null);
            
            // Add mana
            this.PlayerData.Mana += cardSlot.Card.Cost;
            
            // Remove card from battlefield
            cardSlot.Card = null;
            
            // Update undo button
            this.PlayerData.UndoButton.interactable = this.PlayerData.PlayedCards.Count > 0;
        }

        /// <summary>
        /// Displays the information of a given card in the UI;
        /// </summary>
        /// <param name="card"></param>
        public void ShowCardInformation(Card card)
        {
            // Set information
            this.PlayerData.InfoName.text = card.name;
            this.PlayerData.InfoPower.text = card.Power.ToString();
            this.PlayerData.InfoCost.text = card.Cost.ToString();
            this.PlayerData.InfoAbility.text = card.Description;
            
            // Enable card information
            this.PlayerData.CardInformation.SetActive(true);
        }
        
        /// <summary>
        /// Rearranges the cards in the players hand, excluding the given  cards exceptions (can be null).
        /// </summary>
        /// <param name="exceptions"></param>
        public void ArrangeHand(List<Card> exceptions)
        {
            // Get card numbers
            int exceptionCount = exceptions?.Count ?? 0;
            int totalCards = this.PlayerData.Hand.Count - exceptionCount;
            float totalRadius = (totalCards - 1) * this.cardRadius;

            Vector3 startingPosition;
            
            // Set visibility of slider
            this.cardSlider.gameObject.SetActive(totalCards > 4);
            
            // Use slider
            if (totalCards > 4)
            {
                // Calculate starting position
                Vector3 handRadiusPosition = this.handTransform.position - new Vector3(this.handRadius, 0, 0);
                float remainingWidth = (totalRadius - this.handRadius) * 2;
                float sliderOffset = remainingWidth * this.cardSlider.value;

                startingPosition = handRadiusPosition - new Vector3(sliderOffset, 0, 0);
            }
            // Do NOT use slider
            else
            {
                // Calculate starting position
                startingPosition = this.handTransform.position - new Vector3(totalRadius, 0, 0);
            }

            // Set card positions
            foreach (Card card in this.PlayerData.Hand)
            {
                if (exceptions != null)
                {
                    if (exceptions.Contains(card)) continue;
                }

                card.transform.position = startingPosition;
                startingPosition.x += this.cardRadius * 2;
            }
        }
        
        /// <summary>
        /// Places a card onto the battlefield and updates associated properties (mana, ...).
        /// </summary>
        /// <param name="playCardParams"></param>
        public void PlayCard(PlayCardParams playCardParams)
        {
            // Get params
            Card playedCard = this.PlayerData.Hand.FirstOrDefault(x => x.UniqueId == playCardParams.uniqueId);
            if (playedCard == null || playCardParams.position.x > 1 || playCardParams.position.y > 2) return;
            CardSlot cardSlot = this.PlayerData.PlayerField[playCardParams.position.x, playCardParams.position.y];

            // Update card
            this.PlayerData.Hand.Remove(playedCard);
            playedCard.CardState = CardState.Field;
            playedCard.transform.position = cardSlot.transform.position;
                
            // Update card slot
            cardSlot.Card = playedCard;
            
            // Update mana
            this.PlayerData.Mana -= playedCard.Cost;
            
            // Update played cards
            this.PlayerData.PlayedCards.Push(cardSlot);
            
            // Update undo button
            this.PlayerData.UndoButton.interactable = true;
        }

        /// <summary>
        /// Instantiates a new card and adds it to the hand.
        /// </summary>
        /// <param name="drawCardParams"></param>
        public void DrawCard(DrawCardParams drawCardParams)
        {
            // Instantiate card
            Card card = DatabaseManager.Instance.GetCardById(drawCardParams.id, drawCardParams.uniqueId);

            // Add card to library
            this.PlayerData.Hand.Add(card);
            card.CardState = CardState.Hand;

            // Update UI
            this.ArrangeHand(null);
        }

    #endregion

    #region Observable Methods
    
        private void StartMatch(string jsonParams)
        {
            // Get params
            StartMatchParams startMatchParams = JsonUtility.FromJson<StartMatchParams>(jsonParams);
            
            // Set opponent
            this.PlayerData.OpponentName = startMatchParams.opponentName;
            
            // Set mana and turn
            this.PlayerData.Mana = startMatchParams.mana;
            this.PlayerData.Turn = startMatchParams.turn;

            // Draw starting hand
            for (int i = 0; i < startMatchParams.handIds.Length; i++)
            {
                this.DrawCard(new DrawCardParams(startMatchParams.handIds[i], startMatchParams.handUniqueIds[i]));
            }
            
            // Enable buttons
            this.PlayerData.PassTurnButton.interactable = true;
        }

        private void SyncPlayer(string jsonParams)
        {
            // Get params
            SyncPlayerParams syncPlayerParams = JsonUtility.FromJson<SyncPlayerParams>(jsonParams);
            
            // Perform every card's action
            for (int i = 0; i < syncPlayerParams.playedCardUniqueIds.Length; i++)
            {
                 Card card = this.PlayerData.PlayerField.Cast<CardSlot>().ToList()
                    .Find(x => x.Card?.UniqueId == syncPlayerParams.playedCardUniqueIds[i]).Card;
                 
                if (card == null) continue;
                if (card.ActionId == "") continue;
                
                this.InvokeAction(card.ActionId, syncPlayerParams.actionParams[i], false);
            }
            
            // Update Points
            this.PlayerData.PlayerPoints = syncPlayerParams.points;
        }

        private void SyncOpponent(string jsonParams)
        {
            // Get params
            SyncOpponentParams syncOpponentParams = JsonUtility.FromJson<SyncOpponentParams>(jsonParams);

            // Play every card and perform its action
            for (int i = 0; i < syncOpponentParams.playedCardIds.Length; i++)
            {
                // Play card
                Card card = DatabaseManager.Instance.GetCardById(syncOpponentParams.playedCardIds[i],
                    syncOpponentParams.playedCardUniqueIds[i]);

                CardSlot cardSlot = this.PlayerData.OpponentField[syncOpponentParams.positions[i].x,
                    syncOpponentParams.positions[i].y];

                cardSlot.Card = card;
                card.transform.position = cardSlot.transform.position;
                
                // Perform action
                if (card.ActionId == "" || syncOpponentParams.actionParams[i] == "NaN") continue;
                this.InvokeAction(card.ActionId, syncOpponentParams.actionParams[i], true); 
            }
            
            // Update Points
            this.PlayerData.OpponentPoints = syncOpponentParams.points;
        }

        private void EndTurn(string jsonParams)
        {
            // Get params
            EndTurnParams endTurnParams = JsonUtility.FromJson<EndTurnParams>(jsonParams);
            
            // Draw card
            this.DrawCard(new DrawCardParams(endTurnParams.drawnCardId, endTurnParams.drawnCardUniqueId));
            
            // Update UI
            this.PlayerData.PassTurnButton.interactable = true;
            this.PlayerData.Mana = endTurnParams.mana;
            this.PlayerData.Turn = endTurnParams.turn;
            
            // Update player state
            this.PlayerData.PlayerPhase = PlayerPhase.Deploy;
        }
        
        private void EndGame(string jsonParams)
        {
            // Get params
            EndGameParams endGameParams = JsonUtility.FromJson<EndGameParams>(jsonParams);

            // Log end game
            Logger.LogEndGame(endGameParams.won);
            
            // Enable ending screen
            this.PlayerData.EndingScreen.SetActive(true);
            this.PlayerData.EndingMessage.text = $"You {(endGameParams.won ? "won" : "lost")} the game!";
            this.PlayerData.EndingMessage.color = endGameParams.won ? new Color(0.953f, 0.753f, 0.255f) : new Color(1f, 0.333f, 0.286f);
        }

    #endregion
}
