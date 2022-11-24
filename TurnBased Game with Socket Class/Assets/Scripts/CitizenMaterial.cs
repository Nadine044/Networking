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
        //llamar funci�n aqu� que asigne el render con el nombre del material
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AssignMaterial(int material_n)
    {
        foreach(var Item in materialList)
        {
            if (Item.name == player.player_cards.playableCitizenList.citizens[material_n].citizen)
            {
                rend.sharedMaterial = Item;
                Debug.Log(Item.name);
            }
        }
    }
}
