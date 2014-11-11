using UnityEngine;
using System.Collections;

public class DoorScript : MonoBehaviour 
{
    public GameObject exit;
    public bool isBlocked;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();

            player.atDoor = true;
            player.proxyDoor = gameObject;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();

            player.atDoor = true;
            player.proxyDoor = gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player.proxyDoor = gameObject)
            {
                player.atDoor = false;
                player.proxyDoor = null;
            }
        }
    }
}
