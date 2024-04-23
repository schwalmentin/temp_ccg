using System;
using Unity.Netcode;
using UnityEngine;

public class EventManager : NetworkSingleton<EventManager>
{
    #region Player Events

        public event Action<string, ServerRpcParams> p_joinMatch;
        public event Action<string, ServerRpcParams> p_passTurn;

    #endregion
    
    #region Server Events

        public event Action<string> s_startMatch;
        public event Action<string> s_syncPlayer;
        public event Action<string> s_syncOpponent;
        public event Action<string> s_endTurn;
        public event Action<string> s_endGame;

    #endregion
    
    #region Server RPCs

        /// <summary>
        /// Is called by the player to join the match and provide their deck list. The server listens for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="serverRpcParams"></param>
        [ServerRpc(RequireOwnership = false)]
        public void JoinMatchServerRpc(string jsonParams, ServerRpcParams serverRpcParams = default)
        {
            this.p_joinMatch?.Invoke(jsonParams, serverRpcParams);
        }

        /// <summary>
        /// Is called by the player to pass the turn and provide the played cards of the current turn. The server listens for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="serverRpcParams"></param>
        [ServerRpc(RequireOwnership = false)]
        public void PassTurnServerRpc(string jsonParams, ServerRpcParams serverRpcParams = default)
        {
            this.p_passTurn?.Invoke(jsonParams, serverRpcParams);
        }

    #endregion

    #region Client RPCs

        /// <summary>
        /// Is called by the server to start the match and provide the starting hand of the players.
        /// The players listen for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        public void StartMatchClientRpc(string jsonParams, ClientRpcParams clientRpcParams)
        {
            this.s_startMatch?.Invoke(jsonParams);
        }
        
        /// <summary>
        /// Is called by the server to sync each players own board state.
        /// The players listen for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        public void SyncPlayerClientRpc(string jsonParams, ClientRpcParams clientRpcParams)
        {
            this.s_syncPlayer?.Invoke(jsonParams);
        }
        
        /// <summary>
        /// Is called by the server to sync each players board state of the opponent.
        /// The players listen for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        public void SyncOpponentClientRpc(string jsonParams, ClientRpcParams clientRpcParams)
        {
            this.s_syncOpponent?.Invoke(jsonParams);
        }
        
        /// <summary>
        /// Is called by the server to end the turn and provide the drawn card for the next turn.
        /// The players listen for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        public void EndTurnClientRpc(string jsonParams, ClientRpcParams clientRpcParams)
        {
            this.s_endTurn?.Invoke(jsonParams);
        }
        
        /// <summary>
        /// Is called by the server to end the game and provide the information whether a player has won or lost.
        /// The players listens for the call.
        /// </summary>
        /// <param name="jsonParams"></param>
        /// <param name="clientRpcParams"></param>
        [ClientRpc]
        public void EndGameClientRpc(string jsonParams, ClientRpcParams clientRpcParams)
        {
            this.s_endGame?.Invoke(jsonParams);
        }

    #endregion

    #region Debug RPCs

        /// <summary>
        /// A server rpc for the player to send test messages to the server.
        /// </summary>
        /// <param name="message"></param>
        [ServerRpc(RequireOwnership = false)]
        public void LogMessageServerRpc(string message)
        {
            Debug.Log("CLIENT: " + message);
        }

        /// <summary>
        /// A client rpc for the server to send test messages to all players.
        /// </summary>
        /// <param name="message"></param>
        [ClientRpc]
        public void LogMessageClientRpc(string message)
        {
            Debug.Log("SERVER: " + message);
        }
    
    #endregion
}
