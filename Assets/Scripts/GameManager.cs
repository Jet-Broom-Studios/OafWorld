using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Static instance of the Game Manager,
    // can be access from anywhere
    public static GameManager instance = null;

    // The player unit currently selected (if any)
    private PlayerUnitController selectedUnit;

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

    public void SetSelectedUnit(PlayerUnitController unit)
    {
        selectedUnit = unit;
        print("Unit selected!");
    }

    public PlayerUnitController GetSelectedUnit()
    {
        return selectedUnit;
    }
}
