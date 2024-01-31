using System.Collections.Generic;

public interface IPassTurn
{
    void Execute(Stack<PlayedCard> playedCards);
}
