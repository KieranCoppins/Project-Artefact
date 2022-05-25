using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewportHandler : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Collider Triggered Enter");
        Robot robot = other.GetComponent<Robot>();
        if (robot != null)
        {
            Debug.Log("IT WAS A ROBOT");
            for (int i = 0; i < robot.meshRenderers.Length; i++)
            {
                robot.meshRenderers[i].enabled = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Collider Triggered Exit");
        Robot robot = other.GetComponent<Robot>();
        if (robot != null)
        {
            Debug.Log("IT WAS A ROBOT");
            for (int i = 0; i < robot.meshRenderers.Length; i++)
            {
                robot.meshRenderers[i].enabled = false;
            }
        }
    }
}
