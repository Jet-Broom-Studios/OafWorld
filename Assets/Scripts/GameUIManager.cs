using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance = null;

    public Text unitNameText;
    public Text unitStatText;
    public Button endTurnButton;
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
        if (selectedUnit != null)
        {
            unitNameText.text = selectedUnit.unitName;
            unitStatText.text = "Health: " + selectedUnit.currentHealth + "/" + selectedUnit.maxHealth
                + "    Action Points: " + selectedUnit.actionPoints;
        }
        else
        {
            unitNameText.text = "";
            unitStatText.text = "";
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
        unitStatText.gameObject.SetActive(isEnabled);
    }
}
