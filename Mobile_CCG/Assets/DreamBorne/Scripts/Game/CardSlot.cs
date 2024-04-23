using UnityEngine;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private GameObject graphic;

    public Card Card { get; set; }
    public Vector2Int FieldPosition { get; set; }

    private void Awake()
    {
        this.ToggleGraphic(false);
    }

    public void ToggleGraphic(bool isActive)
    {
        this.graphic.SetActive(isActive);
    }
}
