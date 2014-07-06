using UnityEngine;
using System.Collections;

public class GrabPointScript : MonoBehaviour 
{
    public Transform hangPoint;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            
            player.atGrabPoint = true;
            player.proxyGrabPoint = gameObject;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();

            player.atGrabPoint = true;
            player.proxyGrabPoint = gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player.proxyGrabPoint = gameObject)
            {
                player.atGrabPoint = false;
                player.proxyGrabPoint = null;
            }
        }
    }

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
