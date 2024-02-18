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
    [SerializeField] private GameObject selectHighlight;

    public override void DisplayCardInfo()
    {
        Debug.Log ("\n" +
            $"Name: {this.name} \n" + 
            $"Base Points: {this.basePoints}");
    }

    public override void InitializeCard(bool includeGraphic)
    {
        this.graphics.SetActive(includeGraphic);
        this.nameGUI.text = this.cardName;
        this.basePointsGUI.text = this.basePoints.ToString();
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
