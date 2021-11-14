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

    //to compare only
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
}
