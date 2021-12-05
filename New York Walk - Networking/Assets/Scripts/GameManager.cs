using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player1;
    public Player player2;

    public List<int> randomNumbers;
    public List<CitizenMaterial> GerardCitizensMaterialList = new List<CitizenMaterial>();

    //we define the board as an array of gameobjects 
    public GameObject[] array_positions;

    public static GameManager _instance { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        //Music plays :D
        //Before enter a game, put your Player Name

        //Start Game UI

        //Get Cards Function (random 3 cards)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player1.ObtainCitizenCard(player1.card1, randomNumbers);
            GerardCitizensMaterialList[0].AssignMaterial(randomNumbers[0]);
            //player2.ObtainCitizenCard(player2.card1, randomNumbers);

            player1.ObtainCitizenCard(player1.card2, randomNumbers);
            GerardCitizensMaterialList[1].AssignMaterial(randomNumbers[1]);
            //player2.ObtainCitizenCard(player2.card2, randomNumbers);

            player1.ObtainCitizenCard(player1.card3, randomNumbers);
            GerardCitizensMaterialList[2].AssignMaterial(randomNumbers[2]);
            //player2.ObtainCitizenCard(player2.card3, randomNumbers);
        }
        //Click cards to select order 1 2 3 function

        //!!!!  Put citizens on the board (turn based)  !!!!
    }

    
}
