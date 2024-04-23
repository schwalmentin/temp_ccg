using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    #region Variables
    
        // Data
        [Header("Data")]
        [SerializeField] private CardSlot[] cardSlots;
        private CardSlot[,] playerField;
        private Card[,] opponentField;
        [SerializeField] private List<Card> hand;

        // Game UI
        [Header("Game UI")]
        [SerializeField] private TextMeshProUGUI playerName;
        [SerializeField] private Image playerIcon;
        [SerializeField] private TextMeshProUGUI playerPoints;
        [Space]
        [SerializeField] private TextMeshProUGUI opponentName;
        [SerializeField] private Image opponentIcon;
        [SerializeField] private TextMeshProUGUI opponentPoints;
        [Space]
        [SerializeField] private Image[] turn;
        [SerializeField] private TextMeshProUGUI mana;

        // Properties
        public List<Card> Hand => this.hand;
        public CardSlot[,] PlayerField => this.playerField;
        public Card[,] OpponentField => this.opponentField;

        #endregion

    #region Unity Methods

        private void Awake()
        {
            this.playerName.text = PlaytestManager.Instance.GetRandomName();
            // this.hand = new List<Card>();
            this.playerField = this.CardSlotsToField(this.cardSlots, new Vector2Int(2, 3));
            this.opponentField = new Card[2, 3];
            this.UpdateTurn(1);
        }

    #endregion

    #region PlayerData Methods

        /// <summary>
        /// Transforms an array of card slots into a 2d array (field) of card slots, based on the given field size and returns the result.
        /// </summary>
        /// <param name="cardSlots"></param>
        /// <param name="fieldSize"></param>
        /// <returns></returns>
        private CardSlot[,] CardSlotsToField(CardSlot[] cardSlots, Vector2Int fieldSize)
    {
        if (fieldSize.x * fieldSize.y != cardSlots.Length)
        {
            return null;
        }
        
        CardSlot[,] field = new CardSlot[fieldSize.x, fieldSize.y];
        
        for (int i = 0; i < cardSlots.Length; i++)
        {
            int x = i % fieldSize.x;
            int y = Mathf.FloorToInt(i / fieldSize.x);
            field[x, y] = cardSlots[i];
        }

        return field;
    }
        
        /// <summary>
        /// Initializes the UI of the players user data (name, icon, ...).
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="playerIcon"></param>
        public void InitializePlayer(string playerName, Sprite playerIcon)
        {
            this.playerName.text = playerName;
            this.playerIcon.sprite = playerIcon;
        }

        /// <summary>
        /// Initializes the UI of the opponents user data (name, icon, ...).
        /// </summary>
        /// <param name="opponentName"></param>
        /// <param name="opponentIcon"></param>
        public void InitializeOpponent(string opponentName, Sprite opponentIcon)
        {
            this.opponentName.text = opponentName;
            this.opponentIcon.sprite = opponentIcon;
        }

        /// <summary>
        /// Updates the UI both the players and opponents points.
        /// </summary>
        /// <param name="playerPoints"></param>
        /// <param name="opponentPoints"></param>
        public void UpdatePoints(int playerPoints, int opponentPoints)
        {
            this.playerPoints.text = playerPoints.ToString();
            this.opponentPoints.text = opponentPoints.ToString();
        }

        /// <summary>
        /// Updates the UI for the turn count.
        /// </summary>
        /// <param name="turn"></param>
        public void UpdateTurn(int turn)
        {
            for (int i = 0; i < this.turn.Length; i++)
            {
                this.turn[i].gameObject.SetActive(turn > i);
            }
        }

        /// <summary>
        /// Updates the UI of the mana amount.
        /// </summary>
        /// <param name="mana"></param>
        public void UpdateMana(int mana)
        {
            this.mana.text = mana.ToString();
        }
        
    #endregion
}
