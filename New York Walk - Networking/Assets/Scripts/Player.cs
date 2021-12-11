using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public JSONReader player_cards;
    public int randomNumberGenerated;

    public class Card
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int difficulty;
    }

    public Card card1 = new Card();
    public Card card2 = new Card();
    public Card card3 = new Card();

    //Modo guarro quick, despu�s ya se estructurar� mejor
    int[] board = new int[25];
    public GameObject token1;
    bool input_active = false;
    public int current_board_pos;
    bool first_time = true;
    [HideInInspector]
    public int client_n = 0;
    [HideInInspector]
    public int turn_type = 0;
    public static Player _instance { get; private set; }


    GameObject enemys_token = null;
    // Start is called before the first frame update
    void Start()
    {
        _instance = this;
        //set al positions empty
        for(int i =0; i < 25; i++)
        {
            board[i] = 0;
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
        if(tmp > board.Length -1)
        {
            Debug.Log("position out of board, you can't go up more");

        }
        else
        {
            //clean the prevous board pos
            if (board[tmp] != 0) //means there is something there
                return;
            //clean the prevous board pos
            board[current_board_pos] = 0;
            SetBoardPos(tmp);
            input_active = false;
            NetworkingClient._instance.SendPackage();
            //Here invoke event to send package to server
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
            if (board[tmp] != 0) //means there is something there
                return;
            //clean the prevous board pos
            board[current_board_pos] = 0;
            SetBoardPos(tmp);
            input_active = false;
            NetworkingClient._instance.SendPackage();
            //Here invoke event to send package to server
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
                if (board[current_board_pos + 1] != 0)//there is something there
                    return;

                board[current_board_pos] = 0;
                SetBoardPos(current_board_pos + 1);
                input_active = false;
                NetworkingClient._instance.SendPackage();
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
                if (board[current_board_pos - 1] != 0)//there is something there
                    return;
                board[current_board_pos] = 0;
                SetBoardPos(current_board_pos - 1);
                input_active = false;
                NetworkingClient._instance.SendPackage();
                break;
        }
    }
    void SetBoardPos(int array_pos)
    {
        token1.transform.position = GameManager._instance.array_positions[array_pos].transform.position;
        current_board_pos = array_pos;

        
        board[array_pos] = client_n; //1 in the array means the citizen is there 2 means the citizen 2 is here, etc
    }

    public void SetTokenPos(List<GameObject> squares, Player player)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string colliderName;

        if (Physics.Raycast(ray, out hit))
        {
            colliderName = hit.collider.name;
            foreach (var Item in squares)
            {
                if (Item.name == colliderName)
                {
                    player.token1.transform.position = Item.transform.position;
                    //get current pos on board too
                }
            }
        }
    }


    public void ObtainCitizenCard(Card randomCard, List<int> randomNumbers)
    {
        randomNumberGenerated = Random.Range(0, 24);

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

    public int[] GetBoard()
    {
        return board;
    }


    public void RecieveUpdateFromServer(int client_type,int turnstep,int[] new_board)
    {
        client_n = client_type;
        turn_type = turnstep;
        board = new_board;

        if (turn_type == 1)
            input_active = true;

        if(client_n ==2)
        {

            //now spawn the other players token
            if (first_time)
            {
                board[current_board_pos] = 0;
                SetBoardPos(4);
                enemys_token = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemys_token.transform.position = GameManager._instance.array_positions[0].transform.position;
                board[4] = 1;
                first_time = false;
            }

            else
            {
                for(int i =0; i < board.Length; i++)
                {
                    if(board[i]==1) //the new enemy token_position
                    {
                        enemys_token.transform.position = GameManager._instance.array_positions[i].transform.position;
                        //board[i] = 1;
                    }
                }
            }
        }

        else if(client_n == 1)
        {
            //keep the current pos
            if(first_time)
            {
                enemys_token = GameObject.CreatePrimitive(PrimitiveType.Cube);
                enemys_token.transform.position = GameManager._instance.array_positions[4].transform.position;
                board[4] = 2;
                first_time = false;
            }
            else
            {
                for(int i =0; i < board.Length; i++)
                {
                    if (board[i] == 2) //the new enemy token_position
                    {
                        enemys_token.transform.position = GameManager._instance.array_positions[i].transform.position;
                      //  board[i] = 2;
                    }
                }
            }
        }
        turn_type = 2;
        //now have to say that if client type is 2 our cube follows the 2 on the array
    }
}
