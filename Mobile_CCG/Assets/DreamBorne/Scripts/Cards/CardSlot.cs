using UnityEngine;

public class CardSlot : MonoBehaviour
{
    [SerializeField] private Material selectedMaterial;
    [SerializeField] private Material availableMaterial;
    private MeshRenderer meshRenderer;
    private Card card;

    private void Awake()
    {
        this.meshRenderer = GetComponentInChildren<MeshRenderer>();
        this.meshRenderer.gameObject.SetActive(false);
    }

    public void ToggleGraphic(bool enable)
    {
        this.meshRenderer.gameObject.SetActive(enable);
    }

    public Card Card
    {
        get { return this.card; }
        set
        {
            this.meshRenderer.material = value == null ? availableMaterial : selectedMaterial;
            this.card = value;
        }
    }
}
