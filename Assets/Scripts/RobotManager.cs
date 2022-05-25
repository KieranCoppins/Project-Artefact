///
/// Code by Kieran Coppins
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;

public class RobotManager : MonoBehaviour
{
    [SerializeField]
    private GazeProvider gazeProvider;

    [SerializeField]
    private GameObject environment;

    public GameObject[] Robots { get; private set; }

    private PointerHandler pointerHandler;

    private Robot selectedRobot;


    private Vector3 previousScale;
    private bool immersionToggle = true;

    // Start is called before the first frame update
    void Start()
    {
        Robots = GameObject.FindGameObjectsWithTag("Robot");
        selectedRobot = Robots[0].GetComponent<Robot>();
        selectedRobot.SetSelected();
        pointerHandler = gameObject.AddComponent<PointerHandler>();
        pointerHandler.OnPointerClicked.AddListener((evt) => CastRay());

        CoreServices.InputSystem.RegisterHandler<IMixedRealityPointerHandler>(pointerHandler);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("MOUSE DOWN");
        }
    }

    public void CastRay()
    {
        Debug.Log("CastRay()");
        if (gazeProvider.HitInfo.collider.tag == "Environment")
            selectedRobot.SetTarget(gazeProvider.HitPosition);
        else if (gazeProvider.HitInfo.collider.tag == "Robot")
        {
            selectedRobot.UnSelect();
            selectedRobot = gazeProvider.HitInfo.collider.GetComponent<Robot>();
            selectedRobot.SetSelected();

        }
    }

    public void SnapToRobot()
    {
        Vector3 newPos = environment.transform.position - (selectedRobot.transform.localPosition * environment.transform.localScale.x);
        environment.transform.position = new Vector3(newPos.x, environment.transform.position.y, newPos.z);
    }

    public void SnapToWaypoint()
    {
        Vector3 newPos = environment.transform.position - (selectedRobot.waypointObject.transform.localPosition * environment.transform.localScale.x);
        environment.transform.position = new Vector3(newPos.x, environment.transform.position.y, newPos.z);
    }

    public void ImmerseToRobot()
    {
        if (immersionToggle)
        {
            previousScale = environment.transform.localScale;
            environment.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            SnapToRobot();
            Camera.main.cullingMask |=  1 << LayerMask.NameToLayer("Real Environment");
            immersionToggle = false;
        }
        else
        {
            environment.transform.localScale = previousScale;
            Camera.main.cullingMask &= ~(1 << LayerMask.NameToLayer("Real Environment"));
            SnapToRobot();
            immersionToggle = true;
        }
    }
}
