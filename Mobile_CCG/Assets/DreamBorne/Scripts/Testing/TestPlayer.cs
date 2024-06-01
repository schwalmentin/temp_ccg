using Unity.Netcode;
using UnityEngine;

public class TestPlayer : NetworkBehaviour
{
    private void Update()
    {
        if (!this.IsOwner) { return; }

        if (Input.GetKeyDown(KeyCode.S))
        {
            // Server RPC
            this.sServerRPC();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            // Client RPC
            this.cClientRPC();
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            // Client RPC
            this.xServerRPC(" parameters are possible");
        }
    }

    [ServerRpc]
    private void sServerRPC()
    {
        Debug.Log($"This is a server rpc send from: {AuthenticationManager.Instance.PlayerId}");
    }

    [ServerRpc]
    private void xServerRPC(string test)
    {
        Debug.Log("x Server RPC" + test);
        this.cClientRPC();
    }

    [ClientRpc]
    private void cClientRPC()
    {
        Debug.Log($"This is a client rpc send from: {AuthenticationManager.Instance.PlayerId}");
    }
}
