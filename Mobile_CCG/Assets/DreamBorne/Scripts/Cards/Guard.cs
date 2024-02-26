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
    [SerializeField] private GameObject highlight;
    [SerializeField] private TextMeshProUGUI nameGUI;
    [SerializeField] private TextMeshProUGUI powerGUI;

    public int Power
    {
        get { return currentPower; }
    }

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
        this.currentPower = this.maxPower;
    }

    public override void OnDeselect()
    {
        this.highlight.SetActive(false);
    }

    public override void OnSelect()
    {
        this.highlight.SetActive(true);
    }

    public override void GetDamage(int amount)
    {
        this.currentPower -= amount;
    }

    public override void UpdateUI()
    {
        this.nameGUI.text = this.cardName;
        this.powerGUI.text = this.maxPower.ToString();
    }
}
