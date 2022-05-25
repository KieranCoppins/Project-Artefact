///
/// Code by Kieran Coppins
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text mapDiscovered;

    [SerializeField]
    private TMP_Text time;

    private float timer = 0f;
    private FogOfWarMesh fogOfWarMesh;

    private bool complete = false;

    private RobotManager robotManager;

    // Start is called before the first frame update
    void Start()
    {
        fogOfWarMesh = GameObject.FindGameObjectWithTag("Environment").GetComponent<FogOfWarMesh>();
        robotManager = GameObject.FindGameObjectWithTag("Robot Manager").GetComponent<RobotManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!complete)
        {
            timer += Time.deltaTime;
        }
        float percent = Mathf.Round(fogOfWarMesh.visitedCount / fogOfWarMesh.vertexCount * 100f);
        mapDiscovered.text = "Map Discovered: \n" + percent + "%";
        if (percent >= 90)
        {
            complete = true;
        }
        int mins = Mathf.FloorToInt(timer / 60);
        int secs = Mathf.FloorToInt(timer) % 60;
        time.text = "Timer:\n" + mins.ToString("00") + ":" + secs.ToString("00");
    }

    public void GoToRobot()
    {
        robotManager.SnapToRobot();
    }

    public void GoToWaypoint()
    {
        robotManager.SnapToWaypoint();
    }

    public void Immerse()
    {
        robotManager.ImmerseToRobot();
    }
}
