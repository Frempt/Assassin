using UnityEngine;
using System.Collections;

public class MenuCountryScript : MonoBehaviour 
{
    public enum CountryName { NONE = 0, USA, RUSSIA, CHINA, UK };
    public CountryName countryName;
    public Vector3 cursorPoint;

	// Use this for initialization
	void Start () 
    {
        cursorPoint = collider2D.bounds.center;
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
