using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player1;

    //public List<int> randomNumbers;
    public List<CitizenMaterial> player_cards = new List<CitizenMaterial>();

    //we define the board as an array of gameobjects 
    public GameObject[] array_positions;
    public List<GameObject> boardSquares = new List<GameObject>();

    public static GameManager _instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
    }

    public void SetMaterial(int num,int material_n)
    {
        player_cards[num].AssignMaterial(material_n);
    }

    // Update is called once per frame
    void Update()
    {
        //Music plays :D
        //Before enter a game, put your Player Name

        //Start Game UI

        //Click cards to select order 1 2 3 function

        //!!!!  Put citizens on the board (turn based)  !!!!
    }


}
