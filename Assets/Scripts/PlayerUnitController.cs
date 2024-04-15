using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerUnitController : MonoBehaviour, IPointerClickHandler
{
    // How far this unit can move with one action point
    public int movePower;
    // How fast the unit moves between nodes (purely visual)
    public float moveSpeed;

    // What path node this unit is currently at
    private int currentNode;
    // Whether this unit is currently idle (and can be selected and ordered)
    private bool idle;
    // Whether this unit is currently moving (and should be visually moving towards a node)
    private bool moving;
    // The path of nodes currently being traversed (if any)
    private List<int> nodePath;
    
    // Start is called before the first frame update
    void Start()
    {
        idle = true;
        moving = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (moving)
        {
            // Get the last node currently in the list
            int nodeNum = nodePath[nodePath.Count - 1];
            NodeController currentNode = MapManager.instance.GetNode(nodeNum);
            // Get the node's position
            Vector3 nodePostion = currentNode.transform.position;
            if (transform.position != nodePostion)
            {
                // Move towards the node
                transform.position = Vector3.Lerp(transform.position, nodePostion, moveSpeed);
            }
            else
            {
                // Start moving towards the next node (if any)
                // Remove the last node from the list
                nodePath.RemoveAt(nodePath.Count - 1);

                if (nodePath.Count == 0)
                {
                    // If there's no more nodes left, stop moving. We've reached our destination!
                    moving = false;
                    idle = true;
                    this.currentNode = nodeNum;
                }
            }
        }
    }

    // Set this unit as the selected unit
    public void OnPointerClick(PointerEventData eventData)
    {
        if (idle)
        {
            GameManager.instance.SetSelectedUnit(this);
            MapManager.instance.GeneratePathsFromNode(currentNode, movePower);
        }
    }

    public void SetIdle(bool isIdle)
    {
        idle = isIdle;
    }

    public bool IsIdle()
    {
        return idle;
    }

    public void SetCurrentNode(int newNode)
    {
        currentNode = newNode;
        print("currentNode: " + currentNode);
    }

    public int GetCurrentNode()
    {
        return currentNode;
    }

    public void StartMoving(List<int> nodePath)
    {
        idle = false;
        moving = true;
        this.nodePath = nodePath;
    }
}
