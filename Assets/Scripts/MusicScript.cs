using UnityEngine;
using System.Collections;

public class MusicScript : MonoBehaviour 
{
    public static MusicScript instance;

    public AudioSource enemyStem;

    public float enemyDistance = 100;
    public float maximumEnemyDistance = 10;

	// Use this for initialization
	void Start () 
    {
        //setup the static instance
        instance = this;

        //get the enemy audio source
        enemyStem = GetComponents<AudioSource>()[1];
        enemyStem.volume = 0.0f;
        enemyStem.Stop();
	}
	
	// Update is called once per frame
	void Update () 
    {
        //if the enemy is close enough to the player, fade in the enemy stem
        if (enemyDistance < maximumEnemyDistance)
        {
            float factor = 1 - (enemyDistance / maximumEnemyDistance);
            enemyStem.volume = factor;
            enemyStem.time = audio.time;
            enemyStem.Play();
        }
        else
        {
            enemyStem.volume = 0.0f;
            enemyStem.Stop();
        }
	}
}
