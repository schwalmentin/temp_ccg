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
        
        public ServerEngine(int startingHandAmount, int maxTurnAmount)
        {
            // Properties
            this.serverData = new Dictionary<ulong, ServerData>();
            this.uniqueIdCounter = 0;
            this.startingHandAmount = startingHandAmount;
            this.maxTurnAmount = maxTurnAmount;
            
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
            
            // Place card in hand
            this.serverData[playerId].Hand.Add(card);
            return card;
        }

        private int GetUniqueId()
        {
            return this.uniqueIdCounter++;
        }

        private ulong GetOpponentId(ulong playerId)
        {
            return this.serverData.Keys.FirstOrDefault(id => id != playerId);
        }

    #endregion

    #region Action Methods

        private void TestAction(ulong playerId, Card card)
        {
            TestActionParams testActionParams = new TestActionParams($"This is a TestAction message invoked by {this.serverData[playerId].Name}!");
            string jsonParams = JsonUtility.ToJson(testActionParams);
            card.ActionParams = jsonParams;
        }

    #endregion

    #region Observable Methods

        public ServerDataProxy JoinMatch(ulong playerId, JoinMatchParams joinMatchParams)
        {
            // Add player
            ServerData newServerData = new ServerData(joinMatchParams.playerName, playerId, joinMatchParams.deckIds);
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
                
                // Perform action
                this.actions[card.ActionId].Invoke(playerId, card);
            }

            // Check if both players passed the turn
            if (this.serverData[this.GetOpponentId(playerId)].PlayerPhase != PlayerPhase.Synchronize) return;

            // End the turn
            foreach (ulong id in this.serverData.Keys)
            {
                // End Game if all turns are over
                if (this.serverData[playerId].Turn >= this.maxTurnAmount)
                {
                    this.EndGame(id);
                }

                // End the turn
                this.SyncPlayer(id);
                this.SyncOpponent(id);
                this.EndTurn(id);
                return;
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
             SyncPlayerParams syncPlayerParams = new SyncPlayerParams(
                this.serverData[playerId].PlayedCards.Values.Select(x => x.UniqueId).ToArray(),
                this.serverData[playerId].PlayedCards.Values.Select(x => x.ActionParams).ToArray());

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
                this.serverData[opponentId].PlayedCards.Values.Select(x => x.ActionParams).ToArray());

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
