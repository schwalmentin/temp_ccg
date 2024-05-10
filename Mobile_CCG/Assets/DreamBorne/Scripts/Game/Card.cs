using System;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    #region Variables

        private int id;
        private int uniqueId;
        private string cardName;
        private int cost;
        private int power;
        private string actionId;

        private CardState cardState;

        [Header("Graphic")]
        [SerializeField] private bool useGraphic;
        [SerializeField] private GameObject highlights;
        
        [Header("GUI")]
        [SerializeField] private TextMeshProUGUI nameGui;
        [SerializeField] private TextMeshProUGUI costGui;
        [SerializeField] private TextMeshProUGUI powerGui;
        
        // Properties
        public int Id => this.id;
        public int UniqueId => this.uniqueId;
        public int Cost => this.cost;
        public int Power => this.power;
        public string ActionId => this.actionId;
        public CardState CardState
        {
            get { return this.cardState; }
            set { this.cardState = value; }
        }
        public string ActionParams { get; set; }

    #endregion

    #region Card Methods

        /// <summary>
        /// Initializes the card by setting all needed parameters and updating the graphic.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uniqueId"></param>
        /// <param name="name"></param>
        /// <param name="cost"></param>
        /// <param name="power"></param>
        /// <param name="actionId"></param>
        public void Initialize(int id, int uniqueId, string name, int cost, int power, string actionId)
        {
            this.id = id;
            this.uniqueId = uniqueId;
            this.name = this.cardName = name;
            this.cost = cost;
            this.power = power;
            this.actionId = actionId;

            this.cardState = CardState.Library;
            
            this.UpdateGraphic();
        }

        /// <summary>
        /// Updates all information on a card.
        /// </summary>
        public void UpdateGraphic()
        {
            if (!this.useGraphic)
            {
                return;
            }

            this.nameGui.text = this.cardName;
            this.costGui.text = this.cost.ToString();
            this.powerGui.text = this.power.ToString();
        }

        /// <summary>
        /// Displays a detailed version of the card including all important information about it.
        /// (Only prints the card name and its action for now!)
        /// </summary>
        public void ShowInfo()
        {
            Debug.Log($"{this.cardName}: {this.actionId}");
        }

        /// <summary>
        /// Toggles the highlight graphic based on the parameter.
        /// </summary>
        /// <param name="isActive"></param>
        public void ToggleHighlight(bool isActive)
        {
            this.highlights.SetActive(isActive);
        }

    #endregion
}

public enum CardState
{
    Library,
    Hand,
    Field
}