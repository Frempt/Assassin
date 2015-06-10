using UnityEngine;
using System.Collections;

public class LeverScript : MonoBehaviour 
{
    public GameObject pair;

    private Animator animator;

	// Use this for initialization
	void Start () 
    {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void Switch()
    {
        if (collider2D.enabled)
        {
			GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().ProxyObjectExit(gameObject);

            collider2D.enabled = false;
            animator.SetBool("isSwitched", true);
            pair.GetComponent<BlastDoorScript>().Open();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();

			player.SelectNewProxyObject(gameObject);
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();

			player.SelectNewProxyObject(gameObject);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player.proxyObject == gameObject)
            {
				player.ProxyObjectExit(gameObject);
            }
        }
    }
}
