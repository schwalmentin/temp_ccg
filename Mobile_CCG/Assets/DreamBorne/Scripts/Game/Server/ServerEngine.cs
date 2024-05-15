using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerEngine
{
    #region Variables

        // Mappings
        private readonly Dictionary<ulong, ServerData> serverData;
        private readonly Dictionary<string, Action<ulong, Card>> actions;
        
        // Properties
        private int uniqueIdCounter;
        private readonly int startingHandAmount;
        private readonly int maxTurnAmount;
        private readonly Stack<GameObject> cardHolders;
        
        public ServerEngine(int startingHandAmount, int maxTurnAmount, Stack<GameObject> cardHolders)
        {
            // Properties
            this.serverData = new Dictionary<ulong, ServerData>();
            this.uniqueIdCounter = 0;
            this.startingHandAmount = startingHandAmount;
            this.maxTurnAmount = maxTurnAmount;
            this.cardHolders = cardHolders;
            
            // Actions
            this.actions = new Dictionary<string, Action<ulong, Card>>
            {
                { "TestAction", this.TestAction }
            };
        }

        public void OnDestroy()
        {
            
        }

    #endregion
    
    #region ServerEngine Methods

        /// <summary>
        /// Creates a new card and puts it into the hand of the player with id playerid.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        private Card DrawCard(ulong playerId)
        {
            // Get card id
            if (this.serverData[playerId].Library.Count == 0)
            {
                Debug.LogError("No more cards available");
                return null;
            }
            
            int cardId = this.serverData[playerId].Library.Pop();
            
            // Create card
            Card card = DatabaseManager.Instance.GetCardById(cardId, this.GetUniqueId());
            card.transform.parent = this.serverData[playerId].CardHolder.transform;
            
            // Place card in hand
            this.serverData[playerId].Hand.Add(card);
            return card;
        }

        /// <summary>
        /// Returns a unique integer id by counting up from 0.
        /// </summary>
        /// <returns></returns>
        private int GetUniqueId()
        {
            return this.uniqueIdCounter++;
        }

        /// <summary>
        /// Returns the opponent of the player with id playerId.
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        private ulong GetOpponentId(ulong playerId)
        {
            return this.serverData.Keys.FirstOrDefault(id => id != playerId);
        }

    #endregion

    #region Action Methods

        /// <summary>
        /// Creates a debug message and saves it into the params of the card.
        /// </summary>
        /// <param name="playerId"></param>
        /// <param name="card"></param>
        private void TestAction(ulong playerId, Card card)
        {
            TestActionParams testActionParams = new TestActionParams($"This is a TestAction message invoked by {this.serverData[playerId].Name}!");
            string jsonParams = JsonUtility.ToJson(testActionParams);
            card.ActionParams = jsonParams;
            
            Logger.LogAction($"This is a TestAction processed by the Server by {this.serverData[playerId].Name}");
        }

    #endregion

    #region Observable Methods

        public ServerDataProxy JoinMatch(ulong playerId, JoinMatchParams joinMatchParams)
        {
            // Add player
            GameObject cardHolder = this.cardHolders.Pop();
            cardHolder.name = joinMatchParams.playerName;
            ServerData newServerData = new ServerData(joinMatchParams.playerName, playerId, joinMatchParams.deckIds, cardHolder);
            this.serverData.Add(playerId, newServerData);

            // Check if both players joined the match
            if (this.serverData.Count != 2) return new ServerDataProxy(newServerData);
            
            // Start match invocation
            foreach (ulong id in this.serverData.Keys)
            {
                this.StartMatch(id);
            }
            
            return new ServerDataProxy(newServerData);
        }

        public void PassTurn(ulong playerId, PassTurnParams passTurnParams)
        {
            // Update Player Phase
            this.serverData[playerId].PlayerPhase = PlayerPhase.Synchronize;
            
            // Update player state
            for (int i = 0; i < passTurnParams.playedCardUniqueIds.Length; i++)
            {
                // Place card
                Card card = this.serverData[playerId].Hand.Find(x => x.UniqueId == passTurnParams.playedCardUniqueIds[i]);
                Vector2Int position = passTurnParams.positions[i];
                
                this.serverData[playerId].Hand.Remove(card);
                this.serverData[playerId].Field[position.x, position.y] = card;
                
                // Add to played cards
                this.serverData[playerId].PlayedCards.Add(position, card);
                
                // Perform action
                if (card.ActionId == "") continue;
                this.actions[card.ActionId].Invoke(playerId, card);
            }

            // Check if both players passed the turn
            if (this.serverData[this.GetOpponentId(playerId)].PlayerPhase != PlayerPhase.Synchronize) return;

            // Sync player
            foreach (ulong id in this.serverData.Keys)
            {
                this.SyncPlayer(id);
            }
            
            // Sync opponent
            foreach (ulong id in this.serverData.Keys)
            {
                this.SyncOpponent(id);
            }
            
            // End Turn
            foreach (ulong id in this.serverData.Keys)
            {
                // End Game if all turns are over
                if (this.serverData[id].Turn >= this.maxTurnAmount)
                {
                    this.EndGame(id);
                    continue;
                }

                // End turn
                this.EndTurn(id);
            }
        }

    #endregion
    
    #region Event Invocation Methods

        private void StartMatch(ulong playerId)
        {
            // Draw starting hand
            for (int i = 0; i < this.startingHandAmount; i++)
            {
                this.DrawCard(playerId);
            }
            
            // Start Match
            StartMatchParams startMatchParams = new StartMatchParams(
                this.serverData[playerId].Hand.Select(x => x.Id).ToArray(),
                this.serverData[playerId].Hand.Select(x => x.UniqueId).ToArray(),
                this.serverData[GetOpponentId(playerId)].Name,
                this.serverData[playerId].Mana,
                this.serverData[playerId].Turn);

            string jsonParams = JsonUtility.ToJson(startMatchParams);
            
            EventManager.Instance.StartMatchClientRpc(jsonParams, this.serverData[playerId].ClientRpcParams);
        }

        private void SyncPlayer(ulong playerId)
        {
            // Update points
            this.serverData[playerId].Points = 0;

            foreach (Card card in this.serverData[playerId].Field)
            {
                if (card == null) continue;
                this.serverData[playerId].Points += card.Power;
            }
            
            // Sync player
             SyncPlayerParams syncPlayerParams = new SyncPlayerParams(
                this.serverData[playerId].PlayedCards.Values.Select(x => x.UniqueId).ToArray(),
                this.serverData[playerId].PlayedCards.Values.Select(x => x.ActionParams).ToArray(),
                this.serverData[playerId].Points);
             
            string jsonParams = JsonUtility.ToJson(syncPlayerParams);
            EventManager.Instance.SyncPlayerClientRpc(jsonParams, this.serverData[playerId].ClientRpcParams);
        }

        private void SyncOpponent(ulong playerId)
        {
            ulong opponentId = this.GetOpponentId(playerId);
            SyncOpponentParams syncOpponentParams = new SyncOpponentParams(
                this.serverData[opponentId].PlayedCards.Values.Select(x => x.Id).ToArray(),
                this.serverData[opponentId].PlayedCards.Values.Select(x => x.UniqueId).ToArray(),
                this.serverData[opponentId].PlayedCards.Keys.ToArray(),
                this.serverData[opponentId].PlayedCards.Values.Select(x => x.ActionParams).ToArray(),
                this.serverData[opponentId].Points);
            
            string jsonParams = JsonUtility.ToJson(syncOpponentParams);
            EventManager.Instance.SyncOpponentClientRpc(jsonParams, this.serverData[playerId].ClientRpcParams);
        }

        private void EndTurn(ulong playerId)
        {
            // Clear played cards
            this.serverData[playerId].PlayedCards.Clear();
            
            // Draw a card
            Card drawnCard = this.DrawCard(playerId);
            
            // Update Turn
            this.serverData[playerId].Turn++;
            this.serverData[playerId].Mana = this.serverData[playerId].Turn;
            this.serverData[playerId].PlayerPhase = PlayerPhase.Deploy;
            
            // End Turn
            EndTurnParams endTurnParams = new EndTurnParams(
                drawnCard.Id,
                drawnCard.UniqueId,
                this.serverData[playerId].Mana,
                this.serverData[playerId].Turn);

            string jsonParams = JsonUtility.ToJson(endTurnParams);
            EventManager.Instance.EndTurnClientRpc(jsonParams, this.serverData[playerId].ClientRpcParams);
        }

        private void EndGame(ulong playerId)
        {
            EndGameParams endGameParams = new EndGameParams(
                this.serverData[playerId].Points >= this.serverData[this.GetOpponentId(playerId)].Points);

            string jsonParams = JsonUtility.ToJson(endGameParams);
            EventManager.Instance.EndGameClientRpc(jsonParams, this.serverData[playerId].ClientRpcParams);
        }

    #endregion
}
