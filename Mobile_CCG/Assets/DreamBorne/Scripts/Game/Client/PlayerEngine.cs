using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerEngine : MonoBehaviour
{
    #region Variables

        // Model Reference
        private PlayerData playerData;
        
        // Actions
        private Dictionary<string, Action<string>> actions;
        
        // Handcards Rendering
        [Header("Handcard Rendering")]
        [SerializeField] private Transform handTransform;
        [SerializeField] private float maxHandWidth;
        [SerializeField] private float cardRadius;

    #endregion

    #region Unity Methods

        private void Awake()
        {
            this.playerData = FindObjectOfType<PlayerData>();

            this.actions = new Dictionary<string, Action<string>>
            {
                { "DrawCard", this.DrawCard },
                { "PlayCard", this.PlayCard },
                { "TestAction", this.TestAction }
            };

            EventManager.Instance.s_startMatch += this.StartMatch;
            EventManager.Instance.s_syncPlayer += this.SyncPlayer;
            EventManager.Instance.s_syncOpponent += this.SyncOpponent;
            EventManager.Instance.s_endTurn += this.EndTurn;
            EventManager.Instance.s_endGame += this.EndGame;
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

        private void DrawCard(string jsonParams)
        {
            try
            {
                DrawCardParams drawCardParams = JsonUtility.FromJson<DrawCardParams>(jsonParams);
                this.DrawCard(drawCardParams);
            }
            catch (JsonException e)
            {
                Debug.LogException(e);
            }
        }

        private void PlayCard(string jsonParams)
        {
            try
            {
                PlayCardParams playCardParams = JsonUtility.FromJson<PlayCardParams>(jsonParams);
                this.PlayCard(playCardParams);
            }
            catch (JsonException e)
            {
                Debug.LogException(e);
            }
        }

        private void TestAction(string jsonParams)
        {
            print("Test Action was invoked");
        }

    #endregion

    #region PlayerEngine Methods

        /// <summary>
        /// Converts the played cards into json params and invokes the server rpc.
        /// </summary>
        /// <warning>Invokes a server rpc!</warning>
        public void PassTurn()
        {
            // Get json params
            PassTurnParams passTurnParams = new PassTurnParams(
                this.playerData.PlayedCards.Select(x => x.Card.UniqueId).Reverse().ToArray(),
                this.playerData.PlayedCards.Select(x => x.FieldPosition).Reverse().ToArray());
            
            string jsonParams = JsonUtility.ToJson(passTurnParams);
            
            // Invoke pass turn event
            EventManager.Instance.PassTurnServerRpc(jsonParams);
            Debug.Log($"Pass Turn Rpc: {jsonParams}");
            // Disable buttons
            this.playerData.UndoButton.interactable = false;
            this.playerData.PassTurnButton.interactable = false;
        }

        /// <summary>
        /// Undoes the last played card, by removing it from the battlefield and adding it to the hand.
        /// </summary>
        public void UndoCard()
        {
            // Get last played card
            CardSlot cardSlot = this.playerData.PlayedCards.Pop();
            if (cardSlot == null) return;
            
            // Add card to hand
            this.playerData.Hand.Add(cardSlot.Card);
            cardSlot.Card.CardState = CardState.Hand;
            this.ArrangeHand(null);
            
            // Add mana
            this.playerData.Mana += cardSlot.Card.Cost;
            
            // Remove card from battlefield
            cardSlot.Card = null;
            
            // Update undo button
            this.playerData.UndoButton.interactable = this.playerData.PlayedCards.Count > 0;
        }
        
        /// <summary>
        /// Rearranges the cards in the players hand, excluding the given  cards exceptions (can be null).
        /// </summary>
        /// <param name="exceptions"></param>
        public void ArrangeHand(List<Card> exceptions)
        {
            int exceptionCount = exceptions?.Count ?? 0;

            // Ugly Code, please refactor asap
            float currentCardRadius = this.cardRadius * (this.playerData.Hand.Count - exceptionCount - 1) < this.maxHandWidth / 2 ? 
                this.cardRadius : this.maxHandWidth / 2 / (this.playerData.Hand.Count);

            Vector3 firstPosition = this.handTransform.position;
            firstPosition.x -= currentCardRadius * (this.playerData.Hand.Count - exceptionCount - 1);

            foreach (Card card in this.playerData.Hand)
            {
                if (exceptions != null)
                {
                    if (exceptions.Contains(card))
                    {
                        continue;
                    }
                }
                
                card.transform.position = firstPosition;
                firstPosition.x += currentCardRadius * 2;
            }
        }
        
        /// <summary>
        /// Places a card onto the battlefield and updates associeted properties (mana, ...).
        /// </summary>
        /// <param name="playCardParams"></param>
        public void PlayCard(PlayCardParams playCardParams)
        {
            // Get params
            Card playedCard = this.playerData.Hand.FirstOrDefault(x => x.UniqueId == playCardParams.uniqueId);
            if (playedCard == null || playCardParams.position.x > 1 || playCardParams.position.y > 2) return;
            CardSlot cardSlot = this.playerData.PlayerField[playCardParams.position.x, playCardParams.position.y];

            // Update card
            this.playerData.Hand.Remove(playedCard);
            playedCard.CardState = CardState.Field;
            playedCard.transform.position = cardSlot.transform.position;
                
            // Update card slot
            cardSlot.Card = playedCard;
            
            // Update mana
            this.playerData.Mana -= playedCard.Cost;
            
            // Update played cards
            this.playerData.PlayedCards.Push(cardSlot);
            
            // Update undo button
            this.playerData.UndoButton.interactable = true;
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
            this.playerData.Hand.Add(card);
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
            this.playerData.OpponentName = startMatchParams.opponentName;
            
            // Set mana and turn
            this.playerData.Mana = startMatchParams.mana;
            this.playerData.Turn = startMatchParams.turn;

            // Draw starting hand
            for (int i = 0; i < startMatchParams.handIds.Length; i++)
            {
                this.DrawCard(new DrawCardParams(startMatchParams.handIds[i], startMatchParams.handUniqueIds[i]));
            }
            
            // Enable buttons
            this.playerData.PassTurnButton.interactable = true;
        }

        private void SyncPlayer(string jsonParams)
        {
            // Get params
            SyncPlayerParams syncPlayerParams = JsonUtility.FromJson<SyncPlayerParams>(jsonParams);

            // Perform every cards action
            for (int i = 0; i < syncPlayerParams.playedCardUniqueIds.Length; i++)
            {
                Card card = this.playerData.PlayedCards.FirstOrDefault(x =>
                    x.Card.UniqueId == syncPlayerParams.playedCardUniqueIds[i])?.Card;
                
                if (card == null) continue;
                if (card.ActionId == "") continue;
                
                this.actions[card.ActionId].Invoke(syncPlayerParams.actionParams[i]);
            }
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

                CardSlot cardSlot = this.playerData.OpponentField[syncOpponentParams.positions[i].x,
                    syncOpponentParams.positions[i].y];

                cardSlot.Card = card;
                card.transform.position = cardSlot.transform.position;
                
                // Perform action
                if (card.ActionId == "") continue;
                this.actions[card.ActionId].Invoke(syncOpponentParams.actionParams[i]);
            }
        }

        private void EndTurn(string jsonParams)
        {
            // Get params
            EndTurnParams endTurnParams = JsonUtility.FromJson<EndTurnParams>(jsonParams);
            
            // Draw card
            this.DrawCard(new DrawCardParams(endTurnParams.drawnCardId, endTurnParams.drawnCardUniqueId));
            
            // Update UI
            this.playerData.PassTurnButton.interactable = true;
            this.playerData.Mana = endTurnParams.mana;
            this.playerData.Turn = endTurnParams.turn;
        }
        
        private void EndGame(string jsonParams)
        {
            EndGameParams endGameParams = JsonUtility.FromJson<EndGameParams>(jsonParams);

            string message = endGameParams.won ? "You won the game!" : "You lost the game!";
            print(message);
        }

    #endregion
}
