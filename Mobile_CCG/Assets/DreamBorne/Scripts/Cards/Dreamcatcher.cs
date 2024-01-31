using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Dreamcatcher : Card
{
    [SerializeField] private int basePoints;

    [Header("Effects")]
    [SerializeField, SerializeReference] private List<Action> effects;

    [Header("Graphics")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private TextMeshProUGUI nameGUI;
    [SerializeField] private TextMeshProUGUI basePointsGUI;

    public override void InitializeCard(bool includeGraphic)
    {
        this.graphics.SetActive(includeGraphic);
        this.nameGUI.text = this.cardName;
        this.basePointsGUI.text = this.basePoints.ToString();
    }
}
