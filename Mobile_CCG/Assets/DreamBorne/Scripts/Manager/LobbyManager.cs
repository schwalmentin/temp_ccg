using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class LobbyManager : NetworkSingleton<LobbyManager>
{
    #region Variables

    private Lobby lobby;
    private int maxPlayers = 2;
    private int maxConnections = 1;

    private readonly List<ulong> playersInLobby = new();

    SkillBasedMatchmaking mm = new SkillBasedMatchmaking();

    [SerializeField] private Rank rankA;
    [SerializeField] private Rank rankB;

    private IEnumerator heartBeatCoroutine;
    private int heartbeatInterval = 15;

    private RelayHostData hostData;
    private RelayJoinData joinData;

    private string playerName;

    public string PlayerName { get { return this.playerName; } }

    #endregion

    #region Unity Methods

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("DESTROYYYYY - is host: " + this.IsHost);
            Debug.Log("DESTROYYYYY - is server: " + this.IsServer);
            Debug.Log("DESTROYYYYY - is client: " + this.IsClient);
        }
    }

    public override void OnDestroy()
    {
        if (this.lobby == null) { return; }

        Lobbies.Instance.DeleteLobbyAsync(this.lobby.Id);
    }

    #endregion

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

            // Options or data should include the player's rating when creating a lobby. When a player looks for a lobby their rating should be -250 to +250 than their rating, alternatively if this does not work, ranks can be implemented and specific ranks are sought after in the querys

            string playerId = AuthenticationManager.Instance.PlayerId;

            SkillBasedMatchmaking.Player loadedPlayer = await mm.LoadData(playerId);

            string rank = loadedPlayer.Rank.ToString();

            var queryFilter = new List<QueryFilter>()
            {
                {
                     new QueryFilter(
                    field: QueryFilter.FieldOptions.S1,
                    op: QueryFilter.OpOptions.EQ,
                    value: rank
                    )
                }
            };

            options.Filter = queryFilter;

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
            Debug.Log("Start Client");
            NetworkManager.Singleton.StartClient();
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

            string playerId = AuthenticationManager.Instance.PlayerId; //how do we get playerId?

            SkillBasedMatchmaking.Player loadedPlayer = await mm.LoadData(playerId);

            string rank = loadedPlayer.Rank.ToString();

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

                {
                "Rank", new DataObject (
                    visibility: DataObject.VisibilityOptions.Public,
                    value: rank,
                    index: DataObject.IndexOptions.S1)
                }
            };

            // Create lobby
            this.lobby = await Lobbies.Instance.CreateLobbyAsync("game_lobby", this.maxPlayers, options);
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
            Debug.Log("Start Host");
            NetworkManager.Singleton.StartHost();
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
            if (this.lobby == null)
            {
                return;
            }

            await LobbyService.Instance.DeleteLobbyAsync(this.lobby.Id);
            Debug.Log($"Cancel lobby: {this.lobby.Id}");

            if (this.heartBeatCoroutine != null) { StopCoroutine(this.heartBeatCoroutine); }
            this.lobby = null;
            NetworkManager.Singleton.Shutdown();

            /*
            string player1Id = 1; //how do we get playerId?

            double outcomePlayer1 = mm.getWinOutcome();

            double outcomePlayer2 = mm.getLossOutcome();

            ulong player2Id = 2; //how do we get playerId?

            SkillBasedMatchmaking.Player loadedPlayer1 = await mm.LoadData(player1Id);

            SkillBasedMatchmaking.Player loadedPlayer2 = await mm.LoadData(player2Id);


            mm.UpdateRatingAsync(player1Id, player2Id, outcomePlayer1);

            mm.UpdateRatingAsync(player2Id, player1Id, outcomePlayer2);
            */

        }
        catch (LobbyServiceException e)
        {
            Debug.LogException(e);
        }
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

    #region Network Methods

    /// <summary>
    /// Is called once the player spawns in the network. The server adds an OnClientCallback() event.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (this.IsServer)
        {
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
            this.playersInLobby.Add(NetworkManager.Singleton.LocalClientId);
        }
    }

    /// <summary>
    /// Adds the playerId to the lobby and starts the game once there are two players in the same lobby.
    /// </summary>
    /// <param name="playerId"></param>
    private void OnClientConnectedCallback(ulong playerId)
    {
        if (!this.IsServer) { return; }

        if (!this.playersInLobby.Contains(playerId))
        {
            this.playersInLobby.Add(playerId);
        }

        if (this.playersInLobby.Count == 2)
        {
            Debug.Log("Start the Game");

            // Switch to the game scene for all players
            CustomSceneManager.Instance.SwitchNetworkScene("GameScene");
        }
    }

    #endregion
}
