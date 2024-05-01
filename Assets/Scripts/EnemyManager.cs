using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance = null;

    // List of enemies that can still perform actions
    private List<UnitController> enemyList;
    // Whether we have counted all the enemies this turn
    private bool enemiesCounted;

    // Called when the object is initialized
    void Awake()
    {
        // if it doesn't exist
        if (instance == null)
        {
            // Set the instance to the current object (this)
            instance = this;
        }
        // There can only be a single instance of the enemy manager
        else if (instance != this)
        {
            // Destroy the current object, so there is just one manager
            Destroy(gameObject);
        }
        enemyList = new List<UnitController>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameManager.instance.IsPlayerTurn())
        {
            if (!enemiesCounted)
            {
                // Get every enemy on the map
                UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
                for (int i = 0; i < unitList.Length; i++)
                {
                    if (!unitList[i].belongsToPlayer)
                    {
                        enemyList.Add(unitList[i]);
                    }
                }
                enemiesCounted = true;
            }
            else if (enemyList.Count > 0)
            {
                // TODO: The order that enemies act is arbitrary right now.
                // Do we want to enforce some sort of order for which units act first?

                // Grab an enemy unit
                UnitController enemy = enemyList[0];
                if (enemy.actionPoints > 0)
                {
                    GameManager.instance.SetSelectedUnit(enemy);

                    // TODO: Movement logic

                    // If there's a player unit within range, attempt to attack it
                    // TODO: Should probably prioritise attacking the target that we're most likely to hit.
                    // Right now we just shoot the first thing that appears in the list
                    UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
                    bool foundTarget = false;
                    for (int i = 0; i < unitList.Length; i++)
                    {
                        if (unitList[i].belongsToPlayer && unitList[i].IsTargetable())
                        {
                            enemy.OrderAttack(unitList[i]);
                            foundTarget = true;
                            break;
                        }
                    }

                    if (!foundTarget)
                    {
                        // Nothing in range, move on to the next enemy
                        enemyList.RemoveAt(0);
                    }
                }
                else
                {
                    // Enemy can't do anything else
                    enemyList.RemoveAt(0);
                }
            }
            else
            {
                // No more actions to be performed
                // Switch back to the player's turn
                GameManager.instance.ChangeTurn();
            }
        }
        else
        {
            enemiesCounted = false;
        }
    }
}
