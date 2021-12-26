using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using UnityEngine.UI;
public class Player : MonoBehaviour
{
    public JSONReader player_cards;

    public int randomNumberGenerated;
    public List<Material> flag_material_list;
    public class Card
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int pickUpID;
        public int destinyID;
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
        public GameObject pickUp;
        public GameObject destiny;
        public bool pickUpObtained = false;
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
    public GameObject stopCones;

    //bool isUsingSubwayCard = false;
    //bool isUsingFilmingCard = false;
    //bool isUsingVipCard = false;
    //bool isUsingStop = false;
    
    //int turnsFilmingCard = 3;

    //Vector3 card1UI_pos = new Vector3(12.6f, 3.97f, 0.27f);
    //Vector3 card2UI_pos = new Vector3(12.6f, 2.97f, 0.27f);
    //Vector3 card3UI_pos = new Vector3(12.6f, 1.97f, 0.27f);

    int[] board = new int[25];
    bool input_active = false;

    List<Token_c> tokens_list = new List<Token_c>(); //this are our own tokens
    Token_c current_token;
    [SerializeField]
    GameObject token_prefab_base;

    [HideInInspector]
    public int turn_type = 0;
    public static Player _instance { get; private set; }
    public Button pass_turn_btn;
    [SerializeField]
    private GameObject waiting_reconnec_text;

    int card_counter = 0;
    // Start is called before the first frame update
    bool testpas_once = false;

    //Destiny & pickup
    public GameObject destinyprefab;
    public GameObject pickUpprefab;

    //win condition
    private int win_counter = 0;
    void Start()
    {
        _instance = this;
        //set al positions empty
        for(int i =0; i < 25; i++)
        {
            board[i] = -2;
        }
        pass_turn_btn.gameObject.SetActive(false);
        waiting_reconnec_text.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if(!testpas_once && tokens_list.Count == 6)//todo dirty
        {
            pass_turn_btn.gameObject.SetActive(true);
            testpas_once = true;
        }
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
                    case 3:
                        Debug.Log("type 3");
                        SetTokenPos(GameManager._instance.boardSquares);
                        //decrease city cards turn use here??
                        break;
                }
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

                    //clean it vissualy
                    GameManager._instance.EraseRestrictedSpaceCubes();

                    //stop animation
                    current_token.gameObject.GetComponent<Animator>().SetBool("start", false);
                    current_token.pickUp.SetActive(false);
                    //Update server

                   
                    NetworkingClient._instance.SendSetUpPackage(1);
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

                    //Check if the position is adjacent to the current pos in a cross form
                    if(!CheckAdjacentSquares(i)) 
                    {
                        return;
                    }

                    //clean the previous position
                    board[Array.IndexOf(board, current_token.identifier)] = -2;

                    current_token.gameObject.transform.position = squares[i].transform.position; //moves the cube to the position
                    board[i] = current_token.identifier;
                    input_active = false;
                    //stop animation
                    current_token.gameObject.GetComponent<Animator>().SetBool("start", false);

                    if (current_token.pickUp != null)
                        current_token.pickUp.SetActive(false);

                    ChangeAlphaMaterial(current_token.destiny, 0.45f);
                    //Check win condition
                    if (!current_token.pickUpObtained)
                    {
                         current_token.pickUpObtained = CheckPickUp(i);
                    }
                    else
                    {
                        if(CheckDestiny(i))
                        {
                            return;
                        }
                    }
                    NetworkingClient._instance.SendPackage();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// This function validates that the clicked board pos is adjacent to the current token, not diagonals though
    /// </summary>
    /// <param name="clicked_pos"></param>
    /// <returns></returns>
    private bool CheckAdjacentSquares(int clicked_pos)
    {
        //Get our current pos in the array/board
        int idx = Array.IndexOf(board, current_token.identifier);
        int row_offset = 5;
        int col_offset = 1;

        if((idx % 5) == 0 && idx - col_offset == clicked_pos) //it means is on the right edge of the board and clicked on the left edge
        {
            Debug.Log("right to left false");
            return false;
        }
        else if(((idx + 1) % 5) == 0 && idx + col_offset == clicked_pos) //it means is on the left edge of the board and clicked on the right edge
        {
            Debug.Log("left to right false");
            return false;
        }

        //here we validate that the clicked position is adjacent to the current token //diagonals don't count
        if(idx + row_offset == clicked_pos ||  idx - row_offset == clicked_pos ||
            idx + col_offset == clicked_pos || idx - col_offset == clicked_pos)
        {
            //check that we don't jump directly from right to left
            if(idx % 5 == 0 && (idx + row_offset) % 5 !=0)
            {
                Debug.Log("Double check return false 1");
                return false;
            }
            //check that we don't jump directly from left to right
            else if (idx + 1 % 5 == 0 && (idx - row_offset + 1) % 5 != 0 )
            {
                Debug.Log("Double check return false 2");
                return false; 
            }
            return true;
        }
        return false;
    }

