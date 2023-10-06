using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;

public class PlayerController : NetworkBehaviour
{
    [Header("UI Information")]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerCurrency;
    [Space]
    [SerializeField] private TextMeshProUGUI opponentName;
    [SerializeField] private TextMeshProUGUI opponentCurrency;
    [Space]
    [SerializeField] private TextMeshProUGUI turnInfo;
    [SerializeField] private TextMeshProUGUI turnCount;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            this.playerName.gameObject.SetActive(false);
            this.playerCurrency.gameObject.SetActive(false);

            this.opponentName.gameObject.SetActive(false);
            this.opponentCurrency.gameObject.SetActive(false);

            this.turnInfo.gameObject.SetActive(false);
            this.turnCount.gameObject.SetActive(false);
            Destroy(this);
        }
    }

    public void IncreaseCurrency(int amount)
    {

    }

    public void PassTurn()
    {

    }
}
