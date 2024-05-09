using System;
using UnityEngine;

public class ServerSetup : MonoBehaviour
{
    #region Variables

        [Header("Validation Engine")]
        [SerializeField] private int deckSize;
        private ValidationEngine validationEngine;

        [Header("Server Engine")]
        [SerializeField] private int startingHandAmount;
        private ServerEngine serverEngine;

    #endregion
    
    #region Unity Methods

        private void Awake()
        {
            this.serverEngine = new ServerEngine(this.startingHandAmount);
            this.validationEngine = new ValidationEngine(this.serverEngine, this.deckSize);
        }

        private void OnDestroy()
        {
            this.validationEngine.OnDestroy();
            this.serverEngine.OnDestroy();
        }

    #endregion
}
