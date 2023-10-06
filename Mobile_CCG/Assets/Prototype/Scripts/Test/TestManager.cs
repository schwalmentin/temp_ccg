using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TestManager : NetworkBehaviour
{
    [SerializeField] private GameObject testPlayerPrefab;

    private void Awake()
    {
        SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
        print("!!!!!!!!!!!!!!!!");
    }

    //public override void OnNetworkSpawn()
    //{
    //    SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId);
    //    print("????????????????????????????");
    //}

    [ServerRpc(RequireOwnership = false)]
    private void SpawnPlayerServerRpc(ulong playerId)
    {
        print("SPAAAAAAAAAAAAWN");
        var spawn = Instantiate(testPlayerPrefab);
        spawn.GetComponent<NetworkObject>().SpawnWithOwnership(playerId);
        
    }
}
