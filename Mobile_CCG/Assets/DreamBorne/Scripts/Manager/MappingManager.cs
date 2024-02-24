using System.Collections.Generic;
using UnityEngine;

public class MappingManager : Singleton<MappingManager>
{
    [SerializeField] private GameObject[] cardPrefabCollection;
    private Dictionary<uint, GameObject> cardCollection;

    private void Start()
    {
        this.cardCollection = new Dictionary<uint, GameObject>();

        foreach (GameObject cardPrefab in cardPrefabCollection)
        {
            Card card = cardPrefab.GetComponent<Card>();

            if (card == null)
            {
                card = cardPrefab.GetComponentInChildren<Card>();
            }

            if (card == null)
            {
                Debug.LogError($"The card prefab {cardPrefab} does not have a Card component!");
                continue;
            }

            this.cardCollection.Add(card.CardId, cardPrefab);
        }
    }

    public Card CreateCard(uint cardId, bool renderCard)
    {
        GameObject cardPrefab = this.cardCollection[cardId];

        if (cardPrefab == null)
        {
            Debug.LogError($"The card prefab with id {cardId} does not Exist!");
            return null;
        }

        Card card = cardPrefab.GetComponent<Card>();

        if (card == null)
        {
            card = cardPrefab.GetComponentInChildren<Card>();
        }

        GameObject newCardPrefab = Instantiate(cardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
        Card newCard = newCardPrefab.GetComponent<Card>();

        if (newCard == null)
        {
            newCard = cardPrefab.GetComponentInChildren<Card>();
        }

        newCard.InitializeCard(renderCard);
        return newCard;
    }
}
