using System.Collections.Generic;
using UnityEngine;

public class GameContainer
{
    public int dreamsandAmount;
    public int turnCount;
    public GameState gameState;

    public List<Card> hand;
    public Stack<Card> library;
    public Stack<Card> discard;

    public Card[,] invaderField;
    public Guard[,] wardenField;
    public Card[] captureField;

    public List<Card> playedCards;

    public GameContainer(List<Card> deckList)
    {
        this.dreamsandAmount = 0;
        this.turnCount = 1;
        this.gameState = GameState.Start;

        deckList.Shuffle();
        this.library = new Stack<Card>(deckList);
        this.hand = new List<Card>();
        this.discard = new Stack<Card>();

        this.invaderField = new Card[6,2];
        this.wardenField = new Guard[9,4];
        this.captureField = new Card[3];

        this.playedCards = new List<Card>();
    }
}
