using System;
using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    #region Variables

        private int cost;
        private int power;

        [Header("Graphic")]
        [SerializeField] private bool useGraphic;
        [SerializeField] private GameObject highlights;
        
        [Header("GUI")]
        [SerializeField] private TextMeshProUGUI nameGui;
        [SerializeField] private TextMeshProUGUI costGui;
        [SerializeField] private TextMeshProUGUI powerGui;
        
        // Properties
        public int Id { get; private set; }
        public int UniqueId { get; private set; }
        public string Name
        {
            get => this.name;
            set
            {
                this.name = value;
                this.nameGui.text = this.name;
            }
        }
        public int Cost
        {
            get => this.cost;
            set
            {
                this.cost = value;
                this.costGui.text = this.cost.ToString();
            }
        }
        public int Power
        {
            get => this.power;
            set
            {
                this.power = value;
                this.powerGui.text = this.power.ToString();
            }
        }
        public string Description { get; private set; }
        public string ActionId { get; private set; }

        public CardState CardState { get; set; }
        public string ActionParams { get; set; }
        public bool PerformOpponent { get; set; }

    #endregion

    #region Card Methods

        /// <summary>
        /// Initializes the card by setting all needed parameters.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uniqueId"></param>
        /// <param name="name"></param>
        /// <param name="cost"></param>
        /// <param name="power"></param>
        /// <param name="description"></param>
        /// <param name="actionId"></param>
        public void Initialize(int id, int uniqueId, string name, int cost, int power, string description, string actionId)
        {
            this.Id = id;
            this.UniqueId = uniqueId;
            this.Name = name;
            this.Cost = cost;
            this.Power = power;
            this.Description = description;
            this.ActionId = actionId;

            this.CardState = CardState.Library;
            this.PerformOpponent = false;
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