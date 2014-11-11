using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;

class SaveFileController
{
    private XmlDocument saveFile;
    private string savePath = "save.sav";

    public SaveFileController() 
    {
        saveFile = new XmlDocument();

        try
        {
            //try loading the save file
            saveFile.Load(savePath);
        }
        catch (FileNotFoundException e)
        {
            //if the save file wasn't found, create a new one
            XmlElement rootNode = saveFile.CreateElement("Levels");
            saveFile.AppendChild(rootNode);

            //add the USA levels
            XmlElement usaNode = saveFile.CreateElement("USA");
            AddLevelNode(usaNode, "USA1");
            AddLevelNode(usaNode, "USA2");
            AddLevelNode(usaNode, "USA3");

            saveFile.DocumentElement.AppendChild(usaNode);
        }
        catch (Exception e)
        {
            //TODO error with save file
            Console.WriteLine("Save failed to load" + e.ToString());
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
        levelNode.SetAttribute("Compete", "True");
    }
}