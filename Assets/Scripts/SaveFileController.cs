using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using UnityEngine;

class SaveFileController
{
    private XmlDocument saveFile;
	private string savePath = @"C:\Users\Frempt\Dropbox\Projects\Alien Invasion\save.sav";

    public SaveFileController() 
    {
        saveFile = new XmlDocument();

		string saveFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Envious Eyes");
		if(!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
		savePath = Path.Combine(saveFolder, "save.sav");

		try
		{
			if(File.Exists(savePath))
	        {
	            //try loading the save file
	            saveFile.Load(savePath);
	        }
	        else
	        {
	            //if the save file wasn't found, create a new one
	            XmlElement rootNode = saveFile.CreateElement("Levels");
	            saveFile.AppendChild(rootNode);

	            //add the USA levels
	            XmlElement usaNode = saveFile.CreateElement(MenuCountryScript.CountryName.USA.ToString());
	            AddLevelNode(usaNode, "USA1");
				AddLevelNode(usaNode, "USA2");
				AddLevelNode(usaNode, "USA3");
				AddLevelNode(usaNode, "USA4");
				AddLevelNode(usaNode, "USA5");

	            saveFile.DocumentElement.AppendChild(usaNode);

				//add the Russia levels
				XmlElement russiaNode = saveFile.CreateElement(MenuCountryScript.CountryName.RUSSIA.ToString());
				AddLevelNode(russiaNode, "RUSSIA1");
				AddLevelNode(russiaNode, "RUSSIA2");
				AddLevelNode(russiaNode, "RUSSIA3");
				AddLevelNode(russiaNode, "RUSSIA4");
				AddLevelNode(russiaNode, "RUSSIA5");
				
				saveFile.DocumentElement.AppendChild(russiaNode);

				saveFile.Save (savePath);
        	}
		}
        catch (Exception e)
        {
            //TODO error with save file
            Console.WriteLine("Save failed to load " + e.ToString());
        }
    }

    private void AddLevelNode(XmlElement element, string name)
    {
        //create a new level element
        XmlElement level = saveFile.CreateElement(name);

        //add attributes for high score and completed
        XmlAttribute score = saveFile.CreateAttribute("Score");
        XmlAttribute complete = saveFile.CreateAttribute("Complete");
        level.Attributes.Append(score);
        level.Attributes.Append(complete);

        //set default values for the attributes
        level.SetAttribute("Score", "0");
        level.SetAttribute("Complete", "false");

        //add the level as a child to the level group
        element.AppendChild(level);
    }

    private XmlElement GetLevel(MenuCountryScript.CountryName country, string level)
    {
        //iterate through each level group to find the correct country
        foreach (XmlNode node in saveFile.DocumentElement.ChildNodes)
        {
            //if this is the correct country
            if (node.Name == country.ToString())
            {
                //iterate through each level to find the correct one
                foreach (XmlElement levelNode in node.ChildNodes)
                {
                    //if this is the correct level
                    if (levelNode.Name == level)
                    {
                        return levelNode;
                    }
                }
            }
        }

        //return null if the element wasn't found
        return null;
    }

	//get the number of levels in the game
	public int GetNumberOfLevels()
	{
		int returnValue = 0;

		//iterate through each level group to find the correct country
		foreach (XmlNode node in saveFile.DocumentElement.ChildNodes)
		{
			returnValue += node.ChildNodes.Count;
		}

		return returnValue;
	}

	//get the number of levels in a specified country
	public int GetNumberOfLevels(MenuCountryScript.CountryName country)
	{
		//iterate through each level group to find the correct country
		foreach (XmlNode node in saveFile.DocumentElement.ChildNodes)
		{
			//if this is the correct country
			if (node.Name == country.ToString())
			{
				return node.ChildNodes.Count;
			}
		}

		return 0;
	}

	//return a value between 0.0 and 1.0 based on how many levels are complete in a country
	public float GetProgress(MenuCountryScript.CountryName country)
	{
		int completedLevels = 0;

		//iterate through each level group to find the correct country
		foreach (XmlNode node in saveFile.DocumentElement.ChildNodes)
		{
			//if this is the correct country
			if (node.Name == country.ToString())
			{
				foreach(XmlNode level in node.ChildNodes)
				{
					if(IsLevelComplete(country, level.Name)) completedLevels++;
				}
				break;
			}
		}
     
		return ((float)completedLevels / (float)GetNumberOfLevels(country));
	}

    public bool IsLevelComplete(MenuCountryScript.CountryName country, string level)
    {
        //find the correct level
        XmlElement levelNode = GetLevel(country, level);

        //return true if the level is complete
        return (levelNode.GetAttribute("Complete").ToUpper() == "TRUE");
    }

    public int GetHighScore(MenuCountryScript.CountryName country, string level)
    {
        //find the correct level
        XmlElement levelNode = GetLevel(country, level);

        //return the highest score attained on the level
        return Convert.ToInt32(levelNode.GetAttribute("Score"));
    }

    public void SetLevelComplete(MenuCountryScript.CountryName country, string level, int score)
    {
        //find the correct level
        XmlElement levelNode = GetLevel(country, level);

        //if the new score is higher than the old score, replace the old score
        int oldScore = Convert.ToInt32(levelNode.GetAttribute("Score"));
        if (oldScore < score)
        {
            levelNode.SetAttribute("Score", score.ToString());
        }

        //set the level as complete
        levelNode.SetAttribute("Complete", "true");

		//save the file
		saveFile.Save (savePath);
    }
}