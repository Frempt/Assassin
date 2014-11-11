using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        //find colliders at the screen edges
        Collider2D[] leftHit = Physics2D.OverlapPointAll(camera.ScreenToWorldPoint(new Vector3(0.0f, Screen.height/2, 0.0f)));
        Collider2D[] rightHit = Physics2D.OverlapPointAll(camera.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height/2, 0.0f)));

        bool leftFound = false;
        bool rightFound = false;

        //if any of the colliders on the left edge are the left boundary, we've hit the left edge
        foreach (Collider2D collider in leftHit)
        {
            if (collider.tag == "Left")
            {
                leftFound = true;
                break;
            }
        }

        //if any of the colliders on the right edge are the right boundary, we've hit the right edge
        foreach (Collider2D collider in rightHit)
        {
            if (collider.tag == "Right")
            {
                rightFound = true;
                break;
            }
        }

        if (!leftFound || !rightFound)
        {
            camera.orthographicSize++;
        }
	}
}
