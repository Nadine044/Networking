using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class Player : MonoBehaviour
{
    public JSONReader player_cards;
    public JSONReader player_city_cards;

    public int randomNumberGenerated;

    List<CityCard> drawCard = new List<CityCard>();
    List<CityCard> usedCards = new List<CityCard>();

    public class Card
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int difficulty;
        public int[] unavailableSquares;
    }

    public class CityCard
    {
        public string name;
        public string utility;
        public int turns;
        public int howMany;
    }

    public class Token_c
    {
        public GameObject gameObject;
        public int identifier;
        public Card card;
        public Token_c(GameObject go, int id)
        {
            gameObject = go;
            identifier = id;
        }
    }

    public Card card1 = new Card();
    public Card card2 = new Card();
    public Card card3 = new Card();

    public GameObject cityCardsPile;
    public CityCard cityCard1 = new CityCard();
    public CityCard cityCard2 = new CityCard();
    public CityCard cityCard3 = new CityCard();

    //Modo guarro quick, despu�s ya se estructurar� mejor
    int[] board = new int[25];
    bool input_active = false;
    public int current_board_pos;

    List<Token_c> tokens_list = new List<Token_c>(); //this are our own tokens
    Token_c current_token;
    [HideInInspector]
    public int turn_type = 0;
    public static Player _instance { get; private set; }


    int card_counter = 0;
    // Start is called before the first frame update
    
    void Start()
    {
        _instance = this;
        //set al positions empty
        for(int i =0; i < 25; i++)
        {
            board[i] = -2;
        }        
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
                        SetInitialTokenPos(GameManager._instance.boardSquares);
                        break;
                    case 2:
                        SetInitialTokenPos(GameManager._instance.boardSquares);
                        break;
                    case 3:
                        SetTokenPos(GameManager._instance.boardSquares);
                        break;
                }
            }


            if(Input.GetMouseButton(1))
            {
                DrawCityCard(cityCardsPile);
            }
        }
    }

    public void SetInitialTokenPos(List<GameObject> squares)
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
                    if(board[i] != -2)
                    {
                        return;
                    }

                    current_token.gameObject.transform.position = squares[i].transform.position; //moves the cube to the position
                    board[i] = current_token.identifier;
                    input_active = false;

                    //Now we clean the restricted space //TODO better using linq funcs
                    for(int j =0; j < board.Length; j++)
                    {
                        if (board[j] == -1)
                            board[j] = -2;
                    }
                    if(turn_type ==1)
                        NetworkingClient._instance.SendSetUpPackage();
                    else if(turn_type ==2)
                        NetworkingClient._instance.SendPackage();
                    return;
                }
            }
        }
    }

    public void SetTokenPos(List<GameObject> squares)
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
                    if (board[i] != -2)
                    {
                        return;
                    }

                    //clean the previous position
                    List<int> list_array = board.ToList();
                    board[list_array.IndexOf(current_token.identifier)] = -2;

                    current_token.gameObject.transform.position = squares[i].transform.position; //moves the cube to the position
                    board[i] = current_token.identifier;
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

        NetworkingClient._instance.logText.text = "Recieve Update Player";

        //TODO CHANGE THIS PRETTY DIRTY
        if (turn_type == 1 || turn_type == 2) //its means whe are setting cards
        {
            NetworkingClient._instance.logText.text = "Turn Type 1";
            //we have to check if the tokens are already placed
            //tokens_list.Add(card);
            //Update the tokens according to the board
            for(int i =0; i<board.Length;i++)
            {
                if(board[i] != -2 && !tokens_list.Any(enemy_token => enemy_token.identifier == board[i])) //there is some token there that isnt in the list, so we must create a new token and place it
                {
                    GameObject newtoken = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newtoken.transform.position = GameManager._instance.array_positions[i].transform.position; //we place this in the position;
                    Token_c t = new Token_c(newtoken,board[i]);
                    tokens_list.Add(t);
                }
            }

            NetworkingClient._instance.logText.text = "Searching for material " + card;

            //Search for material 
            Card n_card = GetCitizenCardInfo(card); //here we could save a list or something of the cards
            GameManager._instance.SetMaterial(card_counter, card);

            //create token & add it to the list
            Token_c token = new Token_c(GameObject.CreatePrimitive(PrimitiveType.Cube), card);
            token.card = n_card;
            card_counter++;
            tokens_list.Add(token);
            NetworkingClient._instance.logText.text = "Token created";

            // place new token
            current_token = token;

            //now we make a restricted space to set the token through the card calss
            CreateRestrictedSpace(n_card.unavailableSquares);
            input_active = true;


        }

        //TODO MAKE AND ESPECIFIC INDEX OR SOMETING FOR THE LAST TOKEN TO BE REPLICATED THOUGH
        if (turn_type ==3)
        {
            //this will be changed //creates the new token
            for (int i = 0; i < board.Length; i++)
            {
                if (board[i] != -2 && !tokens_list.Any(enemy_token => enemy_token.identifier == board[i])) //there is some token there that isnt in the list, so we must create a new token and place it
                {
                    GameObject newtoken = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newtoken.transform.position = GameManager._instance.array_positions[i].transform.position; //we place this in the position;
                    Token_c t = new Token_c(newtoken, board[i]);
                    tokens_list.Add(t);
                }
            }

            NetworkingClient._instance.logText.text = "Turn Type 3";

            //to update the other player tokens position
            for (int i =0; i < board.Length; ++i)
            {
                if (board[i] != -2 && tokens_list.Any(token => token.identifier == board[i])) 
                {
                    Token_c t = tokens_list.First(token => token.identifier == board[i]); //gets the first element in the list that matches the condition
                    t.gameObject.transform.position = GameManager._instance.array_positions[i].transform.position;
                }
                
            }

            Token_c token_to_move = tokens_list.First(token => token.identifier == card);
            current_token = token_to_move;

            input_active = true;
            //no we set our move 
        }

        NetworkingClient._instance.logText.text = "paSSED turn type";

    }

    void CreateRestrictedSpace(int[] noavailablepos)
    {
        for (int i = 0; i < noavailablepos.Length; ++i)
        {
            if (board[noavailablepos[i]] == 0)
                board[noavailablepos[i]] = -1;
        }
        NetworkingClient._instance.logText.text = "Restricted Space done";
    }
    public void DrawCityCard (GameObject cardsToDraw)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string drawCardsCollider;

        if (Physics.Raycast(ray, out hit) && cardsToDraw.name == "CityCardsDraw_Pile")
        {
            Debug.Log("HEEEEEEELLOOOOOOOO :D");
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

    CityCard GetCityCardInfo(int card_n)
    {
        CityCard card = new CityCard();

        card.name = player_city_cards.cityCardsList.powerUps[card_n].name;
        card.utility = player_city_cards.cityCardsList.powerUps[card_n].utility;
        card.turns = player_city_cards.cityCardsList.powerUps[card_n].turns;
        card.howMany = player_city_cards.cityCardsList.powerUps[card_n].howMany;

        return card;
    }
}
