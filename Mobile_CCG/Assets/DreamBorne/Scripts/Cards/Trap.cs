using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Trap : Card
{
    [SerializeField] private int cost;

    [Header("Effects")]
    [SerializeField, SerializeReference] private List<Action> effects;

    [Header("Graphics")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private TextMeshProUGUI nameGUI;
    [SerializeField] private TextMeshProUGUI costGUI;
    [SerializeField] private GameObject selectHighlight;

    public override void DisplayCardInfo()
    {
        Debug.Log("\n" +
            $"Name: {this.name} \n" +
            $"Cost: {this.cost} \n");
    }

    public override void InitializeCard(bool includeGraphic)
    {
        this.graphics.SetActive(includeGraphic);
        this.nameGUI.text = this.cardName;
        this.costGUI.text = this.cost.ToString();
    }

    public override void OnDeselect()
    {
        this.selectHighlight.SetActive(false);
    }

    public override void OnSelect()
    {
        this.selectHighlight.SetActive(true);
    }
}
