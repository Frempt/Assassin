using UnityEngine;
using System.Collections;

public class BulletScript : MonoBehaviour 
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
        {
            collision.gameObject.GetComponent<PlayerScript>().Die();
        }

        if (collision.collider.tag == "Objective")
        {
            collision.collider.GetComponent<ObjectiveScript>().Seeded();
        }

        Die();
    }

    public void Die()
    {
        Destroy(gameObject);
    }

	// Use this for initialization
	void Start () 
    {
        Invoke("Die", 10.0f);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
