using UnityEngine;
using System.Collections;

public class BlastDoorScript : MonoBehaviour 
{
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

    public void Open()
    {
        if (collider2D.enabled)
        {
            collider2D.enabled = false;
            animator.SetBool("isOpening", true);
        }
    }
}
