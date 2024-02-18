using System.Collections.Generic;
using UnityEngine;

public class PlayerHub : MonoBehaviour
{
    #region Variables

    // General
    [SerializeField] private List<Card> deckList;
    private IGameEngine gameEngine;
    private GameContainer gameContainer;
    private GameContainerProxy gameContainerProxy;

    // Commands
    private CommonPassTurn passTurn;

    // Data
    // private Stack<PlayedCard> playedCards;

    // Properties
    // public GameContainerProxy GameContainerProxy { get { return this.gameContainerProxy; } }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        // Initialize played cards
        // playedCards = new Stack<PlayedCard>();

        // Initialize game container
        this.gameContainer = new GameContainer(this.deckList);
        this.gameContainerProxy = new GameContainerProxy(this.gameContainer);

        // Initialize commands
        this.passTurn = new CommonPassTurn(this.gameContainer);

        // Initialize game engine with the given commands
        this.gameEngine = new GameEngine(passTurn);
    }

    #endregion

    #region Input Methods

    public void PassTurn(List<PlayedCard> playedCards)
    {
        // EventManager.Instance.PassTurnServerRpc();
    }

    public void PlayCard(Card card, Vector2Int fieldPosition)
    {
        // this.playedCards.Push(new PlayedCard(card, fieldPosition));
    }

    public void UnplayCard()
    {
        // if (this.playedCards.Count < 1) {  return; }

        // this.playedCards.Pop();
    }

    #endregion

    #region Server Notifications
    #endregion
}
