using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour 
{
    public GUISkin skin;

    public GameObject cursorPrefab;

	private Rect windowRect = (new Rect(Screen.width/4, Screen.height/4, Screen.width/2, Screen.height/2));

    private MenuCountryScript.CountryName selectedCountry = MenuCountryScript.CountryName.NONE;
    private Vector3 previousMousePosition = Vector2.zero;

    private bool showLevelWindow = false;

	private SaveFileController save;

	// Use this for initialization
	void Start () 
    {
		save = new SaveFileController ();

		//get the progress the player has made in the USA level group
		float usaProgress = save.GetProgress (MenuCountryScript.CountryName.USA);

		GameObject usaObj = GameObject.Find ("MenuMapUSA");
		usaObj.renderer.material.color = new Color ((255 * usaProgress), (255 * (1.0f - usaProgress)), 0.0f);

		//get the progress the player has made in the Russia level group
		float russiaProgress = save.GetProgress (MenuCountryScript.CountryName.RUSSIA);
		
		GameObject russiaObj = GameObject.Find ("MenuMapRussia");
		russiaObj.renderer.material.color = new Color ((255 * russiaProgress), (255 * (1.0f - russiaProgress)), 0.0f);
	}
	
	// Update is called once per frame
	void Update () 
    {
        //if the level selection popup is not shown
        if (!showLevelWindow)
        {
            //check if the mouse has been moved
            bool mouseMoved = (previousMousePosition != Input.mousePosition);

            //if the mouse has been moved
            if (mouseMoved)
            {
                bool selectedChanged = false;

                //get the colliders at the mouse
                Collider2D[] points = Physics2D.OverlapPointAll(Camera.main.ScreenToWorldPoint(Input.mousePosition));

                //iterate through the colliders
                Collider2D country = null;
                foreach (Collider2D point in points)
                {
                    //if the collider is a country
                    if (point.tag == "Country")
                    {
                        //if this country is different from the selected one, select that country
                        country = point;
                        if (point.GetComponent<MenuCountryScript>().countryName != selectedCountry)
                        {
                            selectedCountry = point.GetComponent<MenuCountryScript>().countryName;
                            selectedChanged = true;
                        }

                        //break out of the loop
                        break;
                    }
                }

                //if a country is selected
                if (country)
                {
                    MenuCountryScript countryScript = country.gameObject.GetComponent<MenuCountryScript>();

                    //if the selection has changed, create a new cursor
                    if (selectedChanged)
                    {
                        CursorScript.CreateNewCursor(countryScript.cursorPoint, cursorPrefab);
                        selectedChanged = false;
                    }
                }
                else
                {
                    //if no country is selected, remove the cursor and set the selected country to none
                    CursorScript.RemoveCursors();
                    selectedCountry = MenuCountryScript.CountryName.NONE;
                }
            }

			//if the mouse has been clicked
			if (Input.GetMouseButtonDown(0) && selectedCountry != MenuCountryScript.CountryName.NONE)
			{
				//open the level selection pop up
				switch (selectedCountry)
				{
				case MenuCountryScript.CountryName.USA:
					showLevelWindow = true;
					break;
					
				case MenuCountryScript.CountryName.RUSSIA:
					showLevelWindow = true;
					break;
					
				default:
					break;
				}
			}
        }
		else
		{
			if(Input.GetMouseButtonDown(0))
			{
				if(Input.mousePosition.x > windowRect.x + windowRect.size.x
				    || Input.mousePosition.x < windowRect.x
				    || Input.mousePosition.y > windowRect.y + windowRect.size.y
				    || Input.mousePosition.y < windowRect.y)
				{
					showLevelWindow = false;
				}
 			}
		}

        //set the previous mouse position
        previousMousePosition = Input.mousePosition;
	}

    void OnGUI()
    {
        GUI.skin = skin;

        //if the level selection pop up is to be shown
        if (showLevelWindow)
        {
            //draw the window
            GUI.Window(0, windowRect, LevelWindow, "Levels in " + selectedCountry);
        }
    }

    void LevelWindow(int windowID)
    {
		//get the save file
		//SaveFileController save = new SaveFileController ();

		//column positions for the level grid
		float column1Position = windowRect.width / 30;
		float column2Position = windowRect.width / 3;
		float column3Position = (windowRect.width / 3) * 2;

		//space between each row for the level grid
		float rowSpacing = windowRect.height / 10;

		//draw the level grid
		for(int i = 1; i <= save.GetNumberOfLevels(selectedCountry); i++)
		{
			float rowPosition = rowSpacing * i;

			if (GUI.Button (new Rect (column1Position, rowPosition, windowRect.width / 5, windowRect.height / 10), "Level " + i)) 
			{
				Application.LoadLevel (selectedCountry.ToString() + i);
			}
			
			GUI.Label (new Rect (column2Position, rowPosition, 400, 100), (save.IsLevelComplete(selectedCountry, selectedCountry.ToString() + i) ? "Complete" : "Incomplete"));
			
			GUI.Label (new Rect (column3Position, rowPosition, 400, 100), "Highscore: " + save.GetHighScore(selectedCountry, selectedCountry.ToString() + i));
		}
    }
}