using UnityEngine;
using System.Collections;

public class GameControllerScript : MonoBehaviour 
{
    public static GameControllerScript instance;

    public GUISkin skin;
    public Rect windowRect;

    public GameObject playerPrefab;

    private GameObject player;
    private GameObject[] enemies;

    private Vector3 playerStartPosition = Vector3.zero;

    public bool levelComplete;

    private float elapsedTime = 0.0f;
    private int deaths = 0;
    private int score = 5000;

	public MenuCountryScript.CountryName country;

	// Use this for initialization
	void Start () 
    {
        instance = this;

        windowRect = new Rect(Screen.width/2, Screen.height/2, Screen.width/6, Screen.height/6);
        windowRect.x -= windowRect.width / 2;
        windowRect.y -= windowRect.height / 2;

        player = GameObject.FindGameObjectWithTag("Player");
        playerStartPosition = player.transform.position;

        enemies = GameObject.FindGameObjectsWithTag("Enemy");
	}
	
	// Update is called once per frame
	void Update () 
    {
        //allow the game to exit
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();

        //update the music
        UpdateMusic();

        //calculate the score
        score = 5000;
        score -= (deaths * 100);
        score -= (int)(elapsedTime * (10 * (1 + deaths)));
        if (score < 0) score = 0;

        //if the level is complete, pressing action or jump returns to the menu
        if (levelComplete)
        {
            if (Input.GetButtonDown("Action") || (Input.GetButtonDown("Jump")))
            {
                Application.LoadLevel(0);
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

        if (levelComplete)
        {
            //display the level complete GUI
            GUI.Window(0, windowRect, DoMyWindow, "Level Complete");
        }
    }

    public void DoMyWindow(int windowId)
    {
        GUIStyle style = new GUIStyle();
        Vector2 size = style.CalcSize(new GUIContent("You scored: " + score));
        size.x *= 1.7f;
        size.y *= 2;
        GUI.Label(new Rect((windowRect.width/2) - (size.x / 2), (windowRect.height/3) - (size.y / 2), size.x, size.y), "You scored: " + score);
        
        if (GUI.Button(new Rect(0, windowRect.height/2, windowRect.width, windowRect.height/2), "Back to Menu"))
        {
            Application.LoadLevel(0);
        }
    }

    public void LevelComplete()
    {
        //set the level to complete
        levelComplete = true;
        player.rigidbody2D.velocity = Vector2.zero;
		player.GetComponent<Animator>().SetBool("isMoving", false);

		//remove any bullets in the level
		GameObject[] bullets = GameObject.FindGameObjectsWithTag("Bullet");

		for(int i = 0; i < bullets.Length; i++)
		{
			Destroy (bullets[i]);
		}

		SaveFileController save = new SaveFileController ();
		save.SetLevelComplete (country, Application.loadedLevelName, score);
    }

    public void ResetLevel()
    {
        //reset the level
        Destroy(GameObject.FindGameObjectWithTag("Player"));
        player = (GameObject)GameObject.Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
		CursorScript.RemoveCursors();

        //increment the deaths
        deaths++;
    }

    //update the music
    public void UpdateMusic()
    {
        //create a distance with the maximum distance
        float distance = MusicScript.instance.maximumEnemyDistance;

        //check if any enemies are closer
        foreach (GameObject enemy in enemies)
        {
            Vector2 thisDistance = enemy.transform.position - player.transform.position;

            if (thisDistance.magnitude < distance) distance = thisDistance.magnitude;
        }

        //set the distance to that of the closest enemy
        MusicScript.instance.enemyDistance = distance;
    }
}
