///
/// Code by Kieran Coppins
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Robot : MonoBehaviour
{
    [SerializeField]
    private Material standardMaterial;
    
    [SerializeField]
    private Material selectedMaterial;

    [SerializeField]
    public MeshRenderer[] meshRenderers { get; private set; }

    [SerializeField]
    private bool rotateFirst;

    [SerializeField]
    public GameObject waypointObject;

    [SerializeField]
    private GameObject camera;

    [SerializeField]
    private GameObject cameraViewport;

    private NavMeshAgent agent;

    private float standardSpeed;

    private MeshRenderer[] waypointMeshRenderers;

    private Vector3 lastWaypointPosition;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        waypointObject.name = transform.name + " target";
        waypointObject.transform.position = transform.position;
        waypointObject.transform.parent = GameObject.FindGameObjectWithTag("Environment").transform;
        waypointMeshRenderers = waypointObject.GetComponentsInChildren<MeshRenderer>();
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        standardSpeed = agent.speed;
        UnSelect();
        //target = Instantiate(targetObject, transform.position, Quaternion.identity, GameObject.FindGameObjectWithTag("Environment").transform);
    }

    // Update is called once per frame
    void Update()
    {
        FollowTarget();
        if (rotateFirst)
        {
            SteerToTarget();
        }
        if (!agent.isOnNavMesh)
        {
            TeleportToTarget();
        }
        lastWaypointPosition = waypointObject.transform.position;
    }

    public void SetSelected()
    {
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.material = selectedMaterial;
        }
        foreach(MeshRenderer mr in waypointMeshRenderers)
        {
            mr.enabled = true;
        }
        camera.SetActive(true);
        cameraViewport.SetActive(true);
    }

    public void UnSelect()
    {
        foreach (MeshRenderer mr in meshRenderers)
        {
            mr.material = standardMaterial;
        }
        foreach (MeshRenderer mr in waypointMeshRenderers)
        {
            mr.enabled = false;
        }
        camera.SetActive(false);
        cameraViewport.SetActive(false);
    }

    void FollowTarget()
    {
        if (waypointObject == null)
        {
            return;
        }
        if (lastWaypointPosition != waypointObject.transform.position)
            agent.SetDestination(waypointObject.transform.position);
    }

    void SteerToTarget()
    {
        //Check if we're at our target - prevents spinning on location
        if (agent.remainingDistance < 0.01f)
            return;

        //Ignore y height
        Vector3 posA = new Vector3(agent.steeringTarget.x, 0.0f, agent.steeringTarget.z);
        Vector3 posB = new Vector3(transform.position.x, 0.0f, transform.position.z);

        //Get direction between the next point and the current position
        Vector3 direction = (posA - posB).normalized;

        //Return the dot product
        float dotProduct = Vector3.Dot(direction, transform.forward);

        //If we're facing our target
        if (dotProduct > 0.95)
        {
            //Set our speed to normal
            agent.speed = standardSpeed;
        }
        else
        {
            //Dont move
            agent.speed = 0;
            //Get rotation to face the point
            Quaternion rotation = Quaternion.LookRotation(direction);

            //Slerp to face rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * (agent.angularSpeed * standardSpeed));

        }
    }

    public void SetTarget(Vector3 v)
    {
        waypointObject.transform.position = v;
    }

    public void TeleportToTarget()
    {
        agent.Warp(waypointObject.transform.position);
    }
}
