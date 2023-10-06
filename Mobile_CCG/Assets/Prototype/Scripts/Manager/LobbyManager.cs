using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    #region Variables

    [SerializeField] private TextMeshProUGUI lobbyinfo;
    [SerializeField] private TextMeshProUGUI joincode;

    private string lobbyId;
    private string lobbyName = "game_lobby";
    private int maxPlayers = 2;
    private int heartbeatInterval = 15;
    private IEnumerator heartBeatCoroutine;

    private int maxConnections = 1;


    private RelayHostData hostData;
    private RelayJoinData joinData;
    #endregion

    #region Unity Methods

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnDestroy()
    {
        Lobbies.Instance.DeleteLobbyAsync(lobbyId);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            PongClientRpc(Time.frameCount, "hello, world"); // Server -> Client
        }
    }
    #endregion

    [ClientRpc]
    void PongClientRpc(int somenumber, string sometext) { Debug.Log($"some number: {somenumber} + some text: {sometext}"); }

    #region Lobby Methods

    /// <summary>
    /// Try to find an existing lobby and joining with the quickjoin options. If there is no lobby create one.
    /// </summary>
    public async void JoinMatch()
    {
        Debug.Log("Looking for a lobby...");

        try
        {
            // Add lobby options
            QuickJoinLobbyOptions options = new QuickJoinLobbyOptions();

            // Quckjoin a random lobby
            Lobby lobby = await Lobbies.Instance.QuickJoinLobbyAsync(options);

            Debug.Log($"Joined lobby: {lobby.Id}");
            Debug.Log($"Lobby players: {lobby.Players.Count}");

            // Retrieve join code from host to create a join allocation
            string joinCode = lobby.Data["joinCode"].Value;

            Debug.Log($"Received join code: {joinCode}");

            JoinAllocation allocation = await Relay.Instance.JoinAllocationAsync(joinCode);

            // Create relay object
            this.joinData = new RelayJoinData
            {
                key = allocation.Key,
                port = (ushort)allocation.RelayServer.Port,
                allocationId = allocation.AllocationId,
                allocationIdBytes = allocation.AllocationIdBytes,
                connectionData = allocation.ConnectionData,
                hostConnectionData = allocation.HostConnectionData,
                ipV4Address = allocation.RelayServer.IpV4
            };

            // Set transport data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                this.joinData.ipV4Address,
                this.joinData.port,
                this.joinData.allocationIdBytes,
                this.joinData.key,
                this.joinData.connectionData,
                this.joinData.hostConnectionData);

            // Start client
            NetworkManager.Singleton.StartClient();
            this.lobbyinfo.text = lobby.Id;
            this.joincode.text = joinCode;

            // Call RPC
            Debug.Log("Switching Scene");
            this.SwitchSceneServerRPC();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log($"Cannot find a lobby: {e}");

            // If there is no lobby to join create one
            CreateMatch();
        }
    }

    /// <summary>
    /// Create a new lobby and ping it every view seconds.
    /// </summary>
    private async void CreateMatch()
    {
        try
        {
            // Create relay object
            Allocation allocation = await Relay.Instance.CreateAllocationAsync(this.maxConnections);
            this.hostData = new RelayHostData
            { 
                key = allocation.Key,
                port = (ushort) allocation.RelayServer.Port,
                allocationId = allocation.AllocationId,
                allocationIdBytes = allocation.AllocationIdBytes,
                connectionData = allocation.ConnectionData,
                ipV4Address = allocation.RelayServer.IpV4
            };

            // Retrieve join code
            hostData.joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // Add lobby options
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = false;
            options.Data = new Dictionary<string, DataObject>()
            {
                {
                    "joinCode", new DataObject(DataObject.VisibilityOptions.Member, hostData.joinCode)
                },
            };

            // Create lobby
            Lobby lobby = await Lobbies.Instance.CreateLobbyAsync(this.lobbyName, this.maxPlayers, options);
            this.lobbyId = lobby.Id;

            Debug.Log($"Create lobby: {lobby.Id}");

            // Ping lobby
            this.heartBeatCoroutine = this.HeartbeatLobbyCoroutine(lobby.Id, this.heartbeatInterval);
            StartCoroutine(this.heartBeatCoroutine);

            // Set transport data
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                hostData.ipV4Address,
                hostData.port,
                hostData.allocationIdBytes,
                hostData.key,
                hostData.connectionData
            );

            // Start host
            NetworkManager.Singleton.StartHost();
            this.lobbyinfo.text = lobby.Id;
            this.joincode.text = hostData.joinCode;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
            throw;
        }
    }

    /// <summary>
    /// Shuts down the current lobby and stops the heartbeat.
    /// </summary>
    public async void CloseMatch()
    {
        try
        {
            if (lobbyId == null)
            {
                return;
            }

            await LobbyService.Instance.DeleteLobbyAsync(this.lobbyId);
            Debug.Log($"Cancel lobby: {lobbyId}");

            if (this.heartBeatCoroutine != null) { StopCoroutine(this.heartBeatCoroutine); }
            this.lobbyId = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
    }

    [ServerRpc]
    private void SwitchSceneServerRPC()
    {
        Debug.Log("Switching Scene");
    }

    /// <summary>
    /// Ping a lobby based on its lobbyId every waitTimeInSeconds so it doesn't close unexpectedly.
    /// </summary>
    /// <param name="lobbyId"></param>
    /// <param name="waitTimeInSeconds"></param>
    /// <returns></returns>
    private IEnumerator HeartbeatLobbyCoroutine(string lobbyId, int waitTimeInSeconds)
    {
        WaitForSecondsRealtime delay = new WaitForSecondsRealtime(waitTimeInSeconds);

        while (true)
        {
            Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
            Debug.Log("Lobby Heartbeat");
            yield return delay;
        }
    }
    #endregion
}
