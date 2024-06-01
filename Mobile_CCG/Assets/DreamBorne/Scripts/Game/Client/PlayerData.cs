using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    #region Variables
    
        // Field
        [Header("Field")]
        [SerializeField] private CardSlot[] playerCardSlots;
        [SerializeField] private CardSlot[] opponentCardSlots;
        public CardSlot[,] PlayerField { get; private set; }
        public CardSlot[,] OpponentField { get; private set; }

        // Cards
        [Header("Deck")]
        [SerializeField] private int[] deckIds;
        public int[] Deck => this.deckIds;
        public List<Card> Hand { get; private set; }
        public Stack<CardSlot> PlayedCards { get; private set; }

        public PlayerPhase PlayerPhase { get; set; }
        
        // Game Data
        public int Turn
        {
            get => this.turn;
            set
            {
                this.turn = value;
                for (int i = 0; i < this.turnGui.Length; i++)
                {
                    this.turnGui[i].gameObject.SetActive(this.turn > i);
                }
            }
        }
        private int turn;
        
        public int Mana
        {
            get => this.mana;
            set
            {
                this.mana = value;
                this.manaGui.text = this.mana.ToString();
            }
        }
        private int mana;

        public int PlayerPoints
        {
            get => this.playerPoints;
            set
            {
                this.playerPoints = value;
                this.playerPointsGui.text = this.playerPoints.ToString();
            }
        }
        private int playerPoints;

        public int OpponentPoints
        {
            get => this.opponentPoints;
            set
            {
                this.opponentPoints = value;
                this.opponentPointsGui.text = this.opponentPoints.ToString();
            }
        }
        private int opponentPoints;
        
        public string PlayerName
        {
            get => this.playerName;
            set
            {
                this.playerName = value;
                this.playerNameGui.text = this.playerName;
            }
        }
        private string playerName;
        
        public string OpponentName
        {
            get => this.opponentName;
            set
            {
                this.opponentName = value;
                this.opponentNameGui.text = this.opponentName;
            }
        }
        private string opponentName;

        // Game UI
        [Header("Game UI")]
        [SerializeField] private TextMeshProUGUI playerNameGui;
        [SerializeField] private Image playerIcon;
        [SerializeField] private TextMeshProUGUI playerPointsGui;
        [Space]
        [SerializeField] private TextMeshProUGUI opponentNameGui;
        [SerializeField] private Image opponentIcon;
        [SerializeField] private TextMeshProUGUI opponentPointsGui;
        [Space]
        [SerializeField] private TextMeshProUGUI manaGui;
        [SerializeField] private Image[] turnGui;
        [Space]
        [SerializeField] private Button passTurnButton;
        [SerializeField] private Button undoButton;
        [Space]
        [SerializeField] private GameObject cardInformation;
        [SerializeField] private TextMeshProUGUI infoName;
        [SerializeField] private TextMeshProUGUI infoPower;
        [SerializeField] private TextMeshProUGUI infoCost;
        [SerializeField] private TextMeshProUGUI infoAbility;
            
        // Game UI Properties
        public Button PassTurnButton => this.passTurnButton;
        public Button UndoButton => this.undoButton;
        public GameObject CardInformation => this.cardInformation;
        public TextMeshProUGUI InfoName => this.infoName;
        public TextMeshProUGUI InfoPower => this.infoPower;
        public TextMeshProUGUI InfoCost => this.infoCost;
        public TextMeshProUGUI InfoAbility => this.infoAbility;

    #endregion

    #region Unity Methods

        private void Awake()
        {
            // Initialize Fields
            this.PlayerField = this.CardSlotsToField(this.playerCardSlots, new Vector2Int(2, 3));
            this.OpponentField = this.CardSlotsToField(this.opponentCardSlots, new Vector2Int(2, 3));
            
            // Initialize Cards
            this.Hand = new List<Card>();
            this.PlayedCards = new Stack<CardSlot>();
            
            // UI
            this.UndoButton.interactable = false;
            this.PlayerName = PlaytestManager.Instance.GetRandomName();
            
            // Initialize player
            this.PlayerPhase = PlayerPhase.Deploy;
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
                this.playerCardSlots[i].FieldPosition = new Vector2Int(x, y);
            }

            return field;
        }
        
    #endregion
}
