using Unity.Netcode;
using UnityEngine;

public abstract class Card : MonoBehaviour, INetworkSerializable
{
    [Header("Card Attributes")]
    [SerializeField] protected string cardName;
    [SerializeField] protected uint cardId; // Which type of card
    protected uint uniqueId; // Which instance of a type of card
    protected CardState cardState = CardState.Library;

    public CardState CardState
    { 
        get { return this.cardState; }
        set { this.cardState = value; }
    }

    public string CardName { get { return this.cardName; } }

    public uint CardId { get { return this.cardId; } }

    public abstract void InitializeCard(bool renderCard);

    public abstract void DisplayCardInfo();

    public abstract void OnSelect();
    public abstract void OnDeselect();

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref this.cardName);
        serializer.SerializeValue(ref this.cardId);
        serializer.SerializeValue(ref this.uniqueId);
    }
}
