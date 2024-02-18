using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    #region Variables

    // General
    [Header("General")]
    [SerializeField] private PlayerHub playerHub;
    [SerializeField] private Camera mainCamera;
    [Space]
    [SerializeField] private LayerMask cardMask;
    [SerializeField] private LayerMask cardSlotMask;

    // Card Fields
    [Header("Card Slots")]
    [SerializeField] private CardSlot[] invaderCardslotsArray;
    private Dictionary<Vector2Int, CardSlot> invaderCardslots;

    [SerializeField] private CardSlot[] wardenCardslotsArray;
    private Dictionary<Vector2Int, CardSlot> wardenCardslots;

    [SerializeField] private CardSlot[] captureCardslotsArray;
    private Dictionary<Vector2Int, CardSlot> captureCardslots;

    // Highlights
    [Header("Field Highlights")]
    [SerializeField] private GameObject invaderFieldHighlights; 
    [SerializeField] private GameObject invaderField;
    [Space]
    [SerializeField] private GameObject wardenFieldHighlights;
    [SerializeField] private GameObject wardenField;
    [Space]
    [SerializeField] private GameObject captureFieldHighlights;
    [SerializeField] private GameObject captureField;

    // Card Interaction
    [Header("Card Interaction")]
    [SerializeField] private float dragSpeed;
    [SerializeField] private float dragAndDropOffset;

    // Handcards Rendering
    [Header("Handcard Rendering")]
    [SerializeField] private List<Card> handCardTest;
    [SerializeField] private Transform handTransform;
    [SerializeField] private float maxHandWidth;
    [SerializeField] private float cardRadius;

    // Card Placement
    [Header("Card Placement")]
    [SerializeField] private float yOffset;
    [SerializeField] private float xOffset;
    [SerializeField] private float detectionRadius;
    private List<CardSlot> currentCardSlots;

    // Input
    private Vector2 touchPosition;
    private Vector2 touchPositionWorldSpace;
    private Vector2 startPosition;

    private bool isTouching;
    private bool isMoving;

    [SerializeField] private Card currentCard;
    [SerializeField] private Card selectedCard;

    #endregion

    #region Unity Methods

    private void Awake()
    {
        this.invaderCardslots = this.InitializeCardSlots(new Vector2Int(6, 2), this.invaderCardslotsArray);
        this.wardenCardslots = this.InitializeCardSlots(new Vector2Int(6, 4), this.wardenCardslotsArray);
        this.captureCardslots = this.InitializeCardSlots(new Vector2Int(3, 1), this.captureCardslotsArray);

        this.currentCardSlots = new List<CardSlot>();

        // TEst
        foreach (var card in this.handCardTest)
        {
            card.InitializeCard(true);
            card.CardState = CardState.Hand;
        }
    }

    private void Update()
    {
        this.CardInteraction(this.currentCard, this.selectedCard);
        this.RenderHandCards(this.handCardTest);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(new Vector3(this.handTransform.position.x - this.maxHandWidth / 2, this.handTransform.position.y, this.handTransform.position.z), Vector2.right * this.maxHandWidth);
        Gizmos.DrawWireSphere(this.touchPositionWorldSpace, this.detectionRadius);
    }

    #endregion

    #region Input Handler Methods

    private void CardInteraction(Card currentCard, Card selectedCard)
    {
        if (!this.isTouching || currentCard.CardState != CardState.Hand)
        {
            return;
        }   

        if (Vector2.Distance(this.touchPosition, this.startPosition) > this.dragAndDropOffset && !isMoving)
        {
            this.isMoving = true;
            if (selectedCard != null)
            {
                selectedCard?.OnDeselect();
                this.EnableFields(selectedCard, false);
            }
            currentCard.OnSelect();
            this.EnableFields(currentCard, true);
            selectedCard = currentCard;
        }

        if (!this.isMoving)
        {
            return;
        }

        Ray ray = this.mainCamera.ScreenPointToRay(this.touchPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            this.touchPositionWorldSpace = hit.point;
        }

        currentCard.transform.position = Vector3.Lerp (
            new Vector3(this.touchPositionWorldSpace.x, this.touchPositionWorldSpace.y, currentCard.transform.position.z),
            currentCard.transform.position,
            Mathf.Pow(0.5f, this.dragSpeed * Time.deltaTime));

        this.currentCardSlots.ForEach(x => x.ToggleGraphic(false));
        this.currentCardSlots = this.DetectAvailableCardSlot(currentCard);
        this.currentCardSlots.ForEach(x => x.ToggleGraphic(true));
    }

    private void RenderHandCards(List<Card> hand)
    {
        // Ugly Code, please refactor asap
        float currentCardRadius = this.cardRadius * (hand.Count - 1) < this.maxHandWidth / 2 ?  this.cardRadius : this.maxHandWidth / 2 / (hand.Count);

        Vector3 firstPosition = this.handTransform.position;
        firstPosition.x -= currentCardRadius * (hand.Count - 1);

        foreach (Card card in hand)
        {
            if (card != this.currentCard)
            {
                card.transform.position = firstPosition;
            }

            firstPosition.x += currentCardRadius * 2;
        }
    }

    // Code seperation

    private void EnableFields(Card card, bool enable)
    {
        if (card == null) { return; }

        if (card.GetType() == typeof(Nightmare))
        {
            this.invaderFieldHighlights.SetActive(enable);
            this.invaderField.SetActive(enable);
            return;
        }

        if (card.GetType() == typeof(Guardian))
        {
            this.wardenFieldHighlights.SetActive(enable);
            this.wardenField.SetActive(enable);
            return;
        }

        if (card.GetType() == typeof(Trap) || card.GetType() == typeof(Dreamcatcher))
        {
            this.captureFieldHighlights.SetActive(enable);
            this.captureField.SetActive(enable);
            return;
        }
    }

    private List<CardSlot> DetectAvailableCardSlot(Card card)
    {
        // Create new card slot list
        List<CardSlot> availableCardslots = new List<CardSlot>();
        if (card == null) { return availableCardslots; }

        // Search information
        int cardSlotAmount = card.GetType() == typeof(Guardian) ? ((Guardian) card).Guards.Count : 1;
        Dictionary <Vector2Int, CardSlot> field = GetFieldByCard(card);
        float minDistance = float.MaxValue;

        // Search all card slots within the touch poistion range
        Collider[] cardSlots = Physics.OverlapSphere(
            new Vector2(this.touchPositionWorldSpace.x + this.xOffset * cardSlotAmount, this.touchPositionWorldSpace.y + this.yOffset),
            this.detectionRadius,
            this.cardSlotMask);

        // Search
        foreach (Collider cardSlotCollider in cardSlots)
        {
            CardSlot cardSlot = cardSlotCollider.GetComponent<CardSlot>();
            if (cardSlot == null) { continue; }

            Vector2Int placement = this.GetPlacementOfCardSlot(cardSlot, field);
            float distance = Vector3.Distance(this.touchPositionWorldSpace, cardSlotCollider.transform.position);

            if (distance < minDistance && placement.x + cardSlotAmount <= 6)
            {
                availableCardslots.Clear();
                minDistance = distance;
                for (int i = 0; i < cardSlotAmount; i++)
                {
                    availableCardslots.Add(field[new Vector2Int(placement.x + i, placement.y)]);
                }
            }
        }

        return availableCardslots;
    }

    private void PlaceCard(Card card)
    {
        // Check if a card is played and a valid cardslot is selected
        if (card == null || this.currentCardSlots.Count == 0) {  return; }

        // Get cards
        Dictionary<Vector2Int, CardSlot> field = GetFieldByCard(card);
        Card[] cards = card.GetType() == typeof(Guardian) ? ((Guardian)card).Guards.ToArray() : new Card[] { card };

        // Check if card amount matches with cardslot amount
        if (cards.Length != this.currentCardSlots.Count) { return; }

        // Place card on board
        card.gameObject.SetActive(false);

        for (int i = 0; i < cards.Length; i++)
        {
            Vector2Int placement = GetPlacementOfCardSlot(this.currentCardSlots[i], field);

            if (field[placement].Card != null)
            {
                field[placement].Card.gameObject.SetActive(false);
            }

            field[placement].Card = cards[i];
            cards[i].transform.position = this.currentCardSlots[i].transform.position;
            cards[i].CardState = CardState.Field;
            cards[i].gameObject.SetActive(true);
            cards[i].InitializeCard(true);

            Debug.Log($"The card {cards[i]} was placed at the cardslot {this.currentCardSlots[i]}");
        }

        // Invoke Player Hub

        // Update the board and hand
        this.selectedCard = null;
        this.EnableFields(card, false);

        card.CardState = CardState.Field;
        card.OnDeselect();
        this.handCardTest.Remove(card);
    }

    // Utility classes

    private Dictionary<Vector2Int, CardSlot> GetFieldByCard(Card card)
    {
        return card.GetType() == typeof(Nightmare) ? this.invaderCardslots :
            card.GetType() == typeof(Guardian) ? this.wardenCardslots : this.captureCardslots;
    }

    private Vector2Int GetPlacementOfCardSlot(CardSlot cardSlot, Dictionary<Vector2Int, CardSlot> field)
    {
        return field.FirstOrDefault(x => x.Value == cardSlot).Key;
    }

    private Dictionary<Vector2Int, CardSlot> InitializeCardSlots(Vector2Int fieldSize, CardSlot[] field)
    {
        if (fieldSize.x * fieldSize.y != field.Length)
        {
            Debug.LogError("The dimensions of the field don't match with the provided array of card slots!");
            return null;
        }

        Dictionary<Vector2Int, CardSlot> cardSlots = new Dictionary<Vector2Int, CardSlot>();

        for (int i = 0; i < field.Length; i++)
        {
            int x = i % fieldSize.x;
            int y = Mathf.FloorToInt(i / fieldSize.x);
            cardSlots.Add(new Vector2Int(x, y), field[i]);
        }

        return cardSlots;
    }


    #endregion

    #region Player Input Methods

    public void HoldPress(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Reset
            this.currentCard = null;
            this.isTouching = false;
            this.isMoving = false;

            // Detect card and set it to current card
            Ray ray = this.mainCamera.ScreenPointToRay(this.touchPosition);
            RaycastHit hit;

            if (!Physics.Raycast(ray, out hit, 100, this.cardMask))
            {
                if (this.selectedCard != null)
                {
                    this.isMoving = true;
                    this.isTouching = true;
                    this.currentCard = this.selectedCard;
                }
                return;
            }
            
            this.currentCard = hit.collider.GetComponent<Card>();
            this.isTouching = this.currentCard != null;
            this.startPosition = this.touchPosition;
        }

        if (context.performed && this.isTouching)
        {
            this.PlaceCard(this.currentCard);

            this.isTouching = false;
            this.isMoving = false;
            this.currentCard = null;
        }
    }

    public void ShortPress(InputAction.CallbackContext context)
    {
        if (context.performed && this.isTouching && !this.isMoving && this.currentCard.CardState == CardState.Hand)
        {
            // When there is no selected, select the current card.
            if (this.selectedCard == null)
            {
                print($"select card {currentCard.name}");
                this.currentCard.OnSelect();
                this.EnableFields(this.currentCard, true);
                this.selectedCard = this.currentCard;
                this.isTouching = false;
                return;
            }

            // When the selected card is selected again, unselect it
            if (this.currentCard == this.selectedCard)
            {
                print($"unselect card {selectedCard.name}");
                this.selectedCard.OnDeselect();
                this.EnableFields(this.selectedCard, false);
                this.selectedCard = null;
                this.isTouching = false;
                this.currentCard= null;

                return;
            }

            print($"unselect card {selectedCard.name}");
            this.selectedCard.OnDeselect();
            this.EnableFields(this.selectedCard, false);
            print($"select card {currentCard.name}");
            this.currentCard.OnSelect();
            this.EnableFields(this.currentCard, true);
            this.selectedCard = this.currentCard;
            this.isTouching = false;
        }
    }

    public void LongPress(InputAction.CallbackContext context)
    {
        if (context.performed && this.isTouching && !this.isMoving)
        {
            // implement info pop up
            print("LONG PRESS performed");
            this.currentCard.DisplayCardInfo();
            this.isTouching = false;
        }
    }

    public void TouchPosition(InputAction.CallbackContext context)
    {
        this.touchPosition = context.ReadValue<Vector2>();
    }

    #endregion
}
