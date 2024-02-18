using Unity.Netcode;
using UnityEngine;

public struct PlayedCard : INetworkSerializeByMemcpy
{
    public PlayedCard(Card card, Vector2Int fieldPosition, int cardSlotAmount)
    {
        this.card = card;
        this.fieldPosition = fieldPosition;
        this.cardSlotAmount = cardSlotAmount;
    }

    public Card card { get; }
    public int cardSlotAmount { get; }
    public Vector2Int fieldPosition { get; }
}