    public int[] GetBoard()
    {
        return board;
    }

    public void AwaitForClientReconnection()
    {
        waiting_reconnec_text.SetActive(true);
        input_active = false;
    }

    public void ResumePlay()
    {
        waiting_reconnec_text.SetActive(false);
        input_active = true;
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
            current_token = t;
            SetDestinyPickUp();
            current_token.pickUp.SetActive(false);
        }
        Card c = SearchAddMat(current_card);
        CreateToken(current_card, c);
        SetDestinyPickUp();
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
    public void RecieveReconnectionUpdateFromServerMove(int index,int[] newboard,List<int> token_l,int current_card,int w_counter)
    {
        win_counter = w_counter;
        turn_type = Mathf.Abs(index);
        board = newboard;
        CheckNewTokens();
        //Add the material to the cards & card info of each card to the list
        for (int i = 0; i < token_l.Count; i++)
        {
            Card card = SearchAddMat(token_l[i]);
            Token_c t = tokens_list.First(token => token.identifier == token_l[i]);
            t.card = card;
            current_token = t;
            SetDestinyPickUp();
            current_token.pickUp.SetActive(false);
        }
        //now select the token to move
        Token_c token_to_move = tokens_list.First(token => token.identifier == current_card);
        current_token = token_to_move;
        current_token.gameObject.GetComponent<Animator>().SetBool("start", true);
        current_token.pickUp.SetActive(true);
        ChangeAlphaMaterial(current_token.destiny, 0.45f);
        input_active = true;
    }

    /// <summary>
    /// Function to update the reconnecting client, client doesn't have input
    /// </summary>
    /// <param name="newboard"></param>
    /// <param name="token_l"></param>
    public void RecieveReconnectionUpdateFromServerNoMove(int[] newboard, List<int> token_l,int w_counter)
    {
        win_counter = w_counter;
        board = newboard;
        CheckNewTokens();

        //Add the material to the cards & card info of each card to the list
        for(int i = 0; i < token_l.Count; i++)
        {
            Card card = SearchAddMat(token_l[i]);
            Token_c t = tokens_list.First(token => token.identifier == token_l[i]);
            t.card = card;
            current_token = t;
            SetDestinyPickUp();
            current_token.pickUp.SetActive(true);
            ChangeAlphaMaterial(current_token.destiny, 0.45f);
        }
    }

    /// <summary>
    /// Updates the board, checks for new tokens & unlock the player input
    /// Used on the setup game stage
    /// </summary>
    /// <param name="turnstep"></param>
    /// <param name="new_board"></param>
    /// <param name="card"></param>
    public void RecieveUpdateFromServerSetUp(int turnstep,int[] new_board,int card)
    {
        waiting_reconnec_text.SetActive(false);
        turn_type = turnstep;
        board = new_board;
        NetworkingClient._instance.logText.text = "Recieve Update Player SetUp";
        CheckNewTokens();
        Card c = SearchAddMat(card);
        CreateToken(card, c);
        //create destiny and pickUp go
        SetDestinyPickUp();

        //now we make a restricted space to set the token through the card calss
        CreateRestrictedSpace(c.unavailableSquares);
        input_active = true;
    }

