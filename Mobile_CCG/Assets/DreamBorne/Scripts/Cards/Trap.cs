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

    public override void InitializeCard(bool includeGraphic)
    {
        this.graphics.SetActive(includeGraphic);
        this.nameGUI.text = this.cardName;
        this.costGUI.text = this.cost.ToString();
    }
}
