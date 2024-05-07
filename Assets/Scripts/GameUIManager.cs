using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance = null;

    public Text unitNameText;
    public Text hpText;
    public Text apText;
    public Text mpText;

    public TextMeshProUGUI[] abilityText;
    public Button endTurnButton;
    public Image charCard;
    private bool isEnabled;

    // Called when the object is initialized
    void Awake()
    {
        // if it doesn't exist
        if (instance == null)
        {
            // Set the instance to the current object (this)
            instance = this;
        }
        // There can only be a single instance of the UI manager
        else if (instance != this)
        {
            // Destroy the current object, so there is just one manager
            Destroy(gameObject);
        }

        isEnabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        UnitController selectedUnit = GameManager.instance.GetSelectedUnit();
        if (selectedUnit != null && selectedUnit.belongsToPlayer)
        {
            unitNameText.text = selectedUnit.unitName;
            hpText.text = "HP\n" + selectedUnit.currentHealth + "/" + selectedUnit.maxHealth;
            apText.text = "AP\n" + selectedUnit.actionPoints.ToString();
            mpText.text = "MP\n" + selectedUnit.currMP + "/" + selectedUnit.maxMP;
            abilityText[0].text = selectedUnit.abilities[0].abilityName + "\nMP: " + selectedUnit.abilities[0].manaCost + " | Range: " + selectedUnit.abilities[0].range;
            abilityText[1].text = selectedUnit.abilities[1].abilityName + "\nMP: " + selectedUnit.abilities[1].manaCost + " | Range: " + selectedUnit.abilities[1].range;
            abilityText[2].text = selectedUnit.abilities[2].abilityName + "\nMP: " + selectedUnit.abilities[2].manaCost + " | Range: " + selectedUnit.abilities[2].range;
        }
        else
        {
            unitNameText.text = "";
            hpText.text = "HP";
            apText.text = "AP";
            mpText.text = "MP";
            abilityText[0].text = "Attack";
            abilityText[1].text = "Spell 1";
            abilityText[2].text = "Spell 2";
        }
    }

    // Called when the player clicks the "End Turn" button
    public void EndPlayerTurn()
    {
        if (GameManager.instance.IsPlayerTurn())
        {
            GameManager.instance.ChangeTurn();
        }
    }

    // Enable/Disable the UI (used when switching turns)
    public void SetEnabled(bool enabled)
    {
        isEnabled = enabled;

        endTurnButton.gameObject.SetActive(isEnabled);
        unitNameText.gameObject.SetActive(isEnabled);
        hpText.gameObject.SetActive(isEnabled);
        apText.gameObject.SetActive(isEnabled);
        mpText.gameObject.SetActive(isEnabled);
    }

    // Sends which action is selected to UnitController of selected unit
    public void SelectActions(int actionSelection)
    {
        if (GameManager.instance.GetSelectedUnit() != null)
        {
            GameManager.instance.GetSelectedUnit().SelectActions(actionSelection);
            print("Action " + actionSelection + " has been selected");
        }
    }
}