    /// <summary>
    /// Updates the board, checks for new tokens & unlock the player input
    /// Used on the ongoing game stage
    /// </summary>
    /// <param name="turnstep"></param>
    /// <param name="new_board"></param>
    /// <param name="card"></param>
    public void RecieveUpdateFromServer(int turnstep,int[] new_board,int card)
    {
        waiting_reconnec_text.SetActive(false);
        turn_type = turnstep;
        board = new_board;
        NetworkingClient._instance.logText.text = "Recieve Update Player";
        CheckNewTokens();
        UpdatePlacedTokens();
        Token_c token_to_move = tokens_list.First(token => token.identifier == card);
        current_token = token_to_move;

        if(current_token.pickUp != null)
            current_token.pickUp.SetActive(true);

        ChangeAlphaMaterial(current_token.destiny, 1f);
        current_token.gameObject.GetComponent<Animator>().SetBool("start", true);
        input_active = true;
    }

    /// <summary>
    /// Makes the given array unavailable positions to setup the token
    /// </summary>
    /// <param name="noavailablepos"></param>
    void CreateRestrictedSpace(int[] noavailablepos) //TODO MAKE DEPLOY PLACE RED OR SOMETHING
    {
        for (int i = 0; i < noavailablepos.Length; ++i)
        {
            if (board[noavailablepos[i]] == -2)
                board[noavailablepos[i]] = -1;
        }
        NetworkingClient._instance.logText.text = "Restricted Space done";
        //to make show it visually on the board
        GameManager._instance.SetRestrictedSpaceCubes(noavailablepos);
    }

    void CheckNewTokens()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (board[i] != -2 && !tokens_list.Any(enemy_token => enemy_token.identifier == board[i])) //there is some token there that isnt in the list, so we must create a new token and place it
            {
                GameObject newtoken = Instantiate(token_prefab_base);
                newtoken.transform.position = GameManager._instance.boardSquares[i].transform.position; //we place this in the position;
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
                t.gameObject.transform.position = GameManager._instance.boardSquares[i].transform.position;
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
        Token_c token = new Token_c(Instantiate(token_prefab_base), card_id);
        token.card = card;
        tokens_list.Add(token);
        NetworkingClient._instance.logText.text = "Token created";
        //instantia the pickup and the destiny gameobjects
        current_token = token;
    }

    Card GetCitizenCardInfo(int card_n)
    {
        Card card = new Card();
        card.citizen = player_cards.playableCitizenList.citizens[card_n].citizen;
        card.pickUp = player_cards.playableCitizenList.citizens[card_n].pickUp;
        card.destiny = player_cards.playableCitizenList.citizens[card_n].destiny;
        card.pickUpID = player_cards.playableCitizenList.citizens[card_n].pickUpID;
        card.destinyID = player_cards.playableCitizenList.citizens[card_n].destinyID;
        card.difficulty = player_cards.playableCitizenList.citizens[card_n].difficulty;
        card.unavailableSquares = player_cards.playableCitizenList.citizens[card_n].unavailableSquares;
        return card;
    }

    void GetCityCardInfo(CityCard card, List<int> randomNumbers)
    {
        //randomNumberGenerated = Random.RandomRange(0, 11);
        if (current_city_cards == 1)
            randomNumberGenerated = UnityEngine.Random.Range(0, 11);
        else if (current_city_cards == 2)
            randomNumberGenerated = UnityEngine.Random.Range(0, 10);
        else if (current_city_cards == 3)
            randomNumberGenerated = UnityEngine.Random.Range(0, 9);

        if (!randomNumbers.Contains(randomNumberGenerated))
        {
            randomNumbers.Add(randomNumberGenerated);
            card.name = player_cards.cityCardsList.powerUps[randomNumberGenerated].name;
            card.utility = player_cards.cityCardsList.powerUps[randomNumberGenerated].utility;
            card.turns = player_cards.cityCardsList.powerUps[randomNumberGenerated].turns;            
        }
        else
            GetCityCardInfo(card, randomNumbers);
    }

