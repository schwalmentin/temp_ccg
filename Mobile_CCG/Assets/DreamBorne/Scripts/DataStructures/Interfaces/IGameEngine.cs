using System.Collections.Generic;

public interface IGameEngine
{
    void PassTurn(Stack<PlayedCard> playedCards);
}
