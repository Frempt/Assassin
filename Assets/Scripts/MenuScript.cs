using UnityEngine;
using System.Collections;

public class MenuScript : MonoBehaviour 
{
    public GUISkin skin;

    public GameObject cursorPrefab;

    private MenuCountryScript.CountryName selectedCountry = MenuCountryScript.CountryName.NONE;
    private Vector3 previousMousePosition = Vector2.zero;

    private bool showLevelWindow = false;

	// Use this for initialization
	void Start () 
    {
	
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

                    //if the mouse has been clicked
                    if (Input.GetMouseButtonDown(0))
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
                    //if no country is selected, remove the cursor and set the selected country to none
                    CursorScript.RemoveCursors();
                    selectedCountry = MenuCountryScript.CountryName.NONE;
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
            Rect windowRect = (new Rect(0, 0, Screen.width, Screen.height));
            GUI.Window(0, windowRect, LevelWindow, "Levels in " + selectedCountry);
        }

        /*if(GUI.Button(new Rect(0, 0, Screen.width / 3, Screen.height), "Level 1"))
        {
            Application.LoadLevel("Level01");
        }
        if (GUI.Button(new Rect(Screen.width / 3, 0, Screen.width / 3, Screen.height), "Level 2"))
        {
            Application.LoadLevel("Level02");
        }
        if (GUI.Button(new Rect((Screen.width / 3)*2, 0, Screen.width / 3, Screen.height), "Level 3"))
        {
            Application.LoadLevel("Level03");
        }*/
    }

    void LevelWindow(int windowID)
    {
        //TODO: Set levels based on selected country
        string[] str = new string[3];
        str[0] = "Level 01";
        str[1] = "Level 02";
        str[2] = "Level 03";

        //draw a selection grid and get the selected item
        int selection = GUI.SelectionGrid(new Rect(0, 0, Screen.width, Screen.height), 0, str, str.Length);
    }
}
