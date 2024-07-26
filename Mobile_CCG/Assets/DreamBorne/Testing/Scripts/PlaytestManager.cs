using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaytestManager : NetworkSingleton<PlaytestManager>
{
    #region Variables

        [SerializeField] private GameObject gameScene;

        private List<string> namePool = new List<string>() { "Ratte", "El Gato", "Gold Fisch", "Zitteraal", "Gatze", "Heiko", "Rüdiger", "Crash", "Eddie", "Pfirsiche", "Fischer" };
        private int idCounter = 0;

    #endregion

    #region Unity Methods

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            this.gameScene.SetActive(true);
        }

    #endregion

    #region PlayTestManager Methods

        /// <summary>
        /// Returns a randomly picked name chosen from a pool of 10 names.
        /// </summary>
        /// <returns></returns>
        public string GetRandomName()
        {
            int index = UnityEngine.Random.Range(0, this.namePool.Count - 1);
            string randomName = this.namePool[index];
            this.namePool.Remove(randomName);

            return randomName;
        }

        /// <summary>
        /// Returns a "unique" id that is incrementally increased.
        /// </summary>
        /// <returns></returns>
        public int GetIncrementalId()
        {
            this.idCounter++;
            return this.idCounter;
        }

    #endregion
}
