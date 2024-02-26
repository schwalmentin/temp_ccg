using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Guardian : Card
{
    [SerializeField] private int cost;

    [Header("Guards")]
    [SerializeField] private List<Guard> guards = new List<Guard>();

    [Header("Graphics")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private TextMeshProUGUI nameGUI;
    [SerializeField] private TextMeshProUGUI costGUI;
    [SerializeField] private GameObject selectHighlight;

    public List<Guard> Guards { get { return this.guards; } }

    public override void DisplayCardInfo()
    {
        Debug.Log ("\n" +
            $"Name: {this.name} \n" +
            $"Cost: {this.cost} \n" +
            $"Guards Amount: {this.guards.Count}");
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

    public override int GetCost()
    {
        return this.cost;
    }
}
