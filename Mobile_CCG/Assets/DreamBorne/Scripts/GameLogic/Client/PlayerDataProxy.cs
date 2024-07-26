using UnityEngine;

[RequireComponent(typeof(PlayerData))]
public class PlayerDataProxy : MonoBehaviour
{
    #region Variables

        private PlayerData playerData;
        
        // Properties
        public int Mana => this.playerData.Mana;
        public PlayerPhase PlayerPhase => this.playerData.PlayerPhase;

    #endregion

    #region Unity Methods

        private void Awake()
        {
            this.playerData = this.GetComponent<PlayerData>();
        }

    #endregion
}
