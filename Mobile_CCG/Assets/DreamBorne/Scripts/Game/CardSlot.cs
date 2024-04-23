using UnityEngine;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private GameObject graphic;
    private Card card;
    private void Awake()
    {
        this.ToggleGraphic(false);
    }

    public void ToggleGraphic(bool isActive)
    {
        this.graphic.SetActive(isActive);
    }
}
