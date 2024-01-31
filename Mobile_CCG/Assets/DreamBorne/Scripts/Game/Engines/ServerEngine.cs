using System;
using System.Collections.Generic;
using UnityEngine;

public class ServerEngine : IGameEngine
{
    private readonly IDoIt doit;
    private readonly IDraw draw;

    public ServerEngine(IDoIt doit, IDraw draw)
    {
        this.doit = doit;
        this.draw = draw;
    }

    public void DrawCards(int cardAmount)
    {
        draw.DrawCards(cardAmount);
        throw new System.NotImplementedException();
    }

    public void PassTurn()
    {
        doit.Do();
        throw new System.NotImplementedException();
    }

    public void PassTurn(Stack<PlayedCard> playedCards)
    {
        throw new NotImplementedException();
    }
}

public interface IDraw
{
    public void DrawCards(int cardAmount);
}

public interface IDoIt
{
    public void Do();
}

public class Draw : IDraw
{
    void IDraw.DrawCards(int cardAmount)
    {
        throw new System.NotImplementedException();
    }
}

public static class GameContainerFactory
{
    private static GameContainer gameContainer = new GameContainer(new List<Card>());
    public static GameContainer GetGameContainer() { return gameContainer; }
}

public class DoIt : IDoIt
{
    private GameContainer gameContainer = GameContainerFactory.GetGameContainer();

    public DoIt(GameContainer gameContainer)
    {
        
    }

    void IDoIt.Do()
    {
        
        throw new System.NotImplementedException();
    }
}