using UnityEngine;
using System.Collections;

public class ObjectiveScript : EnemyScript
{
	// Use this for initialization
	new void Start () 
    {
        base.Start();
	}
	
	// Update is called once per frame
	new void Update () 
    {
        base.Update();
	}

    public void Seeded()
    {
        animator.SetBool("isSeeded", true);
        state = EnemyState.ROOTED;
        GameControllerScript.instance.LevelComplete();
    }
}
