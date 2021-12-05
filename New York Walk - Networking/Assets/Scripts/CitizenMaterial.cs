using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CitizenMaterial : MonoBehaviour
{
    public Material[] materialList;
    Renderer rend;

    public Player player;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Renderer>();
        rend.enabled = true;
        //llamar función aquí que asigne el render con el nombre del material
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            AssignMaterial();
    }

    public void AssignMaterial()
    {
        foreach(var Item in materialList)
        {
            if (Item.name == player.player_cards.playableCitizenList.citizens[player.randomNumber].citizen)
            {
                rend.sharedMaterial = Item;
                Debug.Log(Item.name);
            }
        }
    }
}