    public void PassTurnBtnAction()
    {
        if(input_active)
        {
            ChangeAlphaMaterial(current_token.destiny, 0.45f);
            current_token.gameObject.GetComponent<Animator>().SetBool("start", false);
            NetworkingClient._instance.SendPackage();
            input_active = false;
        }
    }

    void SetDestinyPickUp()
    {
        for (int i = 0; i < board.Length; i++)
        {
            if (i == current_token.card.pickUpID)
            {
                GameObject pick = Instantiate(pickUpprefab);
                pick.transform.position = GameManager._instance.boardSquares[i].transform.position;
                //Color color = pick.GetComponentInChildren<Material>().color;
                //color.a = 0.5f; //0 is transparent 1, opaque
                //pick.GetComponentInChildren<Material>().color = color;
                current_token.pickUp = pick;
            }
            else if (i == current_token.card.destinyID)
            {
                GameObject go = Instantiate(pickUpprefab);
                go.transform.position = GameManager._instance.boardSquares[i].transform.position;
                //go.transform.Rotate(new Vector3(90f, 0, 0));
                current_token.destiny = go;

                current_token.destiny.GetComponentInChildren<MeshRenderer>().material = flag_material_list[card_counter - 1];
                ChangeAlphaMaterial(current_token.destiny, 0.45f);
            }
        }
    }

    void ChangeAlphaMaterial(GameObject go, float alpha)
    {
        Color color = go.GetComponentInChildren<MeshRenderer>().material.color;
        color.a = alpha; //0 is transparent 1, opaque
        go.GetComponentInChildren<MeshRenderer>().material.color = color;
    }

    bool CheckPickUp(int pos)
    {
        if(current_token.card.pickUpID == pos)
        {
            Destroy(current_token.pickUp);
            return true;
        }
        return false;
    }

    bool CheckDestiny(int pos)
    {
        if(current_token.card.destinyID == pos)
        {
            win_counter++;
            NetworkingClient._instance.SendWinPackage(current_token.identifier);
            if(win_counter == 3)
            {
                NetworkingClient._instance.logText.text = "We have won!!!!!";
                NetworkingClient._instance.CloseConnection();
            }
            return true;
        }
        return false;
    }

    ///WIP FOR NEXT ASSIGNMENT
    ///

    //public void DrawCityCard (GameObject cardsToDraw, Vector3 UI_card_position)
    //{
    //    RaycastHit hit;
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    string drawCardsCollider;

    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        drawCardsCollider = hit.collider.name;

    //        if (drawCardsCollider == "CityCardsDraw_Pile" && current_city_cards < 3)
    //        {
    //            //Give player random city card and add it to usedPileCards. Quit from ToDraw list
    //            //drawCard

    //            Debug.Log("CITY CARD PICKED!!");
    //            //card1UI_pos

    //            if (current_city_cards < 1)
    //            {
    //                GetCityCardInfo(cityCard1, random_numbers_city_cards);

    //                foreach (var item in pileCards)
    //                {
    //                    if(item.name == cityCard1.name)
    //                    {
    //                        item.transform.position = card1UI_pos;
    //                        pileCards.Remove(item);
    //                        Debug.Log(cityCard1.name);
    //                        break;
    //                    }
    //                }
    //            }

    //            else if (current_city_cards < 2)
    //            {
    //                GetCityCardInfo(cityCard2, random_numbers_city_cards);
    //                foreach (var item in pileCards)
    //                {
    //                    if (item.name == cityCard2.name)
    //                    {
    //                        item.transform.position = card2UI_pos;
    //                        pileCards.Remove(item);
    //                        Debug.Log(cityCard2.name);
    //                        break;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                GetCityCardInfo(cityCard3, random_numbers_city_cards);
    //                foreach (var item in pileCards)
    //                {
    //                    if (item.name == cityCard3.name)
    //                    {
    //                        item.transform.position = card3UI_pos;
    //                        pileCards.Remove(item);
    //                        Debug.Log(cityCard3.name);

