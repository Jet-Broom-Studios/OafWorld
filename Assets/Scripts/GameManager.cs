using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Static instance of the Game Manager,
    // can be access from anywhere
    public static GameManager instance = null;

    // The player unit currently selected (if any)
    private UnitController selectedUnit;
    // Whether it is currently the player's turn or not
    private bool isPlayerTurn;

    // Called when the object is initialized
    void Awake()
    {
        // if it doesn't exist
        if (instance == null)
        {
            // Set the instance to the current object (this)
            instance = this;
        }
        // There can only be a single instance of the game manager
        else if (instance != this)
        {
            // Destroy the current object, so there is just one manager
            Destroy(gameObject);
        }

        // Don't destroy this object when loading scenes
        DontDestroyOnLoad(gameObject);

        selectedUnit = null;
        isPlayerTurn = true;
    }

    public void SetSelectedUnit(UnitController unit)
    {
        selectedUnit = unit;
        if (unit != null && unit.actionPoints > 0)
        {
            print("Unit selected!");
            int currentNode = unit.GetCurrentNode();
            SetTargets(currentNode, unit.attackRange, unit.belongsToPlayer);
            MapManager.instance.GeneratePathsFromNode(currentNode, unit.movePower, unit.belongsToPlayer);
        }

        if (unit == null || unit.actionPoints <= 0)
        {
            MapManager.instance.ResetNodePaths();
            ResetTargets();
        }
    }

    public UnitController GetSelectedUnit()
    {
        return selectedUnit;
    }

    // Swap between the player's and enemy's turn
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            print("It's the player's turn!");
        }
        else
        {
            print("It's the enemy's turn!");
        }

        // Deselect any selected unit
        SetSelectedUnit(null);
        // Disable the UI if it's not the player's turn
        GameUIManager.instance.SetEnabled(isPlayerTurn);

        // Replenish action points of the side who gets to do stuff
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        for (int i = 0; i < unitList.Length; i++)
        {
            if (unitList[i].belongsToPlayer == isPlayerTurn)
            {
                unitList[i].actionPoints = 2;
            }
        }
    }

    // Returns true if it is currently the player's turn
    // The player can only control their units if it is their turn
    // Enemy units can only perform actions when it is not the player's turn
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }

    // Set which units can be targeted from the given position
    internal void SetTargets(int sourceNode, int attackRange, bool byPlayer)
    {
        //print("Setting targets...");
        ResetTargets();
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        //print("Found " + unitList.Length + " potential targets");
        for (int i = 0; i < unitList.Length; i++)
        {
            // TODO: Allow targetting friendly units for beneficial spells
            UnitController unit = unitList[i];
            int targetNode = unit.GetCurrentNode();
            // Mark any unit as targetable if both:
            // They are on opposite teams from the source
            // They are within range
            if (unit.belongsToPlayer != byPlayer && MapManager.instance.GetDistBetweenNodes(sourceNode, targetNode) <= attackRange)
            {
                unit.SetTargetable(true);
                //print("Marked a target!");
            }
        }
    }

    internal void ResetTargets()
    {
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        for (int i = 0; i < unitList.Length; i++)
        {
            unitList[i].SetTargetable(false);
        }
    }

    // Check if no more actions can be taken on this turn.
    // If nothing more can be done, automatically switch turns.
    internal void CheckTurnDone()
    {
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        for (int i = 0; i < unitList.Length; i++)
        {
            if (unitList[i].belongsToPlayer == isPlayerTurn && unitList[i].actionPoints > 0)
            {
                return; // More actions can be taken
            }
        }

        // If we reach here, all action points have been expended.
        // Nothing more can be done on this turn.
        ChangeTurn();
    }
}
