using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    // Static instance of the Map Manager,
    // can be access from anywhere
    public static MapManager instance = null;

    // How far away each node is from their cardinal neighbors
    public float nodeDistance;

    // How many nodes long (x-direction) the grid should be.
    public int gridWidth;

    // How many nodes long (z-direction) the grid should be.
    public int gridHeight;

    // The node object to use
    public NodeController nodeObject;

    private int numNodes;
    private NodeController[] nodeList;
    private int[,] adjacencyMatrix;

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

        numNodes = gridWidth * gridHeight;
        nodeList = new NodeController[numNodes];
        adjacencyMatrix = new int[numNodes, numNodes];
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Generate the nodes of the map grid        
        InitializeGrid();
        // Assign pre-placed units to the closest node
        AssignUnitNodes();
    }

    // Create a grid of pathing nodes on the map
    void InitializeGrid()
    {
        // Start at top left corner (-x, +z)
        float nodeX = -nodeDistance * gridWidth / 2;
        float nodeZ = nodeDistance * gridHeight / 2;

        for (int z = 0; z < gridHeight; z++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                // Create a new node and add it to nodeList
                // TODO: Allow the heights (y-value) of the nodes to vary depending on terrain
                nodeList[z * gridWidth + x] = Object.Instantiate(nodeObject, new Vector3(nodeX, 0f, nodeZ), Quaternion.identity);
                nodeList[z * gridWidth + x].SetNumber(z * gridWidth + x);
                print("Placing node at: " + nodeX + ", " + nodeZ);
                nodeX += nodeDistance;
            }
            nodeX = -nodeDistance * gridWidth / 2;
            nodeZ -= nodeDistance;
        }

        // Fill out the adjacency matrix
        /* adjacencyMatrix represents a square 2d array with dimensions of numNodes,
        * so if there are 9 nodes total, adjacencyMatrix would look something like this:
        * 0 1 0 1 0 0 0 0 0
        * 1 0 1 0 1 0 0 0 0
        * 0 1 0 0 0 1 0 0 0
        * 1 0 0 0 1 0 1 0 0
        * 0 1 0 1 0 1 0 1 0
        * 0 0 1 0 1 0 0 0 1
        * 0 0 0 1 0 0 0 1 0
        * 0 0 0 0 1 0 1 0 1
        * 0 0 0 0 0 1 0 1 0
        * Where each number represents the cost of moving from node [row] to node [column] (and 0 means no connection).
        * Note that the nodes themselves in the game world would be arranged like this:
        * 0 1 2
        * 3 4 5
        * 6 7 8
        */

        // TODO: This assumes that all cardinally or diagonally adjacent nodes are reachable,
        // This will need to be changed to account for blocking objects (such as walls or cliffs).
        for (int nodeNum = 0; nodeNum < numNodes; nodeNum++)
        {
            // Get (up to) 4 nodes that are adjacent to the current node
            List<int> nearbyNodes = GetAdjacentNodes(nodeNum);

            for (int i = 0; i < nearbyNodes.Count; i++)
            {
                // Set the connection
                ConnectNode(nodeNum, nearbyNodes[i], 1);
            }
        }

        // DEBUG
        //print("Adjacency Matrix:");
        //for (int r = 0; r < numNodes; r++)
        //{
        //    string row = "";
        //    for (int c = 0; c < numNodes; c++)
        //    {
        //        row += adjacencyMatrix[r,c] + ", ";
        //    }
        //    print("Row " + r + ": " + row);
        //}
    }

    // Returns the specified node object
    public NodeController GetNode(int nodeNum)
    {
        return nodeList[nodeNum];
    }

    // Assign every pre-placed unit a map node at their current location
    private void AssignUnitNodes()
    {
        // TODO: This just considers player unit objects right now.
        // Any units added later (e.g. enemy units) will also need to be processed here!

        // Get the units on the map...
        GameObject[] unitList = GameObject.FindGameObjectsWithTag("Unit");
        // ...and for each one, find the closest node.
        for (int i = 0; i < unitList.Length; i++)
        {
            Vector3 currentPos = unitList[i].transform.position;
            int minNode = -1;
            float minDist = Mathf.Infinity;
            for (int j = 0; j < numNodes; j++)
            {
                Vector3 nodePos = nodeList[j].transform.position;
                float dist = Vector3.Distance(nodePos, currentPos);
                if (dist < minDist)
                {
                    print("Setting minNode to: " + j);
                    minNode = j;
                    minDist = dist;
                }
            }
            // Set this unit to the closest node.
            // TODO: The script name may be changed in the future! Also scripts for different units may have different names!
            unitList[i].GetComponent<PlayerUnitController>().SetCurrentNode(minNode);
        }
    }

    // Mark pathable nodes from the given source node
    public void GeneratePathsFromNode(int sourceNode, int pathPower)
    {
        ResetNodePaths();
        List<int> nodesToProcess = GetConnectedNodes(sourceNode, pathPower);
        nodeList[sourceNode].SetProcessed(true);
        nodeList[sourceNode].SetPathPower(pathPower);
        nodeList[sourceNode].SetPreviousNode(-1);

        while (nodesToProcess.Count > 0)
        {
            List<int> newNodesToProcess = new List<int>();
            for (int i = 0; i < nodesToProcess.Count; i++)
            { 
                int currentNode = nodesToProcess[i];
                // Only execute if we haven't processed this node already
                if (!nodeList[currentNode].IsProcessed())
                {
                    int highestPrevNode = -1;
                    int highestPower = -1;
                    // Find a previous node that leaves us with the highest remaining power
                    for (int j = 0; j < numNodes; j++)
                    {
                        if (adjacencyMatrix[j, currentNode] > 0 && nodeList[j].GetPathPower() - adjacencyMatrix[j, currentNode] > highestPower)
                        {
                            highestPrevNode = j;
                            highestPower = nodeList[j].GetPathPower() - adjacencyMatrix[j, currentNode];
                        }
                    }
                    nodeList[currentNode].SetPathable(true);
                    nodeList[currentNode].SetPreviousNode(highestPrevNode);
                    nodeList[currentNode].SetProcessed(true);
                    nodeList[currentNode].SetPathPower(highestPower);

                    // Get all the nodes that can be reached from the current node
                    newNodesToProcess.AddRange(GetConnectedNodes(currentNode, highestPower));
                }
            }
            nodesToProcess = newNodesToProcess;
        }
    }

    public List<int> GetNodePath(NodeController endNode)
    {
        List<int> nodePath = new List<int>();
        nodePath.Add(endNode.GetNumber());
        NodeController currentNode = endNode;
        while (currentNode.GetPreviousNode() != -1)
        {
            nodePath.Add(currentNode.GetPreviousNode());
            currentNode = nodeList[currentNode.GetPreviousNode()];
        }

        string path = "";
        for (int i = 0; i < nodePath.Count; i++)
        {
            path += nodePath[i] + ", ";
        }
        print("made path: " + path);


        return nodePath;
    }

    // Clear any node path data from the grid
    public void ResetNodePaths()
    {
        for (int i = 0; i < numNodes; i++)
        {
            nodeList[i].ResetNode();
        }
    }

    private void ConnectNode(int sourceNode, int destinationNode, int pathCost)
    {
        adjacencyMatrix[sourceNode, destinationNode] = pathCost;
    }

    // Returns a list of the nodes adjacent to the given node
    private List<int> GetAdjacentNodes(int sourceNode)
    {
        int[] nearbyNodes = {
            sourceNode - gridWidth,
            sourceNode - 1, sourceNode + 1,
            sourceNode + gridWidth,
        };

        // Check that none of the nodes are out of bounds or bleed into a different row
        List<int> validNodeList = new List<int>();
        for (int i = 0; i < nearbyNodes.Length; i++)
        {
            if (nearbyNodes[i] >= 0 && nearbyNodes[i] < numNodes && Mathf.Abs((sourceNode % gridWidth) - (nearbyNodes[i] % gridWidth)) <= 1)
            {
                validNodeList.Add(nearbyNodes[i]);
            }
        }

        return validNodeList;
    }

    // Returns an array of vectors (x,y) where x is the node number, and y is the cost to move to x.
    private List<int> GetConnectedNodes(int sourceNode, int power)
    {
        List<int> connectedNodeList = new List<int>();
        for (int i = 0; i < numNodes; i++)
        {
            if (adjacencyMatrix[sourceNode, i] > 0 && adjacencyMatrix[sourceNode, i] <= power)
            {
                connectedNodeList.Add(i);
            }
        }

        return connectedNodeList;
    }
}
