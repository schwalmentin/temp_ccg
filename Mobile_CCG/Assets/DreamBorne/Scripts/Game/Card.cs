using TMPro;
using UnityEngine;

public class Card : MonoBehaviour
{
    #region Variables

        private int id;
        private int uniqueId;
        private new string name;
        private int cost;
        private int power;
        private int actionId;

        [Header("Utility")]
        [SerializeField] private bool useGraphic;
        
        [Header("GUI")]
        [SerializeField] private TextMeshProUGUI nameGui;
        [SerializeField] private TextMeshProUGUI costGui;
        [SerializeField] private TextMeshProUGUI powerGui;

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
        public void Initialize(int id, int uniqueId, string name, int cost, int power, int actionId)
        {
            this.id = id;
            this.uniqueId = uniqueId;
            this.name = name;
            this.cost = cost;
            this.power = power;
            this.actionId = actionId;
            
            this.UpdateGraphic();
        }

        /// <summary>
        /// Updates all information on a card.
        /// </summary>
        private void UpdateGraphic()
        {
            if (!this.useGraphic)
            {
                return;
            }

            this.nameGui.text = this.name;
            this.costGui.text = this.cost.ToString();
            this.powerGui.text = this.power.ToString();
        }

    #endregion
}
