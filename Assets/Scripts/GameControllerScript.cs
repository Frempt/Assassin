using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour 
{
    public static GameControllerScript instance;

    public GUISkin skin;

    private GameObject player;
    private GameObject[] enemies;

    private bool levelComplete;

    private float elapsedTime = 0.0f;
    private int deaths = 0;

	// Use this for initialization
	void Start () 
    {
        instance = this;

        GameObject tempPlayer = GameObject.FindGameObjectWithTag("Player");

        player = tempPlayer;

        GameObject[] tempEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        enemies = tempEnemies;
	}
	
	// Update is called once per frame
	void Update () 
    {
        //allow the game to exit
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        //if the level is complete, pressing action or jump resets it
        if (levelComplete)
        {
            if (Input.GetButtonDown("Action") || (Input.GetButtonDown("Jump")))
            {
                ResetLevel();
            }
        }
        else
        {
            //increment the elapsed time if the level is not yet complete
            elapsedTime += Time.deltaTime;
        }
	}

    void OnGUI()
    {
        GUI.skin = skin;

        //display the victory message
        //TODO: calculate score based on time and number of deaths
        if (levelComplete)
        {
            GUI.Label(new Rect(Screen.width / 2, Screen.height / 2, 1000, 300), "Victory! Time = " + elapsedTime);
        }
    }

    public void LevelComplete()
    {
        //remove the enemy objects from play
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        //remove the player object from play
        Destroy(player);

        //set the level to complete
        levelComplete = true;
    }

    public void ResetLevel()
    {
        //reset the level
        //TODO: reset enemy and player objects but keep time and number of deaths recorded

        Application.LoadLevel(Application.loadedLevel);
        
        /*GameObject tempPlayer = GameObject.FindGameObjectWithTag("Player");

        Destroy(tempPlayer);

        GameObject[] tempEnemies = GameObject.FindGameObjectsWithTag("Enemy");

        for (int i = 0; i < tempEnemies.Length; i++)
        {
            Destroy(tempEnemies[i]);
        }

        GameObject.Instantiate(player);

        foreach (GameObject enemy in enemies)
        {
            GameObject.Instantiate(enemy);
        }

        deaths++;*/
    }
}
