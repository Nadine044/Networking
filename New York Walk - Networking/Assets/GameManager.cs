using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Player player1;
    public Player player2;

    public List<int> randomNumbers;


    // Start is called before the first frame update
    void Start()
    {

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
            player2.ObtainCitizenCard(player2.card1, randomNumbers);

            player1.ObtainCitizenCard(player1.card2, randomNumbers);
            player2.ObtainCitizenCard(player2.card2, randomNumbers);

            player1.ObtainCitizenCard(player1.card3, randomNumbers);
            player2.ObtainCitizenCard(player2.card3, randomNumbers);
        }
        //Click cards to select order 1 2 3 function

        //!!!!  Put citizens on the board (turn based)  !!!!
    }

    
}
