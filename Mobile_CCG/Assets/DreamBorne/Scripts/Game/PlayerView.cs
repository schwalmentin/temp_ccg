using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerView : MonoBehaviour
{
    #region Variables

    private ulong localClientId;
    private string userName;

    [Header("Player UI")]
    [SerializeField] private GameObject playerCanvas;
    [Space]
    [SerializeField] private TextMeshProUGUI playerName;
    [SerializeField] private TextMeshProUGUI playerCurrency;
    [Space]
    [SerializeField] private TextMeshProUGUI opponentName;
    [SerializeField] private TextMeshProUGUI opponentCurrency;
    [Space]
    [SerializeField] private TextMeshProUGUI turnInformation;
    [SerializeField] private TextMeshProUGUI turnCount;

    [Header("UI Buttons")]
    [SerializeField] private Button increaseCurrencyButton;
    [SerializeField] private Button passTurnButton;

    [Header("End Game UI")]
    [SerializeField] private GameObject endGameCanvas;
    [Space]
    [SerializeField] private TextMeshProUGUI endGameInformation;

    [Header("Animations")]
    [SerializeField] private Animator warningAnimator;

    private int currencyAmount;
    private int opponentCurrencyAmount;

    private int currentTurnCount;
    private string currentTurnInformation;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        this.userName = PlaytestManager.Instance.GetRandomName();
        this.playerName.text = userName;
        this.localClientId = NetworkManager.Singleton.LocalClientId;

        EventManager.Instance.updClientJoinedGameMaster += this.updOnJoin;
        EventManager.Instance.updClientPassTurnEvent += this.updPassTurn;
        EventManager.Instance.updClientCurrencyEvent += this.updOpponentCurrency;
        EventManager.Instance.updClientHardResetEvent += this.updHardReset;
        EventManager.Instance.updEndGameEvent += this.updEndGame;
    }

    private void Start()
    {
        EventManager.Instance.JoinGameMasterServerRpc(this.localClientId, userName);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            EventManager.Instance.LogMessageServerRpc("Hello World!");
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            this.passTurnButton.interactable = true;
            this.increaseCurrencyButton.interactable = true;
            Debug.LogWarning("You just enabled cheatcodes. Congratulations!");
        }
    }

    private void OnDestroy()
    {
        EventManager.Instance.updClientJoinedGameMaster -= this.updOnJoin;
        EventManager.Instance.updClientPassTurnEvent -= this.updPassTurn;
        EventManager.Instance.updClientCurrencyEvent -= this.updOpponentCurrency;
        EventManager.Instance.updClientHardResetEvent -= this.updHardReset;
        EventManager.Instance.updEndGameEvent -= this.updEndGame;
    }

    #endregion

    #region PlayerView Methods

    /// <summary>
    /// Updates all of the information of the UI (playername, cash amount, etc.) and activates/deactivates the UI buttons.
    /// </summary>
    /// <param name="buttonsActivated"></param>
    private void UpdateInfoUI(bool buttonsActivated)
    {
        this.turnCount.text = this.currentTurnCount.ToString();
        this.turnInformation.text = this.currentTurnInformation;

        this.playerCurrency.text = this.currencyAmount.ToString() + " $";
        this.opponentCurrency.text = this.opponentCurrencyAmount.ToString() + " $";

        this.increaseCurrencyButton.interactable = buttonsActivated;
        this.passTurnButton.interactable = buttonsActivated;
    }

    #endregion

    #region Actions

    /// <summary>
    /// From the editor callable method to pass the turn to the other player.
    /// </summary>
    public void PassTurn()
    {
        // Update UI Info
        this.currentTurnCount++;
        this.currentTurnInformation = "Opponents Turn.";

        this.UpdateInfoUI(false);

        // Send server rpc
        EventManager.Instance.PassTurnServerRpc(this.localClientId);
    }

    /// <summary>
    /// From the editor callable method to increase the cash amount by one.
    /// </summary>
    public void IncreaseCurrency()
    {
        this.currencyAmount++;
        this.UpdateInfoUI(true);

        Debug.Log("The Player wants to increase the currency!");
        EventManager.Instance.IncreaseCurrencyServerRpc(this.localClientId, 1);
    }

    /// <summary>
    /// From the editor callable method switch to the lobby scene.
    /// </summary>
    public void BackToLobby()
    {
        CustomSceneManager.Instance.SwitchScene("Lobby");
    }

    #endregion

    #region Incoming Events

    /// <summary>
    /// A method that subscribes to the EventManager to receive the update when all players have joined the match.
    /// </summary>
    /// <param name="opponentName"></param>
    /// <param name="isStarting"></param>
    private void updOnJoin(string opponentName, bool isStarting)
    {
        this.opponentName.text = opponentName;
        this.currentTurnInformation = isStarting ? "Your Turn." : "Opponents Turn.";

        this.UpdateInfoUI(isStarting);
    }

    /// <summary>
    /// A method that subscribes to the EventManager to receive an update when the opponent passes the turn.
    /// </summary>
    private void updPassTurn()
    {
        this.currentTurnCount++;
        this.currentTurnInformation = "Your Turn.";

        this.UpdateInfoUI(true);
    }

    /// <summary>
    /// A method that subscribes to the EventManager to receive an update when the opponent increases their currency amount.
    /// </summary>
    /// <param name="amount"></param>
    private void updOpponentCurrency(int amount)
    {
        Debug.Log($"Im gonna update the opponents currency to {amount}$!");
        this.opponentCurrencyAmount = amount;
        this.UpdateInfoUI(false);
    }

    /// <summary>
    /// A method that subscribes to the EventManager to receive the update when the player is completely reset by the GameMaster.
    /// </summary>
    /// <param name="playerName"></param>
    /// <param name="opponentName"></param>
    /// <param name="playerAmount"></param>
    /// <param name="opponentAmount"></param>
    /// <param name="yourTurn"></param>
    /// <param name="currentTurn"></param>
    private void updHardReset(string playerName, string opponentName, int playerAmount, int opponentAmount, bool yourTurn, int currentTurn)
    {
        this.playerName.text = playerName;
        this.opponentName.text = opponentName;
        this.currencyAmount = playerAmount;
        this.opponentCurrencyAmount = opponentAmount;
        this.currentTurnCount = currentTurn;
        this.UpdateInfoUI(yourTurn);

        this.warningAnimator.SetTrigger("warning");
    }

    /// <summary>
    /// A method that subscribes to the EventManager to receive the update when the game is ending.
    /// </summary>
    /// <param name="isWinning"></param>
    private void updEndGame(bool isWinning)
    {
        // F3C041 EE7264
        this.playerCanvas.SetActive(false);
        this.endGameCanvas.SetActive(true);

        if (isWinning )
        {
            this.endGameInformation.text = "You won the game!";
            // this.endGameInformation.color = new Color(243 / 255, 192 / 255, 65 / 255);
            this.endGameInformation.color = Color.yellow;
            return;
        }

        this.endGameInformation.text = "You lost the game!";
        // this.endGameInformation.color = new Color(238 / 255, 114 / 255, 100 / 255);
        this.endGameInformation.color = Color.red;
    }

    #endregion
}
