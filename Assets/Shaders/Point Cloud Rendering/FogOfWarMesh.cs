///
/// Code By Ahmaderfani12, Edited by Kieran Coppins
/// GitHub: https://github.com/ahmaderfani12/PointClouds
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWarMesh : MeshToPointCloud
{
    //An array of all robot gameobjects in the scene
    private GameObject[] robots;

    //The viewport instance
    [SerializeField] 
    public Transform viewPort;

    //stores the view radius for each robot
    [SerializeField, Range(0, 50)] 
    private int viewRadius = 1;

    //Stores the viewports location
    private Vector3 viewPortLocation;

    //Compute Buffers for the compute shader
    private ComputeBuffer positionsBufferTemp;  //Stores the x, y, z positions of each vertex on the complete mesh
    private ComputeBuffer visitedBuffer;        //Stores which point index has been visited - loads visitedArray

    //Stores which point index has been visited - 0 unvisited 1 visited.
    private int[] visitedArray;

    //How many groups should the KGrid be split into (k x k x k) or in our case (k x 1 x k)
    [SerializeField, Range (2, 200)]
    private int kGridGroups;

    //Our KGrid
    private KGrid kGrid;

    //Stores how many points we have visited - used for calculating explored percentage
    public float visitedCount { get; private set; }

    //How many vertices are in our mesh
    public float vertexCount { get; private set; }

    private void Awake()
    {
        //Try get the loaded level number and load it
        try
        {
            sourceMesh = sourceMeshes[PlayerPrefs.GetInt("Level")];
        }
        //Otherwise just use the first source mesh by default
        catch
        {
            Debug.LogError("Couldn't Get Player Prefs");
            sourceMesh = sourceMeshes[0];
        }
        //Update our mesh collider to the source mesh
        GetComponent<MeshCollider>().sharedMesh = sourceMesh;

        visitedCount = 0;

        //Create an array of int = 0 with the length of how many vertices we have
        visitedArray = new int[sourceMesh.vertices.Length];

        //Find all our robots with the robot tag (slow but only happens once)
        robots = GameObject.FindGameObjectsWithTag("Robot");

        InitializeFromMeshData();
        SetTempPositionBufferToComputeShader();

        //Set our vertex count from the source mesh
        vertexCount = sourceMesh.vertexCount;
    }

    // Start is called before the first frame update
    void Start()
    {
        visitedArray = new int[sourceMesh.vertices.Length];
        Debug.Log("sourcemesh vertex count: " + sourceMesh.vertexCount);
        Debug.Log("sourcemesh vertex[0]: " + sourceMesh.vertices[0]);
        kGrid = new KGrid(sourceMesh.vertices, kGridGroups);
        robots = GameObject.FindGameObjectsWithTag("Robot");
        StartCoroutine(LineOfSight());
    }

    // Update is called once per frame
    void Update()
    {
        base.Update();
        UpdateVisitedBuffer();
        viewPortLocation = viewPort.position - transform.position;
        computeShader.SetVector("_viewportPos", viewPortLocation);
        computeShader.SetVector("_viewportSize", viewPort.lossyScale / 2);
        computeShader.SetVector("_transformScale", this.transform.lossyScale);


        DispachComputeShader();

    }

    private void SetTempPositionBufferToComputeShader()
    {
        positionsBufferTemp = new ComputeBuffer(sourceMesh.vertexCount, 3 * 4);
        positionsBufferTemp.SetData(sourceMesh.vertices);
        computeShader.SetBuffer(0, "_PositionsTemp", positionsBufferTemp);


        visitedBuffer = new ComputeBuffer(sourceMesh.vertexCount, 4);
        UpdateVisitedBuffer();
    }

    private void UpdateVisitedBuffer()
    {
        visitedBuffer.SetData(visitedArray);
        computeShader.SetBuffer(0, "_Visited", visitedBuffer);
    }

    IEnumerator LineOfSight()
    {
        while (true)
        {
            for (int i = 0; i < robots.Length; i++)
            {
                //Get the grid coordiates from the "KGrid" with the local position of the agents (they are children of the environment)
                kGrid.GetIndex(robots[i].transform.localPosition, out int indexX, out int indexY, out int indexZ);

                //Loop through 3 dimensional space (x, y, z) - could be adjusted to just include all grids in the Y direction
                for (int x = indexX - viewRadius; x <= indexX + viewRadius; x++)
                {
                    //Check if x is out of range
                    if (x < 0 || x > kGridGroups)
                        continue;
                    for (int y = indexY - viewRadius; y <= indexY + viewRadius; y++)
                    {
                        //Check if y is out of range - updated to ignore Y coordinate - a lot of code is now redundant but kept incase wanting to allow for Y grid seperation
                        if (y != 0)
                            continue;
                        for (int z = indexZ - viewRadius; z <= indexZ + viewRadius; z++)
                        {
                            //check if z is out of range
                            if (z < 0 || z > kGridGroups)
                                continue;

                            //Get the indexes stored in the OcTree
                            int[] indexes = kGrid.GetIndexes(x, y, z);

                            //Loop through all the indexes
                            for (int j = 0; j < indexes.Length; j++)
                            {
                                //If one item is set to be visited then the whole section of the grid will be
                                if (visitedArray[indexes[j]] == 1)
                                {
                                    break;
                                }

                                //Set the point to be visited
                                visitedArray[indexes[j]] = 1;

                                visitedCount++;

                                //Old - SLOW system, allows for individual points within a grid to be checked whereas faster system unlocks larger chunks
                                /*
                                Vector3 worldPoint = transform.TransformPoint(sourceMesh.vertices[indexes[j]]);
                                if (worldPoint.x > pos.x - viewRadius && worldPoint.x < pos.x + viewRadius)
                                {
                                    if (worldPoint.y > pos.y - viewRadius && worldPoint.y < pos.y + viewRadius)
                                    {
                                        if (worldPoint.z > pos.z - viewRadius && worldPoint.z < pos.z + viewRadius)
                                        {
                                            visitedArray[indexes[j]] = 1;
                                        }
                                    }
                                }
                                */
                            }
                        }
                    }
                }
            }
            yield return null;
        }
        yield return null;
    }
}

