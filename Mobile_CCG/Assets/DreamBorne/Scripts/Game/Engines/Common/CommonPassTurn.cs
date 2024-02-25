using System.Collections.Generic;
using UnityEngine;

public class CommonPassTurn : IPassTurn
{
    private GameContainer gameContainer;

    public CommonPassTurn(GameContainer gameContainer)
    {
        this.gameContainer = gameContainer;
    }

    public void Execute(Stack<PlayedCard> playedCards)
    {
        //foreach (var playedCard in playedCards)
        //{
        //    Vector2Int fieldPosition = playedCard.fieldPosition;
        //    Card card = playedCard.card;

        //    this.gameContainer.playedCards.Add(card);

        //    if (card.GetType() == typeof(Nightmare))
        //    {
        //        this.gameContainer.invaderField[fieldPosition.x, fieldPosition.y] = card;
        //        return;
        //    }

        //    if (card.GetType() == typeof(Guardian))
        //    {
        //        Guardian guardian = (Guardian)card;
        //        for (int i = 0; i < guardian.Guards.Count; i++)
        //        {
        //            this.gameContainer.wardenField[fieldPosition.x + i, fieldPosition.y] = guardian.Guards[i];
        //        }
        //        return;
        //    }

        //    if (card.GetType() == typeof(Trap))
        //    {
        //        this.gameContainer.captureField[fieldPosition.x] = card;
        //        return;
        //    }

        //    if (card.GetType() == typeof(Dreamcatcher))
        //    {
        //        this.gameContainer.captureField[fieldPosition.x] = card;
        //        return;
        //    }
        //}
    }
}
