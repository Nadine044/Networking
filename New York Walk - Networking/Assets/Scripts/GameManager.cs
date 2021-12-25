using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player1;
    //private bool pickUpArrived;
    //private bool destinyArrived;
    //private bool hasObject;

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
        //pickUpArrived = false;
        //destinyArrived = false;
        //hasObject = false;
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



        //CHECK PICK-UP
        //if (!pickUpArrived)
        //{
        //    if (boardSquares[player1.current_board_pos].name == player1.card1.pickUp)
        //    {
        //        Debug.Log("OBJECT PICKED!!");
        //        pickUpArrived = true;
        //        hasObject = true;
        //    }
        //}

        ////CHECK DESTINY
        //if (!destinyArrived && hasObject)
        //{
        //    if (boardSquares[player1.current_board_pos].name == player1.card1.destiny)
        //    {
        //        Debug.Log("DESTINY ARRIVED!!");
        //        destinyArrived = true;
        //    }
        //}

        //Click cards to select order 1 2 3 function

        //!!!!  Put citizens on the board (turn based)  !!!!
    }


}
