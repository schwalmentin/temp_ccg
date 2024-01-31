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

    public override void InitializeCard(bool includeGraphics)
    {
        this.currentPower = this.maxPower;

        this.graphics.SetActive(includeGraphics);
        this.nameGUI.text = this.cardName;
        this.costGUI.text = this.cost.ToString();
        this.powerGUI.text = this.currentPower.ToString();
    }
}