/// <summary>
/// Stores indexes of points inside a source mesh. 
/// </summary>
class KGridNode
{
    private List<int> indexData;


    public KGridNode()
    {
        indexData = new List<int>();
    }

    /// <summary>
    /// Inserts an index into the node.
    /// </summary>
    /// <param name="point">The index to be inserted</param>
    public void Insert(int point)
    {
        indexData.Add(point);
    }

    /// <summary>
    /// Gets all the indexes inside the node.
    /// </summary>
    /// <returns>Index data as an int array</returns>
    public int[] GetData()
    {
        return indexData.ToArray();
    }
}

/// <summary>
/// Sorts and splits up source mesh data into "k-grids". Allows for less points to be iterated through each frame.
/// </summary>
public class KGrid
{
    private KGridNode[,,] data;

    private float minX, maxX, minY, maxY, minZ, maxZ;

    private float rangeX, rangeY, rangeZ;

    /// <summary>
    /// Constructor to convert points into k-grids.
    /// </summary>
    /// <param name="points"></param>
    /// <param name="k"></param>
    public KGrid(Vector3[] points, int k)
    {
        //Get the smallest and largest x, y, z values
        minX = points[0].x;
        maxX = points[0].x;
        minY = points[0].y; 
        maxY = points[0].y;
        minZ = points[0].z;
        maxZ = points[0].z;

        //Find the smallest & largest x, y, z values in the point set
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].x < minX)
                minX = points[i].x;
            if (points[i].y < minY)
                minY = points[i].y;
            if (points[i].z < minZ)
                minZ = points[i].z;

            if (points[i].x > maxX)
                maxX = points[i].x;
            if (points[i].y > maxY)
                maxY = points[i].y;
            if (points[i].z > maxZ)
                maxZ = points[i].z;
        }

        //Cut up the points into k sections
        rangeX = Mathf.Abs((maxX - minX) / k);
        //rangeY = Mathf.Abs((maxY - minY) / k);
        rangeZ = Mathf.Abs((maxZ - minZ) / k);

        //Create a new node array of size k+1 - make Y 1 as we dont want to check height of points in area
        data = new KGridNode[k + 1, 1, k + 1];

        //Populate the data - terribly inefficient maybe look at another way? At least we only have to do this once?
        //Possibly useless this bit?
        for(int i = 0; i <= k; i++)
        {
            //In theory this loop COULD be removed as it will only run once. however, this wil be kept in to showcase that the Y-axis can also be split
            for (int ii = 0; ii < 1; ii++)
            {
                for (int iii = 0; iii <= k; iii++)
                {
                    data[i, ii, iii] = new KGridNode();
                }
            }
        }

        //Debugging
        /*
        Debug.Log(maxX);
        Debug.Log(maxY);
        Debug.Log(maxZ);
        Debug.Log(minX);
        Debug.Log(minY);
        Debug.Log(minZ);
        Debug.Log(k);
        Debug.Log(rangeX);
        Debug.Log(rangeY);
        Debug.Log(rangeZ);
        */

        //Iterate through each point in the point set
        for (int i = 0; i < points.Length; i++)
        {
            //Calculate the right index from the points x,y,z data
            GetIndex(points[i], out int indexX, out int indexY, out int indexZ);

            /*
            Debug.Log(indexX);
            Debug.Log(indexY);
            Debug.Log(indexZ);
            */

            //Insert the index of the point into the node
            data[indexX, indexY, indexZ].Insert(i);
        }
    }

    /// <summary>
    /// Returns the indexes inside the grid coordinates given
    /// </summary>
    /// <param name="groupX"></param>
    /// <param name="groupY"></param>
    /// <param name="groupZ"></param>
    /// <returns></returns>
    public int[] GetIndexes(int groupX, int groupY, int groupZ)
    {
        return data[groupX, groupY, groupZ].GetData();
    }

    /// <summary>
    /// Calculate the index based off point positional data.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="indexX"></param>
    /// <param name="indexY"></param>
    /// <param name="indexZ"></param>
    public void GetIndex(Vector3 point, out int indexX, out int indexY, out int indexZ)
    {
        indexX = Mathf.FloorToInt(Mathf.Abs((point.x - minX) / rangeX));
        indexY = 0; //If we want to include Y we just need to change this to match the x and z.
        indexZ = Mathf.FloorToInt(Mathf.Abs((point.z - minZ) / rangeZ));
    }
}
