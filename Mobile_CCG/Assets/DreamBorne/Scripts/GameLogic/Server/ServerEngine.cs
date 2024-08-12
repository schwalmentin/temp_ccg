using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class ServerEngine
{
    #region Variables

        // Mappings
        public Dictionary<ulong, ServerData> ServerData { get; }

        private readonly Dictionary<string, IActionServer> actions;
        
        // Properties
        private int uniqueIdCounter;
        private readonly int startingHandAmount;
        private readonly int maxTurnAmount;
        private readonly Stack<GameObject> cardHolders;
        
        public ServerEngine(int startingHandAmount, int maxTurnAmount, Stack<GameObject> cardHolders)
        {
            // Properties
            this.ServerData = new Dictionary<ulong, ServerData>();
            this.uniqueIdCounter = 0;
            this.startingHandAmount = startingHandAmount;
            this.maxTurnAmount = maxTurnAmount;
            this.cardHolders = cardHolders;
            
            // Actions
            this.actions = new Dictionary<string, IActionServer>();
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
        public Card DrawCard(ulong playerId)
        {
            // Get card id
            if (this.ServerData[playerId].Library.Count == 0)
            {
                Debug.LogError("No more cards available");
                return null;
            }
            
            int cardId = this.ServerData[playerId].Library.Pop();
            
            // Create card
            Card card = DatabaseManager.Instance.GetCardById(cardId, this.GetUniqueId());
            card.transform.parent = this.ServerData[playerId].CardHolder.transform;
            
            // Place card in hand
            this.ServerData[playerId].Hand.Add(card);
            return card;
        }

        /// <summary>
        /// Returns a unique integer id by counting up from 0.
        /// </summary>
        /// <returns></returns>
        public int GetUniqueId()
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
            return this.ServerData.Keys.FirstOrDefault(id => id != playerId);
        }

    #endregion

    #region Action Methods

        /// <summary>
        /// Invokes an action based on its corresponding id.
        /// </summary>
        /// <param name="actionId"></param>
        /// <param name="serverData"></param>
        /// <param name="card"></param>
        private void InvokeAction(string actionId, ulong playerId, Card card)
        {
            if (!this.actions.ContainsKey(actionId))
            {
                IActionServer action = (IActionServer) Assembly.GetExecutingAssembly().CreateInstance(actionId + "Server");

                if (action == null)
                {
                    Logger.LogError($"Action with the name {actionId}Server does not exist!");
                    return;
                }
                this.actions.Add(actionId, action);
            }
            
            this.actions[actionId].Execute(this, playerId, card);
        }

    #endregion

    #region Observable Methods

        public ServerDataProxy JoinMatch(ulong playerId, JoinMatchParams joinMatchParams)
        {
            // Add player
            GameObject cardHolder = this.cardHolders.Pop();
            cardHolder.name = joinMatchParams.playerName;
            ServerData newServerData = new ServerData(joinMatchParams.playerName, playerId, joinMatchParams.deckIds, cardHolder);
            this.ServerData.Add(playerId, newServerData);

            // Check if both players joined the match
            if (this.ServerData.Count != 2) return new ServerDataProxy(newServerData);
            
            // Start match invocation
            foreach (ulong id in this.ServerData.Keys)
            {
                this.StartMatch(id);
            }
            
            return new ServerDataProxy(newServerData);
        }

        public void PassTurn(ulong playerId, PassTurnParams passTurnParams)
        {
            // Update Player Phase
            this.ServerData[playerId].PlayerPhase = PlayerPhase.Synchronize;
            
            // Play cards
            for (int i = 0; i < passTurnParams.playedCardUniqueIds.Length; i++)
            {
                // Place card
                Card card = this.ServerData[playerId].Hand.Find(x => x.UniqueId == passTurnParams.playedCardUniqueIds[i]);
                Vector2Int position = passTurnParams.positions[i];
                
                this.ServerData[playerId].Hand.Remove(card);
                this.ServerData[playerId].Field[position.x, position.y] = card;
                
                // Add to played cards
                this.ServerData[playerId].PlayedCards.Add(position, card);
            }
            
            // Invoke card actions
            foreach (Card card in this.ServerData[playerId].PlayedCards.Values.Where(card => card.ActionId != ""))
            {
                this.InvokeAction(card.ActionId, playerId, card);
            }

            // Check if both players passed the turn
            if (this.ServerData[this.GetOpponentId(playerId)].PlayerPhase != PlayerPhase.Synchronize) return;

            // Sync player
            foreach (ulong id in this.ServerData.Keys)
            {
                this.SyncPlayer(id);
            }
            
            // Sync opponent
            foreach (ulong id in this.ServerData.Keys)
            {
                this.SyncOpponent(id);
            }
            
            // End Turn
            foreach (ulong id in this.ServerData.Keys)
            {
                // End Game if all turns are over
                if (this.ServerData[id].Turn >= this.maxTurnAmount)
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
                this.ServerData[playerId].Hand.Select(x => x.Id).ToArray(),
                this.ServerData[playerId].Hand.Select(x => x.UniqueId).ToArray(),
                this.ServerData[this.GetOpponentId(playerId)].Name,
                this.ServerData[playerId].Mana,
                this.ServerData[playerId].Turn);

            string jsonParams = JsonUtility.ToJson(startMatchParams);
            
            EventManager.Instance.StartMatchClientRpc(jsonParams, this.ServerData[playerId].ClientRpcParams);
        }

        private void SyncPlayer(ulong playerId)
        {
            // Update points
            this.ServerData[playerId].Points = 0;

            foreach (Card card in this.ServerData[playerId].Field)
            {
                if (card == null) continue;
                this.ServerData[playerId].Points += card.Power;
            }
            
            // Sync player
             SyncPlayerParams syncPlayerParams = new SyncPlayerParams(
                this.ServerData[playerId].PlayedCards.Values.Select(x => x.UniqueId).ToArray(),
                this.ServerData[playerId].PlayedCards.Values.Select(x => x.ActionParams).ToArray(),
                this.ServerData[playerId].Points);
             
            string jsonParams = JsonUtility.ToJson(syncPlayerParams);
            EventManager.Instance.SyncPlayerClientRpc(jsonParams, this.ServerData[playerId].ClientRpcParams);
        }

        private void SyncOpponent(ulong playerId)
        {
            ulong opponentId = this.GetOpponentId(playerId);

            // Filter action params
            foreach (Card card in this.ServerData[opponentId].PlayedCards.Values)
            {
                if (!card.PerformOpponent)
                {
                    card.ActionParams = "NaN";
                }
            }
            
            // Create params
            SyncOpponentParams syncOpponentParams = new SyncOpponentParams(
                this.ServerData[opponentId].PlayedCards.Values.Select(x => x.Id).ToArray(),
                this.ServerData[opponentId].PlayedCards.Values.Select(x => x.UniqueId).ToArray(),
                this.ServerData[opponentId].PlayedCards.Keys.ToArray(),
                this.ServerData[opponentId].PlayedCards.Values.Select(x => x.ActionParams).ToArray(),
                this.ServerData[opponentId].Points);
            
            string jsonParams = JsonUtility.ToJson(syncOpponentParams);
            EventManager.Instance.SyncOpponentClientRpc(jsonParams, this.ServerData[playerId].ClientRpcParams);
        }

        private void EndTurn(ulong playerId)
        {
            // Clear played cards
            this.ServerData[playerId].PlayedCards.Clear();
            
            // Draw a card
            Card drawnCard = this.DrawCard(playerId);
            
            // Update Turn
            this.ServerData[playerId].Turn++;
            this.ServerData[playerId].Mana = this.ServerData[playerId].Turn;
            this.ServerData[playerId].PlayerPhase = PlayerPhase.Deploy;
            
            // End Turn
            EndTurnParams endTurnParams = new EndTurnParams(
                drawnCard.Id,
                drawnCard.UniqueId,
                this.ServerData[playerId].Mana,
                this.ServerData[playerId].Turn);

            string jsonParams = JsonUtility.ToJson(endTurnParams);
            EventManager.Instance.EndTurnClientRpc(jsonParams, this.ServerData[playerId].ClientRpcParams);
        }

        private void EndGame(ulong playerId)
        {
            EndGameParams endGameParams = new EndGameParams(
                this.ServerData[playerId].Points >= this.ServerData[this.GetOpponentId(playerId)].Points);

            string jsonParams = JsonUtility.ToJson(endGameParams);
            EventManager.Instance.EndGameClientRpc(jsonParams, this.ServerData[playerId].ClientRpcParams);
        }

    #endregion
}
