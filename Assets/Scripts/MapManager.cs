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
    private int updateQueued;

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
        updateQueued = 0;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // Generate the nodes of the map grid        
        InitializeGrid();
        // Assign pre-placed units to the closest node
        AssignUnitNodes();

        SetNodeCover();
    }

    void Update()
    {
        // HACK: To avoid updating the grid before changes have taken place, we actually wait
        // TWO update cycles before updating the grid.
        if (updateQueued > 1)
        {
            updateQueued--;
        }
        else if (updateQueued == 1)
        {
            UpdateGrid();
            updateQueued--;
        }
    }

    // Tell the MapManager that the grid needs to be updated
    // Called the grid layout changes (e.g. when cover is destroyed)
    public void QueueUpdate()
    {
        updateQueued = 2;
    }

    internal void UpdateGrid()
    {
        PopulateAdjacencyMatrix();
        SetNodeCover();
        print("Grid updated!");
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
                //print("Placing node at: " + nodeX + ", " + nodeZ);
                nodeX += nodeDistance;
            }
            nodeX = -nodeDistance * gridWidth / 2;
            nodeZ -= nodeDistance;
        }

        PopulateAdjacencyMatrix();
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
    void PopulateAdjacencyMatrix()
    {
        // TODO: This assumes that all cardinally or diagonally adjacent nodes are reachable,
        // This will need to be changed to account for blocking objects (such as walls or cliffs).
        for (int nodeNum = 0; nodeNum < numNodes; nodeNum++)
        {
            // Get (up to) 4 nodes that are adjacent to the current node
            List<int> nearbyNodes = GetAdjacentNodes(nodeNum);

            for (int i = 0; i < nearbyNodes.Count; i++)
            {
                // Check if there's any objects between the nodes (such as cover)
                // Get bit mask for layer 6 (2^6) for Full Cover objects
                int fullCoverMask = 64;
                // Get bit mask for layer 7 (2^7) for Half Cover objects
                int halfCoverMask = 128;
                Vector3 nodePos = nodeList[nodeNum].transform.position;
                Vector3 nearbyPos = nodeList[nearbyNodes[i]].transform.position;
                if (Physics.Linecast(nodePos, nearbyPos, fullCoverMask))
                {
                    // Full cover in the way; don't set a connection
                    continue;
                }
                if (Physics.Linecast(nodePos, nearbyPos, halfCoverMask))
                {
                    // Half cover in the way; set the connection at an increased cost
                    ConnectNode(nodeNum, nearbyNodes[i], 2);
                    continue;
                }

                // Set the connection
                ConnectNode(nodeNum, nearbyNodes[i], 1);
            }
        }
    }

    // Returns the distance between nodes (irrespective of barriers)
    internal int GetDistBetweenNodes(int startNode, int endNode)
    {
        int dist = Mathf.Abs((startNode % gridWidth) - (endNode % gridWidth)) + Mathf.Abs((startNode / gridHeight) - (endNode / gridHeight));
        return dist;
    }

    // Returns the specified node object
    public NodeController GetNode(int nodeNum)
    {
        return nodeList[nodeNum];
    }

    // Assign every pre-placed unit a map node at their current location
    private void AssignUnitNodes()
    {
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
                    //print("Setting minNode to: " + j);
                    minNode = j;
                    minDist = dist;
                }
            }
            // Set this unit to the closest node.
            unitList[i].GetComponent<UnitController>().SetCurrentNode(minNode);
        }
        UpdateNodeOccupation();
    }

    // Properly mark which nodes have a unit currently standing on them
    public void UpdateNodeOccupation()
    {
        // Set all nodes as unoccupied...
        for (int i = 0; i < nodeList.Length; i++)
        {
            nodeList[i].SetOccupied(false);
        }
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        // Set all occupied nodes...
        for (int i = 0; i < unitList.Length; i++)
        {
            nodeList[unitList[i].GetCurrentNode()].SetOccupied(true);
        }
    }

    // Mark pathable nodes from the given source node
    public void GeneratePathsFromNode(int sourceNode, int pathPower, bool visualize)
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
                    if (visualize && !nodeList[currentNode].IsOccupied()) nodeList[currentNode].SetPathable(true);
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

    // Returns every node that can currently be pathed to
    public List<int> GetPathableNodes()
    {
        List<int> pathableNodes = new List<int>();

        for (int i = 0; i < numNodes; i++)
        {
            if (nodeList[i].GetPreviousNode() >= 0 && !nodeList[i].IsOccupied()) pathableNodes.Add(i);
        }

        return pathableNodes;
    }

    // Clear any node path data from the grid
    public void ResetNodePaths()
    {
        for (int i = 0; i < numNodes; i++)
        {
            nodeList[i].ResetNode();
        }
    }

    // Set the level of protection each node has on each side.
    internal void SetNodeCover()
    {
        for (int i = 0; i < nodeList.Length; i++)
        {
            NodeController currentNode = nodeList[i];
            int[] coverInfo = new int[4];
            Vector3 nodePos = currentNode.transform.position;
            // Check for cover around the target node
            // Get bit mask for layer 6 (2^6) for Full Cover objects
            int fullCoverMask = 64;
            // Get bit mask for layer 7 (2^7) for Half Cover objects
            int halfCoverMask = 128;

            // Check north cover
            if (Physics.Linecast(nodePos, nodePos + new Vector3(0f, 0f, nodeDistance), fullCoverMask))
            {
                coverInfo[0] = 2;
            }
            else if (Physics.Linecast(nodePos, nodePos + new Vector3(0f, 0f, nodeDistance), halfCoverMask))
            {
                coverInfo[0] = 1;
            }

            // Check east cover
            if (Physics.Linecast(nodePos, nodePos + new Vector3(nodeDistance, 0f, 0f), fullCoverMask))
            {
                coverInfo[1] = 2;
            }
            else if (Physics.Linecast(nodePos, nodePos + new Vector3(nodeDistance, 0f, 0f), halfCoverMask))
            {
                coverInfo[1] = 1;
            }

            // Check south cover
            if (Physics.Linecast(nodePos, nodePos + new Vector3(0f, 0f, -nodeDistance), fullCoverMask))
            {
                coverInfo[2] = 2;
            }
            else if (Physics.Linecast(nodePos, nodePos + new Vector3(0f, 0f, -nodeDistance), halfCoverMask))
            {
                coverInfo[2] = 1;
            }

            // Check west cover
            if (Physics.Linecast(nodePos, nodePos + new Vector3(-nodeDistance, 0f, 0f), fullCoverMask))
            {
                coverInfo[3] = 2;
            }
            else if (Physics.Linecast(nodePos, nodePos + new Vector3(-nodeDistance, 0f, 0f), halfCoverMask))
            {
                coverInfo[3] = 1;
            }

            currentNode.SetCoverInfo(coverInfo);
        }
    }

    // Get the level of protection the target node has from the source node
    /* O O O X X
     * O O O X X
     * O #|X X X
     * O O O X X
     * O O O X X
     * '#' represents a unit behind cover. The unit behind the cover 
     * would receive protection from any attacker standing on an 'X' tile, but
     * no protection from attackers standing on an 'O' tile.
     * X X X X X
     * O O _ X X
     * O O #|X X
     * O O O O X
     * O O O O X
     */
    // Also set the target node's front cover to the piece of cover that will absorb a missed attack
    // If the source is 2 or more tiles away, use the cover that gives the best protection.
    // Otherwise, use the cover that gives the worst protection.
    // FIXME: This code seems extraordinarily brain-dead
    internal int GetProtection(int sourceNode, int targetNode)
    {
        Vector3 sourcePos = nodeList[sourceNode].transform.position;
        Vector3 targetPos = nodeList[targetNode].transform.position;
        Vector3 displacement = sourcePos - targetPos;
        print("displacement X: " + displacement.x);
        print("displacement Z: " + displacement.z);
        int protection = 0;
        int coverDirection = -1;

        const int north = 0;
        const int east = 1;
        const int south = 2;
        const int west = 3;

        int northCoverProt = nodeList[targetNode].GetCoverInfo(north);
        int eastCoverProt = nodeList[targetNode].GetCoverInfo(east);
        int southCoverProt = nodeList[targetNode].GetCoverInfo(south);
        int westCoverProt = nodeList[targetNode].GetCoverInfo(west);

        // Handles when source and target are at least 2 tiles away
        if (displacement.z >= 2 * nodeDistance)
        {
            // Source is northward of the target
            if (northCoverProt > protection)
            {
                protection = northCoverProt;
                coverDirection = north;
            }
        }
        if (displacement.x >= 2 * nodeDistance)
        {
            // Source is eastward of the target
            if (eastCoverProt > protection)
            {
                protection = eastCoverProt;
                coverDirection = east;
            }
        }
        if (displacement.z <= -2 * nodeDistance)
        {
            // Source is southward of the target
            if (southCoverProt > protection)
            {
                protection = southCoverProt;
                coverDirection = south;
            }
        }
        if (displacement.x <= -2 * nodeDistance)
        {
            // Source is westward of the target
            if (westCoverProt > protection)
            {
                protection = westCoverProt;
                coverDirection = west;
            }
        }

        // Handles when source and target are 1 tile away
        if (displacement.z == nodeDistance && displacement.x == 0)
        {
            // Source is directly north of the target
            coverDirection = north;
        }
        else if (displacement.x == nodeDistance && displacement.z == 0)
        {
            // Source is directly east of the target
            coverDirection = east;
        }
        else if (displacement.z == -nodeDistance && displacement.x == 0)
        {
            // Source is directly south of the target
            coverDirection = south;
        }
        else if (displacement.x == -nodeDistance && displacement.z == 0)
        {
            // Source is directly west of the target
            coverDirection = west;
        }
        else if (displacement.z == nodeDistance && displacement.x == nodeDistance)
        {
            // Source is directly northeast of target
            // Get minimum amount of cover applicable
            if (northCoverProt < eastCoverProt)
            {
                coverDirection = north;
            }
            else
            {
                coverDirection = east;
            }
        }
        else if (displacement.z == -nodeDistance && displacement.x == nodeDistance)
        {
            // Source is directly southeast of target
            if (eastCoverProt < southCoverProt)
            {
                coverDirection = east;
            }
            else
            {
                coverDirection = south;
            }
        }
        else if (displacement.z == -nodeDistance && displacement.x == -nodeDistance)
        {
            // Source is directly southwest of target
            if (southCoverProt < westCoverProt)
            {
                coverDirection = south;
            }
            else
            {
                coverDirection = west;
            }
        }
        else if (displacement.z == nodeDistance && displacement.x == -nodeDistance)
        {
            // Source is directly northwest of target
            if (westCoverProt < northCoverProt)
            {
                coverDirection = west;
            }
            else
            {
                coverDirection = north;
            }
        }
        if (coverDirection >= 0)
        {
            protection = nodeList[targetNode].GetCoverInfo(coverDirection);
        }

        // Get the actual cover object that will absorb a missed attack (if any)
        Vector3 rayDirection;
        switch (coverDirection)
        {
            default:
            case north:
                rayDirection = new Vector3(0f, 0f, 1f);
                break;
            case east:
                rayDirection = new Vector3(1f, 0f, 0f);
                break;
            case south:
                rayDirection = new Vector3(0f, 0f, -1f);
                break;
            case west:
                rayDirection = new Vector3(-1f, 0f, 0f);
                break;
        }
        
        RaycastHit hit;
        // Get a bit mask for Full Cover (2^6) and Half Cover (2^7) objects
        int layerMask = (64 + 128);
        
        // Get the cover object in the front direction (if any)
        if (Physics.Raycast(targetPos, rayDirection, out hit, nodeDistance, layerMask))
        {
            // Abra cadabra
            //print(hit.transform.gameObject.name);
            CoverController frontCover = (CoverController)hit.transform.gameObject.GetComponent(typeof(CoverController));
            nodeList[targetNode].SetFrontCover(frontCover);
        }
        else
        {
            // Nothing found
            nodeList[targetNode].SetFrontCover(null);
        }

        return protection;
    }

    // Returns true if the given node is fully exposed to an opposing unit
    // isPlayer is true when checking if a player unit is exposed
    internal bool isExposed(int nodeNum, bool isPlayer)
    {
        UnitController[] unitList = FindObjectsByType<UnitController>(FindObjectsSortMode.None);
        for (int i = 0; i < unitList.Length; i++)
        {
            UnitController opUnit = unitList[i];
            // Return true if:
            // The unit is on the opposite team
            // The given node is within it's weapon range
            // There is no cover protecting the given node
            if (opUnit.belongsToPlayer != isPlayer 
                && GetDistBetweenNodes(opUnit.GetCurrentNode(), nodeNum) <= opUnit.attackRange
                && GetProtection(opUnit.GetCurrentNode(), nodeNum) == 0)
            {
                // We're exposed!
                return true;
            }
        }

        return false;
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
