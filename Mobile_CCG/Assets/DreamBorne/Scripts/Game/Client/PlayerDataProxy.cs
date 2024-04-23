using UnityEngine;

[RequireComponent(typeof(PlayerData))]
public class PlayerDataProxy : MonoBehaviour
{
    #region Variables

        private PlayerData playerData;
        
        // Properties
        public int Mana => this.playerData.currentMana;

    #endregion

    #region Unity Methods

        private void Awake()
        {
            this.playerData = this.GetComponent<PlayerData>();
        }

    #endregion
}
