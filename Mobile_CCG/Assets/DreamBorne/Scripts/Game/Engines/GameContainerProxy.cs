public class GameContainerProxy
{
    private GameContainer gameContainer;

    public GameContainerProxy(GameContainer gameContainer)
    {
        this.gameContainer = gameContainer;
    }

    public GameState GameState { get { return this.gameContainer.gameState; } }
}
