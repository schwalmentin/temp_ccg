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

    public List<Guard> Guards { get { return this.guards; } }

    public override void InitializeCard(bool includeGraphic)
    {
        this.graphics.SetActive(includeGraphic);

        foreach (Guard guard in this.guards)
        {
            guard.InitializeGuard(this, includeGraphic);
        }
    }
}
