using Unity.Netcode;
using UnityEngine;

public struct PlayedCard : INetworkSerializeByMemcpy
{
    public PlayedCard(Vector2Int fieldPosition, int cardSlotAmount, uint cardId)
    {
        this.fieldPosition = fieldPosition;
        this.cardSlotAmount = cardSlotAmount;
        this.cardId = cardId;
    }

    public uint cardId { get; }
    public int cardSlotAmount { get; }
    public Vector2Int fieldPosition { get; }
}
