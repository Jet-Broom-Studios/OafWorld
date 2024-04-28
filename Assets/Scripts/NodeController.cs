using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    // Default node material
    public Material neutralMaterial;
    // Material for nodes that can be pathed to
    public Material pathableMaterial;
    // Material for nodes that are selected
    public Material selectedMaterial;

    private new Renderer renderer;
    private bool selected;
    private bool pathable;
    private int prevNode;
    private bool processed;
    private int pathPower;
    private int nodeNumber;
    private int[] coverInfo;
    // If a unit on this node is unsuccessfully attacked, damage this cover object instead
    private CoverController frontCover;

    public void Awake()
    {
        renderer = GetComponent<Renderer>();
        ResetNode();
        coverInfo = new int[4];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //print("Click!");
        if (pathable)
        {
            renderer.material = selectedMaterial;
            selected = true;
            print("prevNode: " + prevNode + ", pathPower: " + pathPower);
            // Get a path of nodes
            List<int> nodePath = MapManager.instance.GetNodePath(this);
            // Start moving the selected unit towards this node
            GameManager.instance.GetSelectedUnit().OrderMove(nodePath);
        }
    }

    // Reset all state and pathing data of this node
    public void ResetNode()
    {
        selected = false;
        SetPathable(false);
        prevNode = -1;
        processed = false;
        pathPower = -1;
    }    

    public void OnPointerEnter(PointerEventData eventData)
    {
        //print("Enter!");
        if (pathable)
        {
            renderer.material = selectedMaterial;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //print("Exit!");
        if (pathable && !selected)
        {
            renderer.material = pathableMaterial;
        }
    }

    public void SetNumber(int number)
    {
        nodeNumber = number;
    }

    public int GetNumber()
    {
        return nodeNumber;
    }

    // Used for path generation
    public void SetPathable(bool isPathable)
    {
        pathable = isPathable;
        if (pathable)
        {
            renderer.material = pathableMaterial;
        }
        else
        {
            renderer.material = neutralMaterial;
        }
    }

    public void SetPreviousNode(int previous)
    {
        prevNode = previous;
    }

    public int GetPreviousNode()
    {
        return prevNode;
    }

    public void SetProcessed(bool isProcessed)
    {
        processed = isProcessed;
    }

    public bool IsProcessed()
    {
        return processed;
    }

    public void SetPathPower(int newPathPower)
    {
        pathPower = newPathPower;
    }

    public int GetPathPower()
    {
        return pathPower;
    }

    // Set info about how covered this node is.
    // side: 0 (north), 1 (east), 2 (south), 3 (west)
    // state: 0 (no cover), 1 (half cover), 2 (full cover)
    public void SetCoverInfo(int side, int state)
    {
        coverInfo[side] = state;
    }

    public void SetCoverInfo(int[] newCoverInfo)
    {
        coverInfo = newCoverInfo;
    }

    public int GetCoverInfo(int side)
    {
        return coverInfo[side];
    }

    public void SetFrontCover(CoverController side)
    {
        frontCover = side;
    }

    public CoverController GetFrontCover()
    {
        return frontCover;
    }
}
