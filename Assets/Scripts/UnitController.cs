using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnitController : MonoBehaviour, IPointerClickHandler, IDamageable
{
    // Human-friendly name for this unit
    public string unitName;
    // Whether this unit is belongs to the player or not
    public bool belongsToPlayer;
    // How far this unit can move with one action point
    public int movePower;
    // How fast the unit moves between nodes (purely visual)
    public float moveSpeed;
    // How far this unit can attack
    public int attackRange;
    // How much damage this unit's attack does
    public int attackDamage0;
    public int attackDamage1;
    public int attackDamage2;
    // This unit's maximum HP
    public int maxHealth;

    // Default node material
    public Material neutralMaterial;
    // Material for nodes that can be pathed to
    public Material targetableMaterial;
    private WizSoundManager wizSounds;
    private RobotSoundManager roboSounds;

    private new Renderer renderer;
    // How much HP this unit currently has
    internal int currentHealth;
    // How many action points this unit currently has
    internal int actionPoints;
    // What path node this unit is currently at
    private int currentNode;
    // Whether this unit is currently idle (and can be selected and ordered)
    private bool idle;
    // Whether this unit is currently moving (and should be visually moving towards a node)
    private bool moving;
    // Whether this unit is currently attacking (and should be visually doing some sort of animation)
    private bool attacking;
    // The path of nodes currently being traversed (if any)
    private List<int> nodePath;
    // Whether this unit can be targetted by another unit (such as when the player is choosing what to shoot)
    private bool targetable;
    // What unit is currently being targeted (can be a friendly unit for beneficial spells, etc.)
    private UnitController currentTarget;

    // script to start animations (which will finalize actions)
    PlayerAnimScript anim;

    // int to select which of the three actions to use when attacking
    private int action = 0; // Defaults to action0

    // Start is called before the first frame update
    void Start()
    {
        idle = true;
        moving = false;
        renderer = GetComponent<Renderer>();
        renderer.material = neutralMaterial;
        currentHealth = maxHealth;
        actionPoints = 2;

        wizSounds = GetComponent<WizSoundManager>();
        roboSounds = GetComponent<RobotSoundManager>();

        anim = GetComponent<PlayerAnimScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            float NODE_SNAP_THRESHOLD = 0.05f;
            // Get the last node currently in the list
            int nodeNum = nodePath[nodePath.Count - 1];
            NodeController targetNode = MapManager.instance.GetNode(nodeNum);
            if (Mathf.Abs((transform.position - targetNode.transform.position).magnitude) > NODE_SNAP_THRESHOLD)
            {
                // Get the direction to the node
                Vector3 nodeDirection = (targetNode.transform.position - transform.position).normalized;
                // Distance ( speed = distance / time --> distance = speed * time)
                float distance = moveSpeed * Time.deltaTime;
                // Get the movement vector
                Vector3 movement = nodeDirection * distance;
                // Move towards the node
                transform.position += movement;
            }
            else
            {
                // Snap to the target nodes's position
                transform.position = targetNode.transform.position;

                // Start moving towards the next node (if any)
                // Remove the last node from the list
                nodePath.RemoveAt(nodePath.Count - 1);

                if (nodePath.Count == 0)
                {
                    // If there's no more nodes left, stop moving. We've reached our destination!
                    FinishAction();
                    this.currentNode = nodeNum;
                }
            }
        }
        else if (attacking)
        {
            // TODO: Do some sort of animation schenanigans here, right now
            // this just instantly damages the target without any visual feedback.

            // Check if the target is in cover
            float chanceToHit = 1 - (0.5f * MapManager.instance.GetProtection(currentNode, currentTarget.currentNode));
            print(name + " is attacking " + currentTarget.unitName + " with a " + chanceToHit + " chance to hit.");
            bool hit = (Random.value < chanceToHit);

            if (hit)
            {
                // Damage the target with proper damage depending on selected action
                if (action == 0)
                {
                    currentTarget.ChangeHealth(-attackDamage0);
                }
                else if (action == 1)
                {
                    currentTarget.ChangeHealth(-attackDamage1);
                }
                else if (action == 2)
                {
                    currentTarget.ChangeHealth(-attackDamage2);
                }
            }
            else
            {
                // Damage the cover (if it exists) with proper damage depending on selected action
                print("Missed target!");
                NodeController targetNode = MapManager.instance.GetNode(currentTarget.currentNode);
                if (targetNode.GetFrontCover() != null)
                {
                    print("front cover found!");
                    if (action == 0)
                    {
                        targetNode.GetFrontCover().ChangeHealth(-attackDamage0);
                    }
                    else if (action == 1)
                    {
                        targetNode.GetFrontCover().ChangeHealth(-attackDamage1);
                    }
                    else if (action == 2)
                    {
                        targetNode.GetFrontCover().ChangeHealth(-attackDamage2);
                    }
                }
            }

            // Attack done
            FinishAction();
        }
    }

    // Damage or heal this unit
    // If the new HP total is 0 or less, kill the unit instead.
    // If the new HP total is greater than maxHealth, set it to maxHealth instead.
    public void ChangeHealth(int deltaHP)
    {
        if (this != null)   // Was trying to access dead units, so I added this
        {
            currentHealth += deltaHP;
            if (deltaHP > 0)
            {
                print(name + " healed " + deltaHP + " HP!");
            }
            else
            {
                print(name + " lost " + -deltaHP + " HP!");
            }

            if (currentHealth <= 0)
            {
                Kill();
            }
            else if (currentHealth > maxHealth)
            {
                currentHealth = maxHealth;
            }
    }
}

    // Destroy this unit
    public void Kill()
    {
        // TODO: Maybe do some animation or whatnot depending on what unit this is/how it died?
        // Right now this just makes the unit disappear.
        print(name + " has died!");
        Destroy(gameObject);
    }

    // Set this unit as the selected unit
    public void OnPointerClick(PointerEventData eventData)
    {
        if (belongsToPlayer)
        {
            // Trying to select a unit
            // Can't select a unit that is currently performing an action (i.e. moving or shooting)
            if (idle)
            {
                GameManager.instance.SetSelectedUnit(this);
            }
        }
        else
        {
            // Trying to target an enemy unit
            if (targetable && GameManager.instance.GetSelectedUnit() != null)
            {
                GameManager.instance.GetSelectedUnit().OrderAttack(this);
            }
        }
    }

    // Move along the given path of nodes
    public void OrderMove(List<int> nodePath)
    {
        actionPoints--;
        SetIdle(false);
        moving = true;
        if (moving && wizSounds != null)
        {
            wizSounds.PlayWalk();
        }
        this.nodePath = nodePath;
    }

    public void OrderMove(int node)
    {
        if (roboSounds != null)
        {
            roboSounds.PlayRoboMove();
        }
        OrderMove(MapManager.instance.GetNodePath(MapManager.instance.GetNode(node)));
    }

    // Attack the target unit
    public void OrderAttack(UnitController targetUnit)
    {
        anim.StartAttack(targetUnit, action);
        if (!belongsToPlayer && roboSounds != null) {
            roboSounds.PlayRoboAtk();
        }
    }
    // Contains what used to exist in OrderAttack, now called from PlayerAnimScript
    public void commitAction(UnitController targetUnit)
    {
        actionPoints--;
        SetIdle(false);
        attacking = true;
        currentTarget = targetUnit;
    }

    public void SetIdle(bool isIdle)
    {
        idle = isIdle;
        if (!idle)
        {
            // If this unit is selected, deselect it
            if (GameManager.instance.GetSelectedUnit() == this)
            {
                GameManager.instance.SetSelectedUnit(null);
            }
        }
    }

    public bool IsIdle()
    {
        return idle;
    }

    public void SetCurrentNode(int newNode)
    {
        currentNode = newNode;
        //print("currentNode: " + currentNode);
    }

    public int GetCurrentNode()
    {
        return currentNode;
    }

    public void SetTargetable(bool isTargetable)
    {
        targetable = isTargetable;
        if (targetable)
        {
            renderer.material = targetableMaterial;
        }
        else
        {
            renderer.material = neutralMaterial;
        }
    }

    public bool IsTargetable()
    {
        return targetable;
    }

    // Reset all action-related variables, and tell the Game Manager to check if the turn is over
    private void FinishAction()
    {
        SetIdle(true);
        moving = false;
        attacking = false;
        GameManager.instance.CheckTurnDone();
        // Update which spots are taken if a unit moves or dies
        MapManager.instance.UpdateNodeOccupation();
        if (wizSounds != null)
        {
            wizSounds.StopWalk();
        }

        if (roboSounds != null)
        {
            roboSounds.StopRoboMove();
        }
    }

    public void SelectActions(int actionSelection)
    {
        action = actionSelection;
    }
}
