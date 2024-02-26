using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Nightmare : Card
{
    [SerializeField] private int cost;
    [SerializeField] private int maxPower;
    private int currentPower;

    [Header("Abilities")]
    [SerializeField] private List<Action> onRevealActions;
    [SerializeField, SerializeReference] private List<Action> onGoingActions;
    [SerializeField, SerializeReference] private List<Action> onDieActions;

    [Header("Graphics")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private TextMeshProUGUI nameGUI;
    [SerializeField] private TextMeshProUGUI costGUI;
    [SerializeField] private TextMeshProUGUI powerGUI;
    [SerializeField] private GameObject selectHighlight;

    public int Power
    {
        get { return this.currentPower;  }
    }

    public override void DisplayCardInfo()
    {
        Debug.Log("\n" +
            $"Name: {this.name} \n" +
            $"Cost: {this.cost} \n" +
            $"Power: {this.maxPower}");
    }


    public override void InitializeCard(bool includeGraphics)
    {
        this.currentPower = this.maxPower;

        this.graphics.SetActive(includeGraphics);
        this.nameGUI.text = this.cardName;
        this.costGUI.text = this.cost.ToString();
        this.powerGUI.text = this.currentPower.ToString();
    }

    public override void OnDeselect()
    {
        this.selectHighlight.SetActive(false);
    }

    public override void OnSelect()
    {
        this.selectHighlight.SetActive(true);
    }

    public override void GetDamage(int amount)
    {
        this.currentPower -= amount;
    }

    public override void UpdateUI()
    {
        this.nameGUI.text = this.cardName;
        this.costGUI.text = this.cost.ToString();
        this.powerGUI.text = this.currentPower.ToString();
    }

    public override int GetCost()
    {
        return this.cost;
    }
}
