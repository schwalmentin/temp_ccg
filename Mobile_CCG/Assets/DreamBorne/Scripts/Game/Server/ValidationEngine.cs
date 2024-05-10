using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class ValidationEngine
{
    #region Variables

        private readonly ServerEngine serverEngine;
        private readonly Dictionary<ulong, ServerDataProxy> serverDataProxy;
        private readonly int deckSize;

        public ValidationEngine(ServerEngine serverEngine, int deckSize)
        {
            this.deckSize = deckSize;

            this.serverEngine = serverEngine;
            this.serverDataProxy = new Dictionary<ulong, ServerDataProxy>();
            
            EventManager.Instance.p_joinMatch += this.JoinMatch;
            EventManager.Instance.p_passTurn += this.PassTurn;
        }

        public void OnDestroy()
        {
            EventManager.Instance.p_joinMatch -= this.JoinMatch;
            EventManager.Instance.p_passTurn -= this.PassTurn;
        }

    #endregion
    
    #region Observable Validation Methods

        /// <summary>
        /// Observes the players and checks if they fulfill the requirements to join the match.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="serverRpcParams"></param>
        private void JoinMatch(string jsonParams, ServerRpcParams serverRpcParams)
        {
            // Get params
            JoinMatchParams joinMatchParams;
            ulong playerId = serverRpcParams.Receive.SenderClientId;
            
            try
            {
                joinMatchParams = JsonUtility.FromJson<JoinMatchParams>(jsonParams);
            }
            catch (Exception e)
            {
                // Reset Player
                Debug.LogException(e);
                return;
            }
            
            // Check if the player already joined
            if (this.serverDataProxy.ContainsKey(playerId)) return;

            // Check if there are no more than 2 players
            if (this.serverDataProxy.Count > 2) return;

            // Check if the deck is valid
            if (joinMatchParams.deckIds.Length != this.deckSize) return;
            if (joinMatchParams.deckIds.Distinct().Count() != this.deckSize) return;
            
            // Add player
            this.serverDataProxy.Add(playerId, this.serverEngine.JoinMatch(playerId, joinMatchParams));
        }

        /// <summary>
        /// Observes the players and checks if they fulfill the requirements to pass their turn. 
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="serverRpcParams"></param>
        private void PassTurn(string jsonParams, ServerRpcParams serverRpcParams)
        {
            // Get params
            PassTurnParams passTurnParams;
            ulong playerId = serverRpcParams.Receive.SenderClientId;

            try
            {
                passTurnParams = JsonUtility.FromJson<PassTurnParams>(jsonParams);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }
            
            List<Card> cards = new List<Card>();
            foreach (int cardUniqueId in passTurnParams.playedCardUniqueIds)
            {
                cards.AddRange(this.serverDataProxy[playerId].Hand.Where(x => x.UniqueId == cardUniqueId));
            }
            
            // Check if the player can pass the turn
            if (this.serverDataProxy[playerId].PlayerPhase != PlayerPhase.Deploy)
            {
                Logger.LogError($"Player with id {playerId} is not in the deploy phase.");
                return;
            }
            
            // Check if every played card has a position
            if (passTurnParams.positions.Length != passTurnParams.playedCardUniqueIds.Length)
            {
                Logger.LogError($"Player with id {playerId} doesn't have the same amount of card ids and positions.");
                return;
            }
            
            // Check if the cards exist in hand
            if (cards.Count != passTurnParams.playedCardUniqueIds.Length)
            {
                Logger.LogError($"Player with id {playerId} played a card that is not in their hand. {cards.Count} | {passTurnParams.playedCardUniqueIds.Length}");
                return;
            }

            // Check if the right amount of mana was spent
            if (cards.Sum(x => x.Cost) > this.serverDataProxy[playerId].Mana)
            {
                Logger.LogError($"Player with id {playerId} spent an illegal amount of mana.");
                return;
            }
            
            // Check if the positions are valid
            if (passTurnParams.positions.Any(position => this.serverDataProxy[playerId].Field[position.x, position.y] != null))
            {
                Logger.LogError($"Player with id {playerId} played a card on an illegal position.");
                return;
            }
            
            // Pass Turn
            this.serverEngine.PassTurn(playerId, passTurnParams);
        }

    #endregion
}
