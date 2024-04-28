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
    }

    public void SetSelectedUnit(UnitController unit)
    {
        selectedUnit = unit;
        if (unit != null && unit.belongsToPlayer)
        {
            int currentNode = unit.GetCurrentNode();
            SetTargets(currentNode, unit.attackRange, true);
            MapManager.instance.GeneratePathsFromNode(currentNode, unit.movePower, true);
        }
        if (unit != null) print("Unit selected!");
        if (unit == null)
        {
            print("Unit deselected!");
            MapManager.instance.ResetNodePaths();
            ResetTargets();
        }
    }

    public UnitController GetSelectedUnit()
    {
        return selectedUnit;
    }

    // Set which units can be targeted from the given position
    internal void SetTargets(int sourceNode, int attackRange, bool byPlayer)
    {
        print("Setting targets...");
        ResetTargets();
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        print("Found " + unitList.Length + " units");
        for (int i = 0; i < unitList.Length; i++)
        {
            // TODO: Allow targetting friendly units for beneficial spells
            UnitController unit = unitList[i];
            int targetNode = unit.GetCurrentNode();
            // Mark any unit as targetable if both:
            // They are on opposite teams from the source
            // They are within range
            if (unit.belongsToPlayer != byPlayer && MapManager.instance.getDistBetweenNodes(sourceNode, targetNode) <= attackRange)
            {
                unit.SetTargetable(true);
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
}
