using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class PlayerEngine : MonoBehaviour
{
    #region Variables

        //Player
        private PlayerData playerData;
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
                { "PlayCard", this.PlayCard }
            };

            EventManager.Instance.s_startMatch += this.StartMatch;
            EventManager.Instance.s_syncPlayer += this.SyncPlayer;
            EventManager.Instance.s_syncOpponent += this.SyncOpponent;
            EventManager.Instance.s_endTurn += this.EndTurn;
            EventManager.Instance.s_endGame += this.EndGame;
            
            this.ArrangeHand(null);
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

    #endregion

    #region PlayerEngine Methods

        public void PassTurn()
        {
            
        }

        public void Undo()
        {
            
        }
        
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
            this.playerData.currentMana -= playedCard.Cost;
        }

        public void DrawCard(DrawCardParams drawCardParams)
        {
            // Instantiate card
            
            // Add card to library
            
            // Update UI
        }

    #endregion

    #region Observable Methods

        private void StartMatch(string jsonParams)
        {
            StartMatchParams startMatchParams = JsonUtility.FromJson<StartMatchParams>(jsonParams);
        }

        private void SyncPlayer(string jsonParams)
        {
            SyncPlayerParams syncPlayerParams = JsonUtility.FromJson<SyncPlayerParams>(jsonParams);
        }

        private void SyncOpponent(string jsonParams)
        {
            SyncPlayerParams syncOpponentParams = JsonUtility.FromJson<SyncPlayerParams>(jsonParams);
        }

        private void EndTurn(string jsonParams)
        {
            EndTurnParams endTurnParams = JsonUtility.FromJson<EndTurnParams>(jsonParams);
        }
        
        private void EndGame(string jsonParams)
        {
            EndGameParams endGameParams = JsonUtility.FromJson<EndGameParams>(jsonParams);
        }

    #endregion
}
