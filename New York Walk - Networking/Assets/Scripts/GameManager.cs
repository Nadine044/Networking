using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

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
    // Start is called before the first frame update
    void Start()
    {
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

}
