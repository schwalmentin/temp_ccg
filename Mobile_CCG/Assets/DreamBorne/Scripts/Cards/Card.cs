using UnityEngine;

public abstract class Card : MonoBehaviour
{
    [Header("Card Attributes")]
    [SerializeField] protected string cardName;
    [SerializeField] protected uint uniqueId;
    protected int cardId;

    public abstract void InitializeCard(bool includeGraphic);
}

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
