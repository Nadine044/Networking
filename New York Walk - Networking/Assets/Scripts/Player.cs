using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public JSONReader player_cards;
    public int randomNumberGenerated;

    CitizenMaterial citizensMaterials;

    public struct Card
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int difficulty;
    }

    public Card card1;
    public Card card2;
    public Card card3;

    //Modo guarro quick, después ya se estructurará mejor
    int[] board_pos = new int[25];
    public GameObject citizen_token;
    bool input_active = true;
    int current_board_pos;
    // Start is called before the first frame update
    void Start()
    {

        //set al positions empty
        for(int i =0; i < 25; i++)
        {
            board_pos[i] = 0;
        }

        SetBoardPos(0);
        
    }

    // Update is called once per frame
    void Update()
    {
        if(input_active)
        {
            if(Input.GetKeyDown(KeyCode.UpArrow))
            {
                Go_UP();
                //input_active = false;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                Go_DOWN();
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Go_LEFT();
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Go_RIGHT();
            }


        }
    }

    void Go_UP()
    {
        //In the case its not reversed.. the reversed way will be converted in the server script
        int tmp = current_board_pos +5;
        if(tmp > board_pos.Length -1)
        {
            Debug.Log("position out of board, you can't go up more");

        }
        else
        {
            //clean the prevous board pos
            board_pos[current_board_pos] = 0;

            SetBoardPos(tmp);
        }
    }

    void Go_DOWN()
    {
        //In the case its not reversed.. the reversed way will be converted in the server script
        int tmp = current_board_pos - 5;
        if (tmp < 0)
        {
            Debug.Log("position out of board, you can't go down more");
        }
        else
        {
            //clean the prevous board pos
            board_pos[current_board_pos] = 0;
            SetBoardPos(tmp);
        }
    }
    void Go_LEFT()
    { 
        switch(current_board_pos)
        {
            case 4:
                Debug.Log("position out of board, you can't go left anymore");
                break;
            case 9:
                Debug.Log("position out of board, you can't go left anymore");
                break;
            case 14:
                Debug.Log("position out of board, you can't go left anymore");
                break;
            case 19:
                Debug.Log("position out of board, you can't go left anymore");
                break;
            case 24:
                Debug.Log("position out of board, you can't go left anymore");
                break;
            default:
                board_pos[current_board_pos] = 0;
                SetBoardPos(current_board_pos + 1);
                break;
        }
    }
    void Go_RIGHT()
    {
        switch (current_board_pos)
        {
            case 0:
                Debug.Log("position out of board, you can't go right anymore");
                break;
            case 5:
                Debug.Log("position out of board, you can't go right anymore");
                break;
            case 10:
                Debug.Log("position out of board, you can't go right anymore");
                break;
            case 15:
                Debug.Log("position out of board, you can't go right anymore");
                break;
            case 20:
                Debug.Log("position out of board, you can't go right anymore");
                break;
            default:
                board_pos[current_board_pos] = 0;
                SetBoardPos(current_board_pos - 1);
                break;
        }
    }
    void SetBoardPos(int array_pos)
    {
        citizen_token.transform.position = GameManager._instance.array_positions[array_pos].transform.position;
        current_board_pos = array_pos;
        board_pos[array_pos] = 1; //1 in the array means the citizen is there
    }


    public void ObtainCitizenCard(Card randomCard, List<int> randomNumbers)
    {
        randomNumberGenerated = Random.RandomRange(0, 8);

        if (!randomNumbers.Contains(randomNumberGenerated))
        {
            randomNumbers.Add(randomNumberGenerated);

            randomCard.citizen = player_cards.playableCitizenList.citizens[randomNumberGenerated].citizen;
            randomCard.pickUp = player_cards.playableCitizenList.citizens[randomNumberGenerated].pickUp;
            randomCard.destiny = player_cards.playableCitizenList.citizens[randomNumberGenerated].destiny;
            randomCard.difficulty = player_cards.playableCitizenList.citizens[randomNumberGenerated].difficulty;

            Debug.Log(randomCard.citizen);
            //Debug.Log(randomCard.pickUp);
            //Debug.Log(randomCard.destiny);
            //Debug.Log(randomCard.difficulty);
        }
        else
            ObtainCitizenCard(randomCard, randomNumbers);
    }
}
