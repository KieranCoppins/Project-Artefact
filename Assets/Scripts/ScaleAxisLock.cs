using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAxisLock : MonoBehaviour
{
    [SerializeField]
    private bool lockX = false;

    [SerializeField]
    private bool lockY = false;

    [SerializeField]
    private bool lockZ = false;

    private Vector3 originalScale;

    private void Start()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        float x = transform.localScale.x;
        float y = transform.localScale.y;
        float z = transform.localScale.z;

        if (lockX)
        {
            x = originalScale.x;
        }
        if (lockY)
        {
            y = originalScale.y;
        }
        if (lockZ)
        {
            z = originalScale.z;
        }

        transform.localScale = new Vector3(x, y, z);
    }
}
