using UnityEngine;
using System.Collections;

public class CursorScript : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(new Vector3(0.0f, 0.0f, 10.0f * Time.deltaTime));
	}

    //create the specified cursor at the specified position
    public static void CreateNewCursor(Vector3 newPosition, GameObject cursorPrefab)
    {
        //check if there is currently a cursor
        GameObject cursorTemp = GameObject.FindGameObjectWithTag("Cursor");

        //if there isn't, create one at the new position
        if (!cursorTemp)
        {
            GameObject.Instantiate(cursorPrefab, newPosition, Quaternion.identity);
        }
        //if there is a cursor, and it has a different location to the new position, destroy it and create a new one
        else if (cursorTemp.transform.position != newPosition)
        {
            Destroy(cursorTemp);

            GameObject.Instantiate(cursorPrefab, newPosition, Quaternion.identity);
        }
    }

    //remove all cursors from the screen
    public static void RemoveCursors()
    {
        //remove the cursor
        GameObject[] cursorTemp = GameObject.FindGameObjectsWithTag("Cursor");

        for (int i = 0; i < cursorTemp.Length; i++)
        {
            Destroy(cursorTemp[i]);
        }
    }
}
