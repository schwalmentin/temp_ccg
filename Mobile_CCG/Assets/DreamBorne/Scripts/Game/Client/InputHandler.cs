using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    #region Variables

        private PlayerEngine playerEngine;

        private TouchState touchState;
        private Vector3 touchPosition;
        private Vector3 touchPositionStarted;
        private Vector3 touchPositionWorldSpace;

        private Card currentCard;
        private Card selectedCard;
        private CardSlot currentCardSlot;

        [Header("")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float maxDragOffset;
        [SerializeField] private Transform hoverTransform;
        
        [SerializeField] private float longPressTime;
        private float longPressTimer;
        
    #endregion

    #region Unity Methods

        private void Awake()
        {
            this.playerEngine = FindObjectOfType<PlayerEngine>();
            this.touchState = TouchState.NotTouching;
        }

        private void Update()
        {
            // On Input
            Physics.Raycast(this.mainCamera.ScreenPointToRay(this.touchPosition), out RaycastHit hit, 100);
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
                    print("Dragging");
                    this.SetSelectedCard(this.currentCard);
                    this.playerEngine.ArrangeHand(null);
                    break;
            }

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
        }

        /// <summary>
        /// Searches for all card slots in a given radius and returns the closest one that is empty.
        /// </summary>
        private CardSlot DetectCardSlots()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sets the value off the selected card and additionally adjusts the highlight graphic.
        /// </summary>
        /// <param name="card"></param>
        private void SetSelectedCard(Card card)
        {
            this.selectedCard?.SelectCard(false);
            this.selectedCard = card;
            this.selectedCard?.SelectCard(true);
        }

    #endregion

    #region Input Methods

        public void PassTurn()
        {
            
        }

        public void Undo()
        {

        }

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
        
        public void TouchPosition(InputAction.CallbackContext context)
        {
            this.touchPosition = context.ReadValue<Vector2>();
        }

    #endregion
}