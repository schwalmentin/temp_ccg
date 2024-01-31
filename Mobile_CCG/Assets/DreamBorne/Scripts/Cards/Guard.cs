using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Guard : MonoBehaviour
{
    [Header("Guard Attributes")]
    [SerializeField] private int maxPower;
    private int currentPower;
    private Guardian guardian;

    [Header("Effects")]
    [SerializeField, SerializeReference] private List<Action> effects;

    [Header("Graphics")]
    [SerializeField] private GameObject graphics;
    [SerializeField] private TextMeshProUGUI powerGUI;

    public void InitializeGuard(Guardian guardian, bool includeGraphics)
    {
        this.guardian = guardian;

        this.graphics.SetActive(includeGraphics);
        this.powerGUI.text = this.maxPower.ToString();
    }
}