    //                        Debug.Log("--------------------");
    //                        Debug.Log(cityCard1.name);
    //                        Debug.Log(cityCard2.name);
    //                        Debug.Log(cityCard3.name);
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    current_city_cards++;
    //}

    //public void SelectCityCard()
    //{
    //    RaycastHit hit;
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        if (hit.collider.name == "Stop1" || hit.collider.name == "Stop2" || hit.collider.name == "Stop3")
    //        {
    //            isUsingStop = true;
    //            Debug.Log("STOP CARD USED!!");
    //        }

    //        else if (hit.collider.name == "VIP1" || hit.collider.name == "VIP2")
    //        {
    //            isUsingVipCard = true;
    //            Debug.Log("VIP CARD USED!!");
    //        }

    //        else if (hit.collider.name == "Security")
    //        {
    //            Debug.Log("SECURITY CARD USED");
    //        }

    //        else if (hit.collider.name == "Filming1" || hit.collider.name == "Filming2")
    //        {
    //            isUsingFilmingCard = true;
    //            Debug.Log("FILMING CARD USED!!");
    //        }

    //        else if (hit.collider.name == "Subway1" || hit.collider.name == "Subway2" || hit.collider.name == "Subway3")
    //        {
    //            //TODO: Si CUALQUIER posición del token del player coincide con parada de metro, activa esto (posiciones: 9, 13, 16)
    //            isUsingSubwayCard = true;
    //            Debug.Log("SUBWAY CARD USED!!");
    //        }
    //    }
    //    current_city_cards--;
    //}

    //public void UseCityCard(List<GameObject> squares, GameObject unavailableSquareToken, GameObject cones)
    //{
    //    RaycastHit hit;
    //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    //    string colliderName;
    //    int id = 0;
    //    if (Physics.Raycast(ray, out hit))
    //    {
    //        colliderName = hit.collider.name;
    //        for (id = 0; id < squares.Count; id++)
    //        {
    //            if (squares[id].name == colliderName && turnsFilmingCard > 0)
    //            {
    //                //FILMING CARD
    //                if (isUsingFilmingCard)
    //                {
    //                    board[id] = -3;
    //                    unavailableSquareToken.transform.position = squares[id].transform.position;
    //                    Debug.Log(colliderName + " is not accessible during " + turnsFilmingCard);
    //                    //TODO: cuando decrece la variable de turnos??????
    //                }

    //                //SUBWAYCARD
    //                else if (isUsingSubwayCard)
    //                {
    //                    if (id == 9 || id == 13 || id == 16)
    //                    {
    //                        //current_token.gameObject.transform.position = squares[id].transform.position;
    //                        Debug.Log("Player teleported to " + colliderName);
    //                        //TODO: en este if tiene que teletransportar la posición del token que quiere mover
    //                    }
    //                }
    //                else if (isUsingVipCard)
    //                {
    //                    Debug.Log("Using VIP Card");
    //                    if (board[id] == -3)
    //                    {
    //                        unavailableSquareToken.transform.position = new Vector3(28, 10, -7); /*starting point*/
    //                        Debug.Log("Restriction removed!!");
    //                        board[id] = -2;
    //                    }
    //                }
    //                else if (isUsingStop)
    //                {
    //                    if (board[id] == -3)
    //                    {
    //                        cones.transform.position = squares[id].transform.position + new Vector3(-0.63f, -0.08f, -0.58f);
    //                        unavailableSquareToken.transform.position = new Vector3(28, 10, -7); /*starting point*/
    //                        board[id] = -2;
    //                        Debug.Log("Cones colocated on " + squares[id].name + " square, UNAVAILABLE ONE");
    //                    }
    //                    else
    //                    {
    //                        Debug.Log("Cannot put cones on this AVAILABLE SQUARE");
    //                    }
    //                }
    //            }
    //        }
    //    }
    //    if (turnsFilmingCard == 0)
    //        board[id] = -2;

    //    isUsingFilmingCard = false;
    //    isUsingSubwayCard = false;
    //    isUsingVipCard = false;
    //    isUsingStop = false;
    //}
}
