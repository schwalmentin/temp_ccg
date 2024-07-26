using UnityEngine;

#region Action Params

    [System.Serializable]
    public struct DrawCardParams
    {
        public int id;
        public int uniqueId;

        public DrawCardParams(int id, int uniqueId)
        {
            this.id = id;
            this.uniqueId = uniqueId;
        }
    }

    [System.Serializable]
    public struct PlayCardParams
    {
        public int uniqueId;
        public Vector2Int position;

        public PlayCardParams(int uniqueId, Vector2Int position)
        {
            this.uniqueId = uniqueId;
            this.position = position;
        }
    }

#endregion

#region Rpc Params

    // Server Rpc Params
    [System.Serializable]
    public struct JoinMatchParams
    {
        public int[] deckIds;
        public string playerName;

        public JoinMatchParams(int[] deckIds, string playerName)
        {
            this.deckIds = deckIds;
            this.playerName = playerName;
        }
    }

    [System.Serializable]
    public struct PassTurnParams
    {
        public int[] playedCardUniqueIds;
        public Vector2Int[] positions;
        
        public PassTurnParams(int[] playedCardUniqueIds, Vector2Int[] positions)
        {
            this.playedCardUniqueIds = playedCardUniqueIds;
            this.positions = positions;
        }
    }

    // Client Rpc Params
    [System.Serializable]
    public struct StartMatchParams
    {
        public int[] handIds;
        public int[] handUniqueIds;
        public string opponentName;
        public int mana;
        public int turn;

        public StartMatchParams(int[] handIds, int[] handUniqueIds, string opponentName, int mana, int turn)
        {
            this.handIds = handIds;
            this.handUniqueIds = handUniqueIds;
            this.opponentName = opponentName;
            this.mana = mana;
            this.turn = turn;
        }
    }

    [System.Serializable]
    public struct SyncPlayerParams
    {
        public int[] playedCardUniqueIds;
        public string[] actionParams;
        public int points;

        public SyncPlayerParams(int[] playedCardUniqueIds, string[] actionParams, int points)
        {
            this.playedCardUniqueIds = playedCardUniqueIds;
            this.actionParams = actionParams;
            this.points = points;
        }
    }

    [System.Serializable]
    public struct SyncOpponentParams
    {
        public int[] playedCardIds;
        public int[] playedCardUniqueIds;
        public Vector2Int[] positions;
        public string[] actionParams;
        public int points;
        
        public SyncOpponentParams(int[] playedCardIds, int[] playedCardUniqueIds, Vector2Int[] positions, string[] actionParams, int points)
        {
            this.playedCardIds = playedCardIds;
            this.playedCardUniqueIds = playedCardUniqueIds;
            this.positions = positions;
            this.actionParams = actionParams;
            this.points = points;
        }
    }

    [System.Serializable]
    public struct EndTurnParams
    {
        public int drawnCardId;
        public int drawnCardUniqueId;
        public int mana;
        public int turn;

        public EndTurnParams(int drawnCardId, int drawnCardUniqueId, int mana, int turn)
        {
            this.drawnCardId = drawnCardId;
            this.drawnCardUniqueId = drawnCardUniqueId;
            this.mana = mana;
            this.turn = turn;
        }
    }

    [System.Serializable]
    public struct EndGameParams
    {
        public bool won;

        public EndGameParams(bool won)
        {
            this.won = won;
        }
    }

#endregion