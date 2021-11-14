using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextLogControl : MonoBehaviour
{
    [SerializeField]
    private GameObject textTemplate;

    public List<GameObject> textItems = new List<GameObject>();

    //to compare only in client commands
    public List<string> secondaryList = new List<string>();

    public void LogText(string  newTextString,Color newColor)
    {

        GameObject newText = Instantiate(textTemplate) as GameObject;
        newText.SetActive(true);

        newText.GetComponent<TextLogItem>().SetText(newTextString, newColor);
        newText.transform.SetParent(textTemplate.transform.parent, false);

        textItems.Add(newText.gameObject);
        secondaryList.Add(newText.GetComponent<Text>().text);
    }

    public void DeleteItem(string name)
    {

        try
        {
            secondaryList.Remove(name);
        }
        catch(SystemException e)
        {
            Debug.Log(e);
        }

        GameObject itemtoremove = null;
        foreach(GameObject g in textItems)
        {
            if (g.GetComponent<Text>().text == name)
            {
                itemtoremove = g;
                break;
            }

        }

        if (itemtoremove != null)
        {
            textItems.Remove(itemtoremove);
            Destroy(itemtoremove);
        }

    }


    public void ChangeLogName(string new_name,string old_name)
    {
        //string[] words = old_name.Split(':');

        foreach(GameObject g in textItems)
        {
            if(g.GetComponent<Text>().text.StartsWith(old_name))
            {
                string[] words =  g.GetComponent<Text>().text.Split(':');
                string new_string = new_name + words[1];
                g.GetComponent<Text>().text = new_string;

            }
        }
    }

    public void ReplaceItem(string new_name, string old_name)
    {
        //Check if the name already exists
        foreach(GameObject g in textItems)
        {
            if(g.GetComponent<Text>().text == new_name)
            {
                return;
            }
        }

        //replace names
        foreach (GameObject g in textItems)
        {
            if (g.GetComponent<Text>().text == old_name)
            {
                g.GetComponent<Text>().text = new_name;
                break;
            }
        }

        //For serialize
        for (int i = 0; i < secondaryList.Count; i++)
        {
            if (secondaryList[i] == old_name)
            {
                secondaryList[i] = new_name;
            }
        }
    }
}
