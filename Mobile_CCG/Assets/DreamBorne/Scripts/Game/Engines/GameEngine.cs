using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : IGameEngine
{
    private IPassTurn passTurn;

    public GameEngine(IPassTurn passTurn)
    {
        this.passTurn = passTurn;
    }

    public void PassTurn(Stack<PlayedCard> playedCards)
    {
        this.passTurn.Execute(playedCards);
    }
}
