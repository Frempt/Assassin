using UnityEngine;
using System.Collections;

public class TeleportNodeScript : MonoBehaviour 
{
    public enum NodeColour { GREEN = 0, RED = 1, BLUE = 2 };
    public NodeColour colour;

    private float spinRate;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerScript>().proxyTeleporter = gameObject;
            other.GetComponent<PlayerScript>().atTeleporter = true;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            other.GetComponent<PlayerScript>().proxyTeleporter = gameObject;
            other.GetComponent<PlayerScript>().atTeleporter = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player.proxyTeleporter = gameObject)
            {
                player.atTeleporter = false;
                player.proxyTeleporter = null;
            }
        }
    }
    
	// Use this for initialization
	void Start () 
    {
        spinRate = Random.Range(15.0f, 30.0f);
	}
	
	// Update is called once per frame
	void Update () 
    {
        transform.Rotate(new Vector3(0.0f, 0.0f, -spinRate * Time.deltaTime));
	}
}
