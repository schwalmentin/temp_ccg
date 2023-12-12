using System;
using System.Collections.Generic;
using UnityEngine;

public class PlaytestManager : NetworkSingleton<PlaytestManager>
{
    #region Variables

    [SerializeField] private GameObject gameScene;

    private List<string> namePool = new List<string>() { "Ratte", "El Gato", "Gold Fisch", "Zitteraal", "Gatze", "Heiko", "Rüdiger", "Crash", "Eddie", "Pfirsiche" };
    private int idCounter = 0;

    #endregion

    #region Unity Methods

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        this.gameScene.SetActive(true);

        GameMaster gameMaster = FindObjectOfType<GameMaster>();

        if (gameMaster != null && !this.IsServer)
        {
            Destroy(gameMaster.gameObject);
        }
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
        string name = this.namePool[index];
        this.namePool.Remove(name);

        return name;
    }

    /// <summary>
    /// Returns a "unique" id that is incrementally increased.
    /// </summary>
    /// <returns></returns>
    public ulong GetIncrementalId()
    {
        this.idCounter++;
        return Convert.ToUInt64(this.idCounter);
    }

    #endregion
}
