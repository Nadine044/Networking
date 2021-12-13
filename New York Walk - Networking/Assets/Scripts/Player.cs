using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
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
        public int[] unavailableSquares;
    }

    public Card card1 = new Card();
    public Card card2 = new Card();
    public Card card3 = new Card();

    //Modo guarro quick, despu�s ya se estructurar� mejor
    int[] board = new int[25];
    bool input_active = false;
    public int current_board_pos;

    List<int> tokens_list = new List<int>(); //this are our own tokens

    [HideInInspector]
    public GameObject current_token = null; //temporal but this number is linked (it's the same) to the card recieve
    [HideInInspector]
    public int turn_type = 0;
    public static Player _instance { get; private set; }

    int identifier_token_number = -1;
    int card_counter = 0;

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
            if(Input.GetMouseButton(0))
            {
                switch (turn_type)
                {
                    case 1:
                        SetInitialTokenPos(GameManager._instance.boardSquares, current_token, identifier_token_number);
                        break;
                    case 3:
                        SetTokenPos(GameManager._instance.boardSquares, current_token, identifier_token_number);
                        break;
                }
            }

        }
    }

    void SetBoardPos(int array_pos)
    {
        //token1.transform.position = GameManager._instance.array_positions[array_pos].transform.position;
        //current_board_pos = array_pos;

        //EP
      //  board[array_pos] = client_n; //1 in the array means the citizen is there 2 means the citizen 2 is here, etc
    }

    public void SetInitialTokenPos(List<GameObject> squares, GameObject token,int identifier_n)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string colliderName;

        if (Physics.Raycast(ray, out hit))
        {
            colliderName = hit.collider.name;
            for(int i =0; i < squares.Count; i++)
            {
                if(squares[i].name == colliderName)
                {
                    //first check if the position is already full
                    if(board[i] !=0)
                    {
                        return;
                    }

                    token.transform.position = squares[i].transform.position; //moves the cube to the position
                    board[i] = identifier_n;
                    input_active = false;

                    //Now we clean the restricted space //TODO better using linq funcs
                    for(int j =0; i < board.Length -1; j++)
                    {
                        if (board[j] == -1)
                            board[j] = 0;
                    }
                    NetworkingClient._instance.SendPackage();
                    return;
                }
            }
        }
    }

    //TODO CLEAN THE PREVIOUS TOKEN POSITION 
    public void SetTokenPos(List<GameObject> squares, GameObject token, int identifier_n)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string colliderName;

        if (Physics.Raycast(ray, out hit))
        {
            colliderName = hit.collider.name;
            for (int i = 0; i < squares.Count; i++)
            {
                if (squares[i].name == colliderName)
                {
                    //first check if the position is already full
                    if (board[i] != 0)
                    {
                        return;
                    }

                    //clean the previous position
                    List<int> list_array = board.ToList();
                    board[list_array.IndexOf(identifier_n)] =0;

                    token.transform.position = squares[i].transform.position; //moves the cube to the position
                    board[i] = identifier_n;
                    input_active = false;

                    NetworkingClient._instance.SendPackage();
                    return;
                }
            }
        }
    }


    public int[] GetBoard()
    {
        return board;
    }


    public void RecieveUpdateFromServer(int turnstep,int[] new_board,int card)
    {
        turn_type = turnstep;
        board = new_board;

        if (turn_type == 1) //its means whe are setting cards
        {

            //we have to check if the tokens are already placed
            //tokens_list.Add(card);

            //Update the tokens according to the board
            for(int i =0; i<board.Length;i++)
            {
                if(board[i] !=0 && !tokens_list.Contains(board[i])) //there is some token there that isnt in the list, so we must create a new token and place it
                {

                    GameObject newtoken = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newtoken.transform.position = GameManager._instance.array_positions[0].transform.position; //we place this in the position;
                    tokens_list.Add(board[i]);//enemy token //TODO think what we will do in the future token class,etc
                }
            }

            //Search for material 
            Card n_card = GetCitizenCardInfo(card); //here we could save a list or something of the cards
            GameManager._instance.SetMaterial(card_counter, card);
            card_counter++;
            // place new token
            identifier_token_number = card;
            current_token = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //now we make a restricted space to set the token through the card calss
            CreateRestrictedSpace(n_card.unavailableSquares);
            input_active = true;


        }


    }

    void CreateRestrictedSpace(int[] noavailablepos)
    {
        int tmp_counter = 0;
        for(int i =0; i < board.Length -1; i++)
        {
            //if the current square equals the first value of the restricted squares array it means we have to set 
            //board[i] to a restricted space
            if(i == noavailablepos[tmp_counter])
            {
                board[i] = -1;
                //it means we are out of the array index
                if (noavailablepos.Length < tmp_counter + 1)
                    return;

                tmp_counter++;
            }
        }
    }

    Card GetCitizenCardInfo(int card_n)
    {
        Card card = new Card();

        card.citizen = player_cards.playableCitizenList.citizens[card_n].citizen;
        card.pickUp = player_cards.playableCitizenList.citizens[card_n].pickUp;
        card.destiny = player_cards.playableCitizenList.citizens[card_n].destiny;
        card.difficulty = player_cards.playableCitizenList.citizens[card_n].difficulty;
        card.unavailableSquares = player_cards.playableCitizenList.citizens[card_n].unavailableSquares;

        return card;
    }
}
