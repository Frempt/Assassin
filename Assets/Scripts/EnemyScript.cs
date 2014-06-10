using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour 
{
    public GameObject patrolPoint1;
    public GameObject patrolPoint2;

    public Vector2 velocity;
    public float maxHorizontalSpeed = 5.0f;

    private Vector2 target = Vector3.zero;

    private enum PatrolPoint { PATROLPOINT1 = 0, PATROLPOINT2 = 1 };
    private PatrolPoint patrolPoint = PatrolPoint.PATROLPOINT1;

	// Use this for initialization
	void Start () 
    {
        target = patrolPoint1.transform.position;
	}
	
	// Update is called once per frame
	void Update () 
    {
        //patrol
        target.y = transform.position.y;
        Vector2 direction = target - (Vector2)transform.position;
        direction.y = 0.0f;

        //turn around
        if (direction.magnitude < 1.0f)
        {
            if (patrolPoint == PatrolPoint.PATROLPOINT1)
            {
                target = patrolPoint2.transform.position;
                patrolPoint = PatrolPoint.PATROLPOINT2;
            }
            else if (patrolPoint == PatrolPoint.PATROLPOINT2)
            {
                target = patrolPoint1.transform.position;
                patrolPoint = PatrolPoint.PATROLPOINT1;
            }
        }

        //update the enemy position
        direction.Normalize();
        transform.Translate(direction * maxHorizontalSpeed * Time.deltaTime);
	}
}
