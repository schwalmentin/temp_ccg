using System.Collections.Generic;

public class GameContainerProxy
{
    private GameContainer gameContainer;

    public GameContainerProxy(GameContainer gameContainer)
    {
        this.gameContainer = gameContainer;
    }

    public GameState GameState { get { return this.gameContainer.gameState; } }

    public List<Card> HandCards
    {
        get { return this.gameContainer.hand; }
    }
}
