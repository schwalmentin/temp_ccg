using System.Collections.Generic;
using UnityEngine;

public class ServerSetup : MonoBehaviour
{
    #region Variables

        [Header("Validation Engine")]
        [SerializeField] private int deckSize;
        private ValidationEngine validationEngine;

        [Header("Server Engine")]
        [SerializeField] private int startingHandAmount;
        [SerializeField] private int maxTurnAmount;
        [SerializeField] private GameObject[] cardHolders;
        private ServerEngine serverEngine;

    #endregion
    
    #region Unity Methods

        private void Awake()
        {
            this.serverEngine = new ServerEngine(this.startingHandAmount, this.maxTurnAmount, new Stack<GameObject>(this.cardHolders));
            this.validationEngine = new ValidationEngine(this.serverEngine, this.deckSize);
        }

        private void OnDestroy()
        {
            this.validationEngine.OnDestroy();
            this.serverEngine.OnDestroy();
        }

    #endregion
}
