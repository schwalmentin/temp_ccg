using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    #region Variables

        // Model References
        private PlayerEngine playerEngine;
        private PlayerDataProxy playerDataProxy;

        // Touch
        private TouchState touchState;
        private Vector3 touchPosition;
        private Vector3 touchPositionStarted;
        private Vector3 touchPositionWorldSpace;

        // Card Selection
        private Card currentCard;
        private Card selectedCard;
        private CardSlot currentCardSlot;
        
        // Input
        [Header("Input")]
        [SerializeField] private float longPressTime;
        private float longPressTimer;
        
        // Drag and Drop
        [Header("Drag and Drop")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Transform hoverTransform;
        [SerializeField] private LayerMask draggingMask;
        [Space]
        [SerializeField] private float maxDragOffset;

        // Card Slot Detection
        [Header("Card Slot Detection")]
        [SerializeField] private float detectionRadius;
        [SerializeField] private float detectionOffset;
        [SerializeField] private LayerMask cardSlotMask;
        
    #endregion

    #region Unity Methods

        private void Awake()
        {
            this.playerEngine = FindObjectOfType<PlayerEngine>();
            this.playerDataProxy = FindObjectOfType<PlayerDataProxy>();
            
            this.touchState = TouchState.NotTouching;
        }

        private void Update()
        {
            Physics.Raycast(this.mainCamera.ScreenPointToRay(this.touchPosition), out RaycastHit hit, 100, this.draggingMask);
            this.touchPositionWorldSpace = new Vector3(hit.point.x, hit.point.y, this.hoverTransform.position.z);
            
            this.PressingState();
            this.DraggingState();
        }

    #endregion

    #region InputHandler Methods
    
        /// <summary>
        /// When first touching the screen, it shoots a ray to search a card to use for the input.
        /// </summary>
        /// <returns>Next TouchState</returns>
        private TouchState OnInputDown()
        {
            // Update starting touch position
            this.touchPositionStarted = this.touchPosition;
            
            // Shoot ray to hit a card
            Physics.Raycast(this.mainCamera.ScreenPointToRay(this.touchPosition), out RaycastHit hit, 100);
            if (hit.collider == null) return TouchState.NotTouching;
            
            // Check if a card was hit
            Card hitCard = hit.collider.GetComponent<Card>();
            if (hitCard != null)
            {
                this.currentCard = hitCard;
                return TouchState.Pressing;
            }
                    
            // Check if an old card is selected
            if (this.selectedCard == null) return TouchState.NotTouching;

            this.currentCard = this.selectedCard;
            return TouchState.Dragging;
        }

        /// <summary>
        /// Invokes the Short Press and Dragging actions and completes the touch input cycle.
        /// </summary>
        /// <returns></returns>
        private TouchState OnInputUp()
        {
            switch (this.touchState)
            {
                // -----> |SHORT PRESS| INVOCATION HAPPENS HERE <-----
                case TouchState.Pressing:
                    if (this.selectedCard == null)
                    {
                        this.SetSelectedCard(this.currentCard);
                        break;
                    }

                    if (this.selectedCard == this.currentCard)
                    {
                        this.SetSelectedCard(null);
                        this.currentCard = null;
                        break;
                    }

                    this.SetSelectedCard(this.currentCard);
                    break;
                
                // -----> |DRAG AND DROP| INVOCATION HAPPENS HERE <-----
                case TouchState.Dragging:
                    if (this.currentCardSlot == null || this.currentCard.Cost > this.playerDataProxy.Mana)
                    {
                        this.SetSelectedCard(this.currentCard);
                        this.playerEngine.ArrangeHand(null);
                        break;
                    }

                    this.playerEngine.PlayCard(new PlayCardParams(this.currentCard.UniqueId, this.currentCardSlot.FieldPosition));
                    this.playerEngine.ArrangeHand(null);
                    this.SetSelectedCard(null);
                    break;
            }

            this.currentCardSlot?.ToggleGraphic(false);
            this.currentCardSlot = null;
            this.currentCard = null;
            return TouchState.NotTouching;
        }

        /// <summary>
        /// Checks for a long press and drag and drop during the touch state PressDown.
        /// </summary>
        private void PressingState()
        {
            if (this.touchState != TouchState.Pressing) return;
            
            // Check for long press
            this.longPressTimer += Time.deltaTime;

            if (this.longPressTimer >= this.longPressTime)
            {
                this.touchState = TouchState.LongPress;
                this.longPressTimer = 0;
                
                // -----> |LONG PRESS| INVOCATION HAPPENS HERE <-----
                this.currentCard?.ShowInfo();
                return;
            }
                
            // Check for drag and drop
            if (Vector2.Distance(this.touchPositionStarted, this.touchPosition) > this.maxDragOffset)
            {
                if (this.currentCard.CardState != CardState.Hand)
                {
                    this.touchState = TouchState.NotTouching;
                    return;
                }
                
                this.SetSelectedCard(this.currentCard);
                this.touchState = TouchState.Dragging;
                this.playerEngine.ArrangeHand(new List<Card>() { this.currentCard });
            }
        }

        /// <summary>
        /// Manages dragging and dropping during the touch state Dragging.
        /// </summary>
        private void DraggingState()
        {
            // Dragging Input State
            if (this.touchState != TouchState.Dragging) return;

            this.currentCard.transform.localPosition = this.touchPositionWorldSpace;
            this.DetectNearestCardSlot();
        }

        /// <summary>
        /// Searches for all card slots in a given radius and returns the closest one that is empty.
        /// </summary>
        /// <returns>The nearest CardSlot. If no CardSlot is detected it returns null.</returns>
        private void DetectNearestCardSlot()
        {
            // Deselect current card slot
            this.currentCardSlot?.ToggleGraphic(false);
            
            // Search for nearest card slot
            Collider[] cardSlots = Physics.OverlapSphere(
                new Vector3(this.touchPositionWorldSpace.x, this.touchPositionWorldSpace.y + this.detectionOffset, this.touchPositionWorldSpace.z),
                this.detectionRadius, 
                this.cardSlotMask);

            CardSlot nearestCardSlot = null;
            float nearestCardSlotRange = float.MaxValue;
            
            foreach (Collider cardSlotCollider in cardSlots)
            {
                CardSlot cardSlot = cardSlotCollider.GetComponent<CardSlot>();
                
                if (cardSlot == null) continue;
                if (cardSlot.Card != null) continue;

                float range = Vector2.Distance(this.touchPositionWorldSpace, cardSlot.transform.position);
                
                if (range > nearestCardSlotRange) continue;

                nearestCardSlot = cardSlot;
                nearestCardSlotRange = range;
            }
            
            // Select nearest card slot
            this.currentCardSlot = nearestCardSlot;
            this.currentCardSlot?.ToggleGraphic(true);
        }

        /// <summary>
        /// Sets the value off the selected card and additionally adjusts the highlight graphic.
        /// </summary>
        /// <param name="card"></param>
        private void SetSelectedCard(Card card)
        {
            if (card != null)
            {
                if (card.CardState != CardState.Hand) return;
            }
            
            this.selectedCard?.ToggleHighlight(false);
            this.selectedCard = card;
            this.selectedCard?.ToggleHighlight(true);
        }

    #endregion

    #region Input Methods

        /// <summary>
        /// Is invoked via the UI and passes the turn.
        /// </summary>
        public void PassTurn()
        {
            this.playerEngine.PassTurn();
        }

        /// <summary>
        /// Is invoked via UI and undoes the last played card.
        /// </summary>
        public void Undo()
        {
            this.playerEngine.UndoCard();
        }

        /// <summary>
        /// Is invoked via the new input system, when a touch down/up action is performed.
        /// </summary>
        /// <param name="context"></param>
        public void TouchInput(InputAction.CallbackContext context)
        {
            // On Input Down
            if (context.started)
            {
                this.touchState = this.OnInputDown();
            }

            // On Input Up
            if (context.canceled)
            {
                this.touchState = this.OnInputUp();
            }
        }
        
        /// <summary>
        /// Is invoked via the new input system and updates the touch position.
        /// </summary>
        /// <param name="context"></param>
        public void TouchPosition(InputAction.CallbackContext context)
        {
            this.touchPosition = context.ReadValue<Vector2>();
        }

    #endregion
}