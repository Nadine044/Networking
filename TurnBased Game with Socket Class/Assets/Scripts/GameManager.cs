using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    //public List<int> randomNumbers;
    public List<CitizenMaterial> player_cards = new List<CitizenMaterial>();

    //we define the board as an array/list of gameobjects 
    public List<GameObject> boardSquares = new List<GameObject>();

    public static GameManager _instance { get; private set; }

    [SerializeField]
    private GameObject restritecSpaceCubePrefab;
    private List<GameObject> restrictedCubesList = new List<GameObject>();

    [SerializeField]
    private GameObject availableCubePrefab;
    private List<GameObject> availableposCubeList = new List<GameObject>();
    // Start is called before the first frame update
    [SerializeField]
    private GameObject wonpanel;
    [SerializeField]
    private GameObject losepanel;
    void Start()
    {
        wonpanel.SetActive(false);
        losepanel.SetActive(false);
        _instance = this;
    }

    public void SetMaterial(int num,int material_n)
    {
        player_cards[num].AssignMaterial(material_n);
    }

    public void SetRestrictedSpaceCubes(int[] restricted_pos)
    {
        for (int i = 0; i < restricted_pos.Length; i++)
        {
            GameObject go = Instantiate(restritecSpaceCubePrefab);
            go.transform.position = boardSquares[restricted_pos[i]].transform.position;
            restrictedCubesList.Add(go);
        }
    }

    public void EraseRestrictedSpaceCubes()
    {
        for(int i =0; i < restrictedCubesList.Count(); i++)
        {
            Destroy(restrictedCubesList[i]);
        }
        restrictedCubesList.Clear();
    }

    public void SetWonPanel()
    {
        wonpanel.SetActive(true);
    }

    public void SetLosePanel()
    {
        losepanel.SetActive(true);
    }

    public void SetAvailableCubes(int currentpos)
    {
        if (currentpos + 5 <= 24) //we are inside of boundaries
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardSquares[currentpos + 5].transform.position;
            availableposCubeList.Add(go);
        }
        if (currentpos - 5 >= 0)
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardSquares[currentpos - 5].transform.position;
            availableposCubeList.Add(go);
        }
        if (currentpos % 5 != 0) //means we aren't on the right side 
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardSquares[currentpos - 1].transform.position; //one to the right
            availableposCubeList.Add(go);
        }
        if ((currentpos + 1) % 5 != 0)//means we are on the left side
        {
            GameObject go = Instantiate(availableCubePrefab);
            go.transform.position = boardSquares[currentpos + 1].transform.position; //one to the left
            availableposCubeList.Add(go);
        }
    }
    public void EraseAvailableSquares()
    {
        for (int i = 0; i < availableposCubeList.Count(); i++)
        {
            Destroy(availableposCubeList[i]);
        }
        availableposCubeList.Clear();
    }
}
