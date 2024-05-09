using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ServerEngine
{
    #region Variables

        private readonly Dictionary<ulong, ServerData> serverData;
        private int uniqueIdCounter;
        private int startingHandAmount;
        
        public ServerEngine(int startingHandAmount)
        {
            this.serverData = new Dictionary<ulong, ServerData>();
            this.uniqueIdCounter = 0;
            this.startingHandAmount = startingHandAmount;
        }

        public void OnDestroy()
        {
            
        }

    #endregion
    
    #region ServerEngine Methods

        private void DrawCard(ulong playerId)
        {
            // Get card id
            if (this.serverData[playerId].Library.Count == 0)
            {
                Debug.LogError("No more cards available");
                return;
            }
            
            int cardId = this.serverData[playerId].Library.Pop();
            
            // Create card
            Card card = DatabaseManager.Instance.GetCardById(cardId, this.GetUniqueId());
            
            // Place card in hand
            this.serverData[playerId].Hand.Add(card);
        }

        private int GetUniqueId()
        {
            return this.uniqueIdCounter++;
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
            foreach (ulong uniqueIdKey in this.serverData.Keys)
            {
                this.StartMatch(uniqueIdKey);
            }
            
            return new ServerDataProxy(newServerData);
        }

        public void PassTurn(ulong playerId, PassTurnParams passTurnParams)
        {
            throw new NotImplementedException();
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
                this.serverData[playerId].Name,
                this.serverData[playerId].Mana,
                this.serverData[playerId].Turn);

            string jsonParams = JsonUtility.ToJson(startMatchParams);
            
            EventManager.Instance.StartMatchClientRpc(jsonParams, this.serverData[playerId].ClientRpcParams);
        }

    #endregion
}
