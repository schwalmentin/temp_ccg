using UnityEngine;

public abstract class Card : MonoBehaviour
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

    public abstract void InitializeCard(bool includeGraphic);

    public abstract void DisplayCardInfo();

    public abstract void OnSelect();
    public abstract void OnDeselect();
}
