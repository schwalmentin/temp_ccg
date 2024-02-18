using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Guard : Card
{
    [Header("Guard Attributes")]
    [SerializeField] private int maxPower;
    private int currentPower;

    [Header("Effects")]
    [SerializeField, SerializeReference] private List<Action> effects;

    [Header("Graphics")]
    [SerializeField] private GameObject graphic;
    [SerializeField] private TextMeshProUGUI nameGUI;
    [SerializeField] private TextMeshProUGUI powerGUI;

    public override void DisplayCardInfo()
    {
        Debug.Log("\n" +
            $"Name: {this.cardName} \n" +
            $"Power: {this.maxPower}");
    }

    public override void InitializeCard(bool includeGraphic)
    {
        this.graphic.SetActive(includeGraphic);
        this.nameGUI.text = this.cardName;
        this.powerGUI.text = this.maxPower.ToString();
    }

    public override void OnDeselect()
    {
    }

    public override void OnSelect()
    {
    }
}
