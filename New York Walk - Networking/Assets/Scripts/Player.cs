using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Player : MonoBehaviour
{
    public JSONReader player_cards;
    public JSONReader player_city_cards;

    public int randomNumberGenerated;

    public class Card
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int difficulty;
        public int[] unavailableSquares;
    }

    [System.Serializable]
    public class CityCard
    {
        public string name;
        public string utility;
        public int turns;
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

    //City Cards
    //public CityCard[] pileCards;

    List<int> random_numbers_city_cards = new List<int>();
    public List<GameObject> pileCards = new List<GameObject>();
    //public List<CityCard> pileCards = new List<CityCard>();
    List<CityCard> usedCards = new List<CityCard>();

    public GameObject cityCardsPile;
    public CityCard cityCard1 = new CityCard();
    public CityCard cityCard2 = new CityCard();
    public CityCard cityCard3 = new CityCard();

    int current_city_cards = 0;
    public GameObject unavailableSquareToken;

    bool isUsingSubwayCard = false;
    bool isUsingFilmingCard = false;
    bool isUsingVipCard = false;
    
    int turnsFilmingCard = 3;

    Vector3 card1UI_pos = new Vector3(12.6f, 3.97f, 0.27f);
    Vector3 card2UI_pos = new Vector3(12.6f, 2.97f, 0.27f);
    Vector3 card3UI_pos = new Vector3(12.6f, 1.97f, 0.27f);

    //Modo guarro quick, despu�s ya se estructurar� mejor
    int[] board = new int[25];
    bool input_active = true;
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
        if (input_active)
        {
            if(Input.GetMouseButton(0))
            {
                switch (turn_type)
                {
                    case 1:
                        Debug.Log("type 1");
                        SetInitialTokenPos(GameManager._instance.boardSquares);
                        break;
                    case 2:
                        Debug.Log("type 2");
                        SetInitialTokenPos(GameManager._instance.boardSquares);
                        break;
                    case 3:
                        Debug.Log("type 3");
                        SetTokenPos(GameManager._instance.boardSquares);
                        //decrease city cards turn use here??
                        break;
                }
            }

            if(Input.GetMouseButtonDown(1))
            {
                DrawCityCard(cityCardsPile, card1UI_pos);
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                input_active = true;
                SelectCityCard();
            }

            if ((isUsingFilmingCard || isUsingSubwayCard || isUsingVipCard) && Input.GetKeyDown(KeyCode.V))
            {
                UseCityCard(GameManager._instance.boardSquares, unavailableSquareToken);
            }

        }
    }
    public void SelectCityCard()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.name == "Stop1" || hit.collider.name == "Stop2" || hit.collider.name == "Stop3")
            {
                Debug.Log("STOP CARD USED!!");
            }

            else if (hit.collider.name == "VIP1" || hit.collider.name == "VIP2")
            {
                isUsingVipCard = true;
                Debug.Log("VIP CARD USED!!");
            }

            else if (hit.collider.name == "Security")
            {
                Debug.Log("SECURITY CARD USED");
            }

            else if (hit.collider.name == "Filming1" || hit.collider.name == "Filming2")
            {
                isUsingFilmingCard = true;
                Debug.Log("FILMING CARD USED!!");
            }

            else if (hit.collider.name == "Subway1" || hit.collider.name == "Subway2" || hit.collider.name == "Subway3")
            {
                //TODO: Si CUALQUIER posición del token del player coincide con parada de metro, activa esto (posiciones: 9, 13, 16)
                isUsingSubwayCard = true;
                Debug.Log("SUBWAY CARD USED!!");
            }
        }

        current_city_cards--;
    }

    public void UseCityCard(List<GameObject> squares, GameObject unavailableSquareToken)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string colliderName;
        int id = 0;

        if (Physics.Raycast(ray, out hit))
        {
            colliderName = hit.collider.name;
            for (id = 0; id < squares.Count; id++)
            {
                if (squares[id].name == colliderName && turnsFilmingCard > 0)
                {
                    //FILMING CARD
                    if (isUsingFilmingCard)
                    {
                        board[id] = -3;
                        unavailableSquareToken.transform.position = squares[id].transform.position;
                        Debug.Log(colliderName + " is not accessible during " + turnsFilmingCard);
                        //TODO: cuando decrece la variable de turnos??????
                    }

                    //SUBWAYCARD
                    else if (isUsingSubwayCard)
                    {
                        if (id == 9 || id == 13 || id == 16)
                        {
                            //current_token.gameObject.transform.position = squares[id].transform.position;
                            Debug.Log("Player teleported to " + colliderName);
                            //TODO: en este if tiene que teletransportar la posición del token que quiere mover
                        }
                    }
                    else if (isUsingVipCard)
                    {
                        Debug.Log("Using VIP Card");
                        if (board[id] == -3)
                        {
                            unavailableSquareToken.transform.position = new Vector3(28, 10, -7); /*starting point*/
                            Debug.Log("Restriction removed!!");
                            board[id] = -2;
                        }
                    }
                }
            }
        }

        if (turnsFilmingCard == 0)
            board[id] = -2;

        isUsingFilmingCard = false;
        isUsingSubwayCard = false;
        isUsingVipCard = false;
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
                    if (board[i] != -2 || board[i] == -3)
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


    public void AwaitForClientReconnection()
    {
        input_active = false;
        //Active ui text saying other client isn't connected
    }

    /// <summary>
    /// Function to update the reconnecting client, and client makes his move on the setup stage
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newboard"></param>
    /// <param name="token_l"></param>
    /// <param name="current_card"></param>
    public void RecieveReconnectionUpdateFromServerMoveSetUp(int index, int[] newboard, List<int> token_l, int current_card)
    {

        turn_type = 1;
        board = newboard;
        CheckNewTokens();
        //Add the material to the cards & card info of each card to the list
        for (int i = 0; i < token_l.Count; i++)
        {
            Card card = SearchAddMat(token_l[i]);
            Token_c t = tokens_list.First(token => token.identifier == token_l[i]);
            t.card = card;
        }
        Card c = SearchAddMat(current_card);
        CreateToken(current_card, c);

        //now we make a restricted space to set the token through the card calss
        CreateRestrictedSpace(c.unavailableSquares);
        input_active = true;
    }

    /// <summary>
    /// Function to update the reconnecting client, and client makes his move
    /// </summary>
    /// <param name="index"></param>
    /// <param name="newboard"></param>
    /// <param name="token_l"></param>
    public void RecieveReconnectionUpdateFromServerMove(int index,int[] newboard,List<int> token_l,int current_card)
    {
        turn_type = Mathf.Abs(index);
        board = newboard;
        CheckNewTokens();
        //Add the material to the cards & card info of each card to the list
        for (int i = 0; i < token_l.Count; i++)
        {
            Card card = SearchAddMat(token_l[i]);
            Token_c t = tokens_list.First(token => token.identifier == token_l[i]);
            t.card = card;
        }

        //now select the token to move
        Token_c token_to_move = tokens_list.First(token => token.identifier == current_card);
        current_token = token_to_move;
        input_active = true;
    }

    /// <summary>
    /// Function to update the reconnecting client, client doesn't have input
    /// </summary>
    /// <param name="newboard"></param>
    /// <param name="token_l"></param>
    public void RecieveReconnectionUpdateFromServerNoMove(int[] newboard, List<int> token_l)
    {
        board = newboard;
        board = newboard;
        CheckNewTokens();

        //Add the material to the cards & card info of each card to the list
        for(int i = 0; i < token_l.Count; i++)
        {
            Card card = SearchAddMat(token_l[i]);
            Token_c t = tokens_list.First(token => token.identifier == token_l[i]);
            t.card = card;
        }
        
        if (tokens_list.Count <= 5)
        {
            NetworkingClient._instance.SendSetUpPackage();
        }
        else
        {
            NetworkingClient._instance.SendPackage();
        }
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

            CheckNewTokens();

            Card c = SearchAddMat(card);

            CreateToken(card, c);

            //now we make a restricted space to set the token through the card calss
            CreateRestrictedSpace(c.unavailableSquares);
            input_active = true;
        }

        //TODO MAKE AND ESPECIFIC INDEX OR SOMETING FOR THE LAST TOKEN TO BE REPLICATED THOUGH
        else if (turn_type ==3)
        {
            //this will be changed //creates the new token
            
            NetworkingClient._instance.logText.text = "Turn Type 3";
            CheckNewTokens();//Must be done in turn 2 this

            //to update the other player tokens position
            UpdatePlacedTokens();

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

    void CheckNewTokens()
    {
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
    }

    void UpdatePlacedTokens()
    {
        //to update the other player tokens position
        for (int i = 0; i < board.Length; ++i)
        {
            if (board[i] != -2 && tokens_list.Any(token => token.identifier == board[i]))
            {
                Token_c t = tokens_list.First(token => token.identifier == board[i]); //gets the first element in the list that matches the condition
                t.gameObject.transform.position = GameManager._instance.array_positions[i].transform.position;
            }

        }
    }
    Card SearchAddMat(int card_id)
    {
        //Search for material 
        Card n_card = GetCitizenCardInfo(card_id); //here we could save a list or something of the cards
        GameManager._instance.SetMaterial(card_counter, card_id);
        card_counter++;//To iterate between gameobjects card i think, need to check
        return n_card;
    }

    void CreateToken(int card_id, Card card)
    {
        Token_c token = new Token_c(GameObject.CreatePrimitive(PrimitiveType.Cube), card_id);
        token.card = card;
        tokens_list.Add(token);
        NetworkingClient._instance.logText.text = "Token created";
        // place new token
        current_token = token;
    }


    public void DrawCityCard (GameObject cardsToDraw, Vector3 UI_card_position)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        string drawCardsCollider;

        if (Physics.Raycast(ray, out hit))
        {
            drawCardsCollider = hit.collider.name;

            if (drawCardsCollider == "CityCardsDraw_Pile" && current_city_cards < 3)
            {
                //Give player random city card and add it to usedPileCards. Quit from ToDraw list
                //drawCard

                Debug.Log("CITY CARD PICKED!!");
                //card1UI_pos

                if (current_city_cards < 1)
                {
                    GetCityCardInfo(cityCard1, random_numbers_city_cards);

                    foreach (var item in pileCards)
                    {
                        if(item.name == cityCard1.name)
                        {
                            item.transform.position = card1UI_pos;
                            pileCards.Remove(item);
                            Debug.Log(cityCard1.name);
                            break;
                        }
                    }
                }

                else if (current_city_cards < 2)
                {
                    GetCityCardInfo(cityCard2, random_numbers_city_cards);
                    foreach (var item in pileCards)
                    {
                        if (item.name == cityCard2.name)
                        {
                            item.transform.position = card2UI_pos;
                            pileCards.Remove(item);
                            Debug.Log(cityCard2.name);
                            break;
                        }
                    }
                }
                else
                {
                    GetCityCardInfo(cityCard3, random_numbers_city_cards);
                    foreach (var item in pileCards)
                    {
                        if (item.name == cityCard3.name)
                        {
                            item.transform.position = card3UI_pos;
                            pileCards.Remove(item);
                            Debug.Log(cityCard3.name);

                            Debug.Log("--------------------");
                            Debug.Log(cityCard1.name);
                            Debug.Log(cityCard2.name);
                            Debug.Log(cityCard3.name);
                            break;
                        }
                    }
                }
            }
        }

        

        current_city_cards++;
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

    void GetCityCardInfo(CityCard card, List<int> randomNumbers)
    {
        //randomNumberGenerated = Random.RandomRange(0, 11);

        if (current_city_cards == 1)
            randomNumberGenerated = Random.Range(0, 11);
        else if (current_city_cards == 2)
            randomNumberGenerated = Random.Range(0, 10);
        else if (current_city_cards == 3)
            randomNumberGenerated = Random.Range(0, 9);

        if (!randomNumbers.Contains(randomNumberGenerated))
        {
            randomNumbers.Add(randomNumberGenerated);

            card.name = player_city_cards.cityCardsList.powerUps[randomNumberGenerated].name;
            card.utility = player_city_cards.cityCardsList.powerUps[randomNumberGenerated].utility;
            card.turns = player_city_cards.cityCardsList.powerUps[randomNumberGenerated].turns;            
        }
        else
            GetCityCardInfo(card, randomNumbers);
    }
}
