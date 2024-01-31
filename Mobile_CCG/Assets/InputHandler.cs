using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [Header("General")]
    [SerializeField] private PlayerHub playerHub;
    [SerializeField] private Camera mainCamera;
    private GameContainerProxy gameContainerProxy;

    [Header("Card Slots")]
    public GameObject[] invaderCardslots;
    private GameObject[,] invaderField;

    [SerializeField] private GameObject[] wardenCardslots;
    private GameObject[,] wardenField;

    [SerializeField] private GameObject[] captureField;

    [Header("Card Interaction")]
    [SerializeField] private LayerMask cardMask;

    // Input
    private Vector2 touchPosition;

    private void Awake()
    {
        this.invaderField = Tools.ArrayTo2DArray(6, 2, ref this.invaderCardslots);
        this.wardenField = Tools.ArrayTo2DArray(6, 4, ref this.wardenCardslots);
    }

    private void Start()
    {
        this.gameContainerProxy = this.playerHub?.GameContainerProxy;
    }

    private void Update()
    {
        this.CardInteraction();
    }

    private void CardInteraction()
    {
        if (!this.isTouching)
        {
            return;
        }   

        if (Vector2.Distance(this.touchPosition, this.startPosition) > this.dragAndDropOffset && !isMoving)
        {
            print("Start DragAndDrop " + Vector2.Distance(this.touchPosition, this.startPosition));

            print(this.touchPosition);
            print(this.startPosition);
            this.isMoving = true;
            this.selectedCard = this.currentCard;
        }
    }

    [SerializeField] private float dragAndDropOffset;
    public bool isTouching;
    public bool isMoving;
    public Card currentCard;
    public Card selectedCard;
    private Vector2 startPosition;

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
            // implement drag and drop onrelease with currentCard
            print("OnRelease");
            this.isTouching = false;
            this.isMoving = false;
        }
    }

    public void ShortPress(InputAction.CallbackContext context)
    {
        if (context.performed && this.isTouching && !this.isMoving)
        {
            // When there is no selected, select the current card.
            if (this.selectedCard == null)
            {
                print($"select card {currentCard.name}");
                this.selectedCard = this.currentCard;
                this.isTouching = false;
                return;
            }

            // When the selected card is selected again, unselect it
            if (this.currentCard == this.selectedCard)
            {
                print($"unselect card {selectedCard.name}");
                this.selectedCard = null;
                this.isTouching = false;

                return;
            }

            print($"unselect card {selectedCard.name}");
            print($"select card {currentCard.name}");
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
            this.isTouching = false;
        }
    }

    public void TouchPosition(InputAction.CallbackContext context)
    {
        this.touchPosition = context.ReadValue<Vector2>();
    }
}
