///
/// Code By Ahmaderfani12
/// GitHub: https://github.com/ahmaderfani12/PointClouds
/// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{


    [SerializeField]
    private GameObject viewportPlacer;

    [SerializeField]
    private Transform viewportPlacerSpawnpoint;

    [SerializeField]
    private GameObject viewport;

    [SerializeField]
    private GameObject environment;

    [SerializeField]
    private GameObject menu;

    private GameObject pViewportPlacer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Button event to place the viewport placer that the user can move, rotate and scale to place the environment
    /// </summary>
    public void placeViewportPlacer()
    {
        if (pViewportPlacer == null)
        {
            pViewportPlacer = Instantiate(viewportPlacer, viewportPlacerSpawnpoint.position, Quaternion.identity);
        }
        else
        {
            pViewportPlacer.transform.position = viewportPlacerSpawnpoint.position;
        }
    }

    /// <summary>
    /// Button event to place the viewport and environment where the viewport placer is located
    /// </summary>
    public void placeViewport()
    {
        //Check that we have the viewport placer in the world
        if (pViewportPlacer != null)
        {
            //Set the position of the viewport to the placer
            viewport.transform.position = pViewportPlacer.transform.position;

            //Set the same scale as the placer - however just set the y to 100 (render everything in the y)
            viewport.transform.localScale = new Vector3(pViewportPlacer.transform.localScale.x, 
                100.0f,
                pViewportPlacer.transform.localScale.z);

            //Get the environment spawn location - which is the viewport position but half the y to place on the floor of the object
            Vector3 environmentSpawn = new Vector3(viewport.transform.position.x,
                viewport.transform.position.y - (pViewportPlacer.transform.localScale.y / 2),
                viewport.transform.position.z);
            environment.transform.position = environmentSpawn;

            //Calculate the scale of the environtment to fit inside the viewport
            float scale = viewport.transform.localScale.x / 100f;
            environment.transform.localScale = new Vector3(scale, scale, scale);

            //Create an instance of the new menu which shows the time, % map explored etc.
            Instantiate(menu, transform.position, Quaternion.identity);

            //Destroy the placer and the this menu - we won't be needing those anymore
            Destroy(pViewportPlacer);
            Destroy(this.gameObject);
        }
    }
}
