using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance = null;
    // List of enemies that can still perform actions
    private List<UnitController> enemyList;
    private List<UnitController> remainingEnemies;
    public static int enemyCount;
    // Enemy we are currently ordering around
    UnitController currentEnemy;
    // Whether we have counted all the enemies this turn
    private bool enemiesCounted;

    // Logic variables..
    bool canMove;
    bool canAttack;

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
        remainingEnemies = new List<UnitController>();
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        for (int i = 0; i < unitList.Length; i++)
        {
            if (!unitList[i].belongsToPlayer)
            {
                remainingEnemies.Add(unitList[i]);
            }
        }
        enemyCount = remainingEnemies.Count;
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (enemyCount <= 0)
        {
            GameEnd.gameWin = true;
            if (DialogueSelectManager.currLevel == "Level1")
            {
                GameManager.l1Complete = true;
            }
            else if (DialogueSelectManager.currLevel == "Level2")
            {
                GameManager.l2Complete = true;
            }
            else if (DialogueSelectManager.currLevel == "Level3")
            {
                GameManager.l3Complete = true;
            }
            DelaySceneChange(5);
        }
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
                if (currentEnemy == null)
                {
                    // TODO: The order that enemies act is arbitrary right now.
                    // Do we want to enforce some sort of order for which units act first?

                    // Grab an enemy unit
                    currentEnemy = enemyList[0];
                    ResetUnitVars();
                }

                if (currentEnemy.actionPoints > 0 && currentEnemy.IsIdle())
                {
                    GameManager.instance.SetSelectedUnit(currentEnemy);

                    // TODO: Different behaviors for different enemies?

                    BasicBehavior();
                }
                else if (currentEnemy.IsIdle())
                {
                    // Enemy unit can't do anything else
                    currentEnemy = null;
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

    // Basic enemy unit behavior
    // NOTE: These units will only move and attack once per turn, even if they technically can multiple times
    // This is because these robots are "stupid"
    private void BasicBehavior()
    {
        if (!currentEnemy.IsIdle())
        {
            return; // Wait for the unit to be idle before telling it to do stuff
        }
        
        print("Running basic enemy behavior!");
        if (!HasTarget() && canMove)
        {
            print("Nothing in range; moving!");
            // Nothing is in range!
            // Find somewhere closer to a target
            MoveToBestSpot();
            canMove = false;
            return;
        }
        
        if (canAttack)
        {
            // If there's a player unit within range, attempt to attack 
            if (AttackBestTarget())
            {
                print("Attacking!");
                // Found something to shoot at; stop here
                canAttack = false;
                return;
            }
        }

        // If we reached this point we either can't attack or can't find anything to attack

        // Movement logic
        if (MapManager.instance.isExposed(currentEnemy.GetCurrentNode(), false) && canMove)
        {
            print("Exposed; moving!");
            // This unit is exposed!
            // Try to find somewhere to take cover
            MoveToBestSpot();
            canMove = false;
            return;
        }

        // If we made it to here, there's nothing else this unit should do
        currentEnemy = null;
        enemyList.RemoveAt(0);
    }

    // Orders the current unit to attack a target that it is most likely to hit
    // Returns true if a target is found, returns false otherwise
    private bool AttackBestTarget()
    {
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        UnitController bestTarget = null;
        int bestProtection = 3; // NOTE: "Best" here means the lowest/easiest to hit
        for (int i = 0; i < unitList.Length; i++)
        {
            if (unitList[i].belongsToPlayer && unitList[i].IsTargetable())
            {
                int protection = MapManager.instance.GetProtection(currentEnemy.GetCurrentNode(), unitList[i].GetCurrentNode());
                if (protection < bestProtection)
                {
                    bestTarget = unitList[i];
                    bestProtection = protection;
                }
            }
        }
        if (bestTarget != null)
        {
            // Found a target
            currentEnemy.OrderAttack(bestTarget);
            return true;
        }
        else
        {
            // Couldn't find a target
            return false;
        }
    }

    /* Move the selected unit to the "best" spot
    * The ideal "best" spot is the closest location that is:
    * - Behind some sort of cover (NOTE: we don't care how much protection the cover gives,
    *  as long as it's something. This might change if we add an enemy unit with a stronger 
    *  sense of self-preservation)
    * - In range of the nearest target
    * 
    * If we can't find anywhere that is in range of a target, just get as close 
    * to a target as possible (even if that exposes this unit!)
    * NOTE: These units care more about attacking than protecting themselves, so if it has to
    * choose between being in cover or being in range of a target, it will ALWAYS choose being
    * in range!
    * 
    * Basically, the logic is:
    * Try to find a spot in range and in cover.
    * If we can't find a spot in cover, try to find a spot in range.
    * If we can't find a spot in range, find a spot as close to the nearest target as possible.
    */
    private void MoveToBestSpot()
    {
        int bestNode = -1;
        bool bestInCover = false;
        bool bestInRange = false;
        int bestDistance = int.MaxValue;
        int bestDistToTarget = int.MaxValue;
        UnitController nearestTarget = GetNearestTarget();

        List<int> nodes = MapManager.instance.GetPathableNodes();

        for (int i = 0; i < nodes.Count; i++)
        {
            int node = nodes[i];
            print("Checking node #" + node);
            // Get info about this node
            bool nodeInCover = !MapManager.instance.isExposed(node, false);
            int nodeDistToTarget = MapManager.instance.GetDistBetweenNodes(node, nearestTarget.GetCurrentNode());
            bool nodeInRange = (nodeDistToTarget <= currentEnemy.attackRange);
            int nodeDistance = MapManager.instance.GetDistBetweenNodes(node, currentEnemy.GetCurrentNode());

            // Is this node in range?
            if (nodeInRange)
            {
                // Yes!
                // Is this node in cover?
                if (nodeInCover)
                {
                    // Yes!
                    // Do we already have a similar spot that is closer?
                    if (bestInCover && bestInRange && bestDistance <= nodeDistance)
                    {
                        // Already have a closer node, so ignore this node
                        continue;
                    }
                    else
                    {
                        // Nope! Set this node as the best
                        print("Found a good node!");
                        bestNode = node;
                        bestInCover = nodeInCover;
                        bestInRange = nodeInRange;
                        bestDistance = nodeDistance;
                        bestDistToTarget = nodeDistToTarget;
                    }
                }
                else if (!bestInCover)
                {
                    // Not in cover, but we haven't found a spot in cover yet
                    // Do we already have a similar spot that is closer?
                    if (bestInRange && bestDistance <= nodeDistance)
                    {
                        // Already have a closer node, so ignore this node
                        continue;
                    }
                    else
                    {
                        // Nope! Set this node as the best
                        print("Found a good node!");
                        bestNode = node;
                        bestInCover = nodeInCover;
                        bestInRange = nodeInRange;
                        bestDistance = nodeDistance;
                        bestDistToTarget = nodeDistToTarget;
                    }
                }
                // Already have a node in cover; ignore this node
                continue;
            }
            else if (!bestInRange)
            {
                // Not in range, but we haven't found a spot in range yet
                // Do we already have a similar spot that is closer to the target?
                if (bestDistToTarget <= nodeDistToTarget)
                {
                    // Already have a closer node, so ignore this node
                    continue;
                }
                else
                {
                    // Nope! Set this node as the best
                    print("Found a good node!");
                    bestNode = node;
                    bestInCover = nodeInCover;
                    bestInRange = nodeInRange;
                    bestDistance = nodeDistance;
                    bestDistToTarget = nodeDistToTarget;
                }
            }
            // Already have a node in range; ignore this node
            continue;
        }

        // If we found a node, move the selected unit there!
        if (bestNode > 0)
        {
            print("Moving enemy unit!");
            currentEnemy.OrderMove(bestNode);
        }
    }

    // Returns true if there is at least one target in range of the selected unit
    private bool HasTarget()
    {
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        for (int i = 0; i < unitList.Length; i++)
        {
            if (unitList[i].belongsToPlayer && unitList[i].IsTargetable())
            {
                print("Enemy has at least one target!");
                return true;
            }
        }
        print("Enemy has found no targets!");
        return false;
    }

    // Returns the closest player unit to the currently selected unit
    private UnitController GetNearestTarget()
    {
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        int minDistance = int.MaxValue;
        UnitController closestUnit = null;
        for (int i = 0; i < unitList.Length; i++)
        {
            if (unitList[i].belongsToPlayer)
            {
                int distance = MapManager.instance.GetDistBetweenNodes(unitList[i].GetCurrentNode(), currentEnemy.GetCurrentNode());
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestUnit = unitList[i];
                }
            }
        }
        return closestUnit;
    }

    // Reset variables related to unit decision logic
    private void ResetUnitVars()
    {
        canMove = true;
        canAttack = true;
    }

    void DelaySceneChange(float delay)
    {
        StartCoroutine(DelayAction(delay));
    }

    IEnumerator DelayAction(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        if (GameManager.l1Complete && GameManager.l2Complete && GameManager.l3Complete)
        {
            GameManager.endGame = true;
            DialogueSelectManager.currLevel = "TitleScene";
            GameEnd.gameWin = false;
            MusicManager.instance.GetComponent<AudioSource>().Play();
            SceneManager.LoadScene("DialogueScene");
        }
        else
        {
            GameEnd.gameWin = false;
            MusicManager.instance.GetComponent<AudioSource>().Play();
            SceneManager.LoadScene("LevelSelectScene");
        }
    }
}
