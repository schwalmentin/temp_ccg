using UnityEngine;

public struct PlayedCard
{
    public PlayedCard(Card card, Vector2Int fieldPosition)
    {
        this.card = card;
        this.fieldPosition = fieldPosition;
    }

    public Card card { get; }
    public Vector2Int fieldPosition { get; }
}
