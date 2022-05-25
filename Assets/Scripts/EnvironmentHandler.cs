///
/// Code by Kieran Coppins
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnvironmentHandler : MonoBehaviour
{
    private GameObject[] robots;
    private List<NavMeshAgent> agents = new List<NavMeshAgent> ();

    private Vector3 prevPos;
    private Vector3 currPos;

    private float prevAngle;
    private float currAngle;

    private Vector3 prevScale;
    private Vector3 currScale;

    private NavMeshSurface nms;

    [SerializeField]
    private float agentSpeed = 1.5f;
    

    private void Start()
    {
        currPos = transform.position;
        currAngle = transform.rotation.eulerAngles.y;
        currScale = transform.localScale;
        nms = GetComponent<NavMeshSurface>();
        nms.BuildNavMesh();
        StartCoroutine(MoveHandler());
        robots = GameObject.FindGameObjectsWithTag("Robot");
        for (int i = 0; i < robots.Length; i++)
        {
            agents.Add(robots[i].GetComponent<NavMeshAgent>());
        }
        for (int i = 0; i < agents.Count; i++)
        {
            agents[i].speed = agentSpeed * transform.localScale.x;
        }
    }

    private void Update()
    {

    }

    private IEnumerator MoveHandler()
    {
        while (true)
        {
            Translate();

            yield return null;
        }
    }

    void Translate()
    {
        prevPos = currPos;
        currPos = transform.position;
        prevAngle = currAngle;
        currAngle = transform.rotation.eulerAngles.y;

        //Get the angle which the environment has changed
        float angle = currAngle - prevAngle;

        //Calculate any difference between scale
        prevScale = currScale;
        currScale = transform.localScale;
        float scale = Vector3.Distance(currScale, prevScale);

        Vector3 direction = (currPos - prevPos).normalized;
        float distance = Vector3.Distance(prevPos, currPos);

        //If we have moved the environment
        if (distance > 0 || angle != 0 || scale != 0)
        {
            //Only rebuild nav mesh if the scale has changed
            if (scale != 0)
            {
                //Rebuild the nav mesh
                nms.BuildNavMesh();
            }
            //We need to only do this when moving the environment on desktop, not in Hololens?
            /*
            for (int i = 0; i < agents.Count; i++)
            {
                Vector3 rotation = Quaternion.Euler(0, angle, 0) * (agents[i].transform.position - transform.position) + transform.position;

                agents[i].transform.rotation = Quaternion.Euler(agents[i].transform.rotation.eulerAngles.x,
                    agents[i].transform.rotation.eulerAngles.y + angle,
                    agents[i].transform.rotation.eulerAngles.z);

                agents[i].Warp(rotation + direction * distance);
            }
            */
        }
    }

    public void SnapAgents()
    {
        for (int i = 0; i < agents.Count; i++)
        {
            //Teleporting agent
            agents[i].GetComponent<Robot>().TeleportToTarget();
        }
    }
}
