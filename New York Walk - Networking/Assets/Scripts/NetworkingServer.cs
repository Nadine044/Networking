using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Net;
using System.Threading;
using System;
using System.IO;
using System.Linq;
using UnityEngine.UI;

public class NetworkingServer : Networking
{
    class Client
    {
        public Socket client_socket;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];

        public EventWaitHandle recieveDone = new AutoResetEvent(false);
        public bool end_connexion = false;

        public int client_type = 0;
        public bool client_turn = false;
        public List<int> client_cards = new List<int>();

        public List<Token> tokens_list = new List<Token>();
        public int tokencounter = 0;

        public int win_counter = 0; //if 3 client wins the game
        public string client_name_debug;
      //  public bool showcards = false;

        public void CreateToken (int id, string name, int pickUp, int final_dst)
        {
            Token t = new Token();

            t.identifier_n = id;
            t.name = name;
            t.pickUp = pickUp;
            t.final_dst = final_dst;

            tokens_list.Add(t);
        }
        public class Token //Each player has 3 tokens
        {
            public int identifier_n;
            public int pickUp; //for now, maybe better to be a number but we'll see :)
            public int final_dst; //for now, maybe better to be a number but we'll see :)
            public string name;
            public bool active = true;
        }
    }

    int turn_counter=0;
    int current_clients = 0;
    int welcome_callback_counter = 0;
    List<Client> client_list = new List<Client>();

    //CARDS
    List<int> cards_for_both = new List<int>(); //each number is connected to a type of card
    int rand_tmp = -1;
    private const int max_token_per_client = 3;
    List<int> exclusive_cards = new List<int>(); //2 cards after the set up stage to lock 2 different positions
    static readonly int max_cards_to_give = 8;
    //board
    int[] board = new int[25];

    bool closingApp = false;
    //Debugging
    public Text logtext;
    public Text board_array_text;
    void Start()
    {
        GenerateRandomCards();
        //DEBUGING
        logtext.text = string.Empty;
        for(int i = 0; i < cards_for_both.Count(); i++)
        {
            logtext.text += ", " + cards_for_both[i];
        }
        for (int i=0; i< 25; i++)
        {
            board[i] = -2;
        }
        UpdateDebugBoardText();
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 26003);
        StartThreadingFunction(Connect);
    }

    void GenerateRandomCards()
    {
        for (int i = 0; i < 6; i++)
        {
            cards_for_both.Add(UnityEngine.Random.Range(0, 24));
        }
        List<int> result = cards_for_both.Distinct().ToList();
        while (result.Count < max_cards_to_give)
        {
            //while the 6 numbers aren't different keep making randoms
            cards_for_both.Add(UnityEngine.Random.Range(0, 24));
            result = cards_for_both.Distinct().ToList();
        }
        cards_for_both = result.GetRange(0, 6);
        exclusive_cards = result.GetRange(6, 2);
    }

    void UpdateDebugBoardText()
    {
        if(board_array_text != null)
        {
            board_array_text.text = string.Empty;

            for(int i = 0; i < board.Length; i++)
            {
                board_array_text.text += board[i].ToString() +", ";
            }
        }
    }

    #region Connect
    void Connect()
    {
        socket.Bind(ipep);
        //maximum 2 clients at the queu entry
        socket.Listen(2);

        while(current_clients < 2)
        {
            socket.BeginAccept(new AsyncCallback(ConnectCallback), socket);
            current_clients++;
        }
    }

    void CloseConnection()
    {
        //maybe need to abort all threads
        foreach(Client client in client_list)
        {
            if(client.client_socket.Connected)
            {
                try
                {
                    client.client_socket.Close();
                }
                catch(SocketException e)
                {
                    Debug.LogWarning("Couldn't close socket with client " + e);
                }
            }
        }
        try
        {
            socket.Close();
            Debug.Log("Closing Server");
        }
        catch (SocketException e)
        {
            Debug.LogWarning("Cound't close the server socket " + e);
        }
    }

    /// <summary>
    /// Asyncfunction called when the client connects to the server
    /// </summary>
    /// <param name="ar"></param>
    private void ConnectCallback(IAsyncResult ar) 
    {
        if(ar == null)
        {
            return;
        }
        Client client = new Client();
        try
        {
            client.client_socket = socket.EndAccept(ar);
        }
        catch(ObjectDisposedException e)
        {
            Debug.Log("Caught " + e.Message);
            return;
        }
        if (client.client_socket.Connected)
        {
            client_list.Add(client);
            byte[] b = Serialize(0, "wait for the other player", board, -1); //client =0 means its not decided who is who TEMPORAL
            client.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(WelcomeCallback), client.client_socket);
        }
        else
        {
            current_clients--;
        }
    }

    private void WelcomeCallback(IAsyncResult ar) //bit dirty, may be changed
    {
        //for a safety double check 
        welcome_callback_counter++;
        if (client_list.Count == 2 && welcome_callback_counter == 2)
            SelectFirstPlayerTurn();
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            closingApp = true;
            CloseConnection();
            Application.Quit();
        }
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc;
            functionsToRunInMainThread.TryDequeue(out someFunc);

            //Now run it;
            someFunc();
        }
    }

    #region ClientUpdate
    void OnUpdateClient(object obj)
    {
        Client client = (Client)obj;
        while (!client.end_connexion)
        {
            client.recieveDone.Reset();
            if (client.end_connexion)
            {
                break;
            }
            if (client.client_socket.Connected) //to avoid crashes if the socket disconects 
            {
                client.client_socket.BeginReceive(client.buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnUpdateCallbackClient), client);
            }
            else
            {
                break;
            }
            client.recieveDone.WaitOne();
        }
        try
        {
            client.client_socket.Close();
            Debug.Log("Closing socket with client " + client);

        }
        catch (SocketException e)
        {
            Debug.LogWarning("Couldn't close socket connection with client " + client + " " + e);
        }
    }

    void SelectFirstPlayerTurn()
    {
        Debug.Log("Selecting first player");
        int card_id = cards_for_both[turn_counter];
        var r = new System.Random();
        int f_rand = r.Next(0, 100); 
        int rand_int;
        if(f_rand %2 ==0)
        {
            rand_int = 1;
        }
        else
        {
            rand_int = 0;
        }
        byte[] b = Serialize(1, turn_counter + "giving cards", board, card_id); 
        client_list[rand_int].client_turn = true;
        //Action func = () =>
        //{
        //    client_list[rand_int].CreateToken(
        //   cards_for_both[turn_counter],
        //   //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
        //   //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUpID,
        //   //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destinyID
        //   );
        //    Debug.Log("Token created & turn up");
        //    turn_counter++;
        //};
        //QueueMainThreadFunction(func);

        if (client_list[rand_int].client_socket.Connected)
        {
            client_list[rand_int].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SetUpGameCardsClient), rand_int);
        }
        else
        {
            //TODO ENTER The wait for reconnect state
        }
        for (int i = 0; i < client_list.Count(); i++)
        {
            if (i == 0)
                client_list[i].client_name_debug = "A";
            else client_list[i].client_name_debug = "B";
            Debug.Log("Starting update thead " + i);
            Thread t = new Thread(OnUpdateClient);
            t.Start(client_list[i]);
        }
    }

    void OnUpdateCallbackClient(IAsyncResult ar)//TODO POLISH CALLBACK
    {
        if (ar == null)
        {
            Client c = (Client)ar.AsyncState;
            Debug.Log("COULD GET THE ASYNC_STATE EVEN IF ASYNCRESULT IS NULL ");
            return;
        }
        Client client = (Client)ar.AsyncState;
        int bytesRead = client.client_socket.EndReceive(ar);
        if (bytesRead > 0)
        {
            //not his turn
            if (!client.client_turn)
            {
                client.recieveDone.Set();
            }
            if (client.client_turn)
            {
                client.client_turn = false;
                Package package = Deserialize(client.buffer);
                board = package.board_array;
                Action updateboard = () => { UpdateDebugBoardText(); };
                QueueMainThreadFunction(updateboard);
                if (turn_counter >= 6)
                {
                    if(package.index != 4)
                        package.index = 3;
                }
                switch (package.index)
                {
                    case 1: //means we have to check what the turn counter is and tell the other client which card must he use and board update
                        for (int i = 0; i < client_list.Count(); i++)
                        {
                            if (client_list[i] != client)
                            {
                                int card_id = 0;
                                //the other client that we must update
                                if (turn_counter <= 5)
                                    card_id = cards_for_both[turn_counter];

                                rand_tmp = i;
                                int _enter = turn_counter;
                                //Action func = () =>
                                //{
                                //    int tmp = _enter;
                                //    if (tmp > 5)
                                //        tmp = 5;
                                //    client_list[rand_tmp].CreateToken(
                                //       cards_for_both[tmp],
                                //       //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
                                //       //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUpID,
                                //       //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destinyID
                                //       );
                                //};
                                //QueueMainThreadFunction(func);
                                byte[] b = Serialize(1, "your turn client 2", board, card_id);

                                //We send data to the other client, and let him send us back his input
                                if (client_list[i].client_socket.Connected)
                                {
                                    client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(UpdateToOtherClientCallback), client_list[i]);
                                }
                                else
                                {
                                    turn_counter--;
                                    //Here we must pause the game until the client reconnects back
                                }
                            }
                        }
                        break;//////////

                    case 3: //means all is setted up
                        for (int i = 0; i < client_list.Count(); i++)
                        {
                            if (client_list[i] != client)
                            {
                                //first check if the token has finished
                                CheckTokenActive(client_list[i]);

                                byte[] b = Serialize(3, "your turn nº " + turn_counter, board,
                                    client_list[i].tokens_list[client_list[i].tokencounter].identifier_n);

                                int tmp_tokencounter = client_list[i].tokencounter;
                                client_list[i].tokencounter++;
                                if (client_list[i].tokencounter >= max_token_per_client) //reset token counter
                                {
                                    client_list[i].tokencounter = 0;
                                }
                                if (client_list[i].client_socket.Connected)//We send data to the other client, and let him send us back his input
                                {
                                    client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(UpdateToOtherClientCallback), client_list[i]);
                                }
                                else
                                {
                                    client_list[i].tokencounter = tmp_tokencounter;
                                    turn_counter--;
                                    //Here we must pause the game until the client reconnects back
                                }
                            }
                        }
                        break;//////////

                    case 4:
                        //First we check that is correct
                        for(int j =0; j < client.tokens_list.Count(); j++)
                        {
                            if(client.tokens_list[j].identifier_n == package.card)
                            {
                                client.tokens_list[j].active = false;
                                client.win_counter++;
                            }
                        }
                        //check if 3 tokens arrived and give win
                        if(client.win_counter == 3)
                        {
                            for(int i =0; i < client_list.Count(); i++)
                            {
                                if(client_list[i] != client)
                                {
                                    byte[] b = Serialize(5, board);
                                    client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(FinishGameCallback), client_list[i]);
                                    client_list[i].end_connexion = true;
                                    client_list[i].recieveDone.Set();
                                }
                            }
                        }
                        else //not win
                        {
                            //now we tell the other client
                            for (int i = 0; i < client_list.Count(); i++)
                            {
                                if(client_list[i] != client)
                                {
                                    CheckTokenActive(client_list[i]);
                                    byte[] b = Serialize(3, "your turn nº " + turn_counter, board,
                                       client_list[i].tokens_list[client_list[i].tokencounter].identifier_n);

                                    int tmp_tokencounter = client_list[i].tokencounter;
                                    client_list[i].tokencounter++;
                                    if (client_list[i].tokencounter >= max_token_per_client) //reset token counter
                                    {
                                        client_list[i].tokencounter = 0;
                                    }
                                    if (client_list[i].client_socket.Connected)//We send data to the other client, and let him send us back his input
                                    {
                                        client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(UpdateToOtherClientCallback), client_list[i]);
                                    }
                                    else
                                    {
                                        client_list[i].tokencounter = tmp_tokencounter;
                                        turn_counter--;
                                        //Here we must pause the game until the client reconnects back
                                    }
                                }
                            }
                        }
                        break;//////////
                }
                client.recieveDone.Set();
            }
        }

        else //no bytes recieved, means the client disconnected or our socket closed
        {
            //passes twice problem heere
            if (!closingApp)//we are not closing the application, just the client disconnected
            {
                turn_counter--;
                for (int i = 0; i < client_list.Count(); i++)
                {
                    if (client_list[i] != client)
                    {
                        WaitForClientReconnection(client_list[i]); //we tell the other client to wait
                    }
                }
                //wait for the client to reconnect
                Reconnect(client);
            }
            client.end_connexion = true;
            client.recieveDone.Set();
        }
    }

    void CheckTokenActive(Client c)
    {
        if (!c.tokens_list[c.tokencounter].active)
        {
            c.tokencounter++;
            if(c.tokencounter >= 3)
            {
                c.tokencounter = 0;
            }
            if(!c.tokens_list[c.tokencounter].active)
            {
                Debug.Log("recursive check");
                CheckTokenActive(c);
            }
        }
    }

    void FinishGameCallback(IAsyncResult ar)//TODO TELL THE USER THAT THE GAME IS OVER AND HE MUST EXIT THE APPLICATION
    {
        //CloseConnection();
        //Application.Quit();
    }
    void UpdateToOtherClientCallback(IAsyncResult ar)
    {
        turn_counter++;
        Client client = (Client)ar.AsyncState;
        client.client_turn = true;
    }

    void SetUpGameCardsClient(IAsyncResult ar)
    {
        Debug.Log("SetupCallback");
    }
    #endregion

    #region Reconnections
    void Reconnect(Client c)
    {
        List<object> tmp_list = new List<object>();
        tmp_list.Add(c);
        tmp_list.Add(socket);
        socket.Listen(1);
        socket.BeginAccept(new AsyncCallback(ReconnectCallback), tmp_list);
    }

    /// <summary>
    /// Called when the client has reconnected
    /// </summary>
    /// <param name="ar">we pass client class & server socket as objects</param>
    void ReconnectCallback(IAsyncResult ar)
    {
        if(ar == null)
        {
            return;
        }
        List<object> tmp_list = (List<object>)ar.AsyncState;
        Client c = (Client)tmp_list[0];
        Socket sckt = (Socket)tmp_list[1];

        c.client_socket = sckt.EndAccept(ar);
        c.end_connexion = false;
        //get a int list of the client tokens
        List<int> tmp_token_list = new List<int>();

        for (int j = 0; j < c.tokens_list.Count(); j++)
        {
            tmp_token_list.Add(c.tokens_list[j].identifier_n);
        }

        if (c.client_turn) //let the player make the move 
        {
            if (turn_counter < 6) //seting up games stage
            {
                //We must remove the last item because it was added on the readcallback 
                //but the client disconnected so the other client doesn't know about it 
                c.tokens_list.RemoveAt(c.tokens_list.Count() - 1);
                tmp_token_list.RemoveAt(tmp_token_list.Count - 1);

                int card_id = cards_for_both[turn_counter];
                byte[] b = Serialize(-4, board, tmp_token_list.ToArray(), card_id);
                int tmp = turn_counter;
                //Action func = () =>
                //{
                //    c.CreateToken(
                //        cards_for_both[tmp],
                //        //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
                //        //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUpID,
                //        //GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destinyID
                //        );
                //};
                //QueueMainThreadFunction(func);
                c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(ReconnectSendCallback), c);
            }
            else if (turn_counter >= 6)
            {
                //-1 it's because the counter already augmented after we told the player it was his move,
                //but as the player disconnected we couldn't restore it properly
                c.tokencounter--;
                if (c.tokencounter < 0)
                {
                    c.tokencounter = 0;
                }
                CheckTokenActive(c); //we set the turn at the index where the tokens hasn't arrived to destiny

                byte[] b = Serialize(-3, board, tmp_token_list.ToArray(), c.tokens_list[c.tokencounter].identifier_n,c.win_counter);
                c.tokencounter++;
                c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(ReconnectSendCallback), c);
            }
        }

        else if (!c.client_turn) //first update the reconnecting player (how the board is), then tell the waiting player he can make the play
        {
            byte[] b = Serialize(-2, board, c.win_counter, tmp_token_list.ToArray()); //where do we say that wincounter is
            c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(ReconnectUpdatePlayerBoardCallback), c);
        }
    }

    /// <summary>
    /// Async function called when the reconnecting client has been updated about the game state & data
    /// In this fucntion the waiting client resume his turn 
    /// </summary>
    /// <param name="ar"></param>
    void ReconnectUpdatePlayerBoardCallback(IAsyncResult ar)
    {
        Client c = (Client)ar.AsyncState;
        Thread t = new Thread(OnUpdateClient);
        t.Start(c);
        for (int i = 0; i < client_list.Count(); i++)
        {
            if (client_list[i] != c)
            {
                byte[] b;

                if (turn_counter < 6) //for set up stage
                {
                    //resume play state;
                    b = Serialize(-5);
                    if (client_list[i].client_socket.Connected)
                    {
                        client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, UpdateToOtherClientCallback, c);
                    }
                }
                else if (turn_counter >= 6) //for advance game stage
                {
                    b = Serialize(-5);
                    if (client_list[i].client_socket.Connected)
                    {
                        client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, UpdateToOtherClientCallback, c);
                    }
                }
            }
        }
    }
    void ReconnectSendCallback(IAsyncResult ar)
    {
        turn_counter++;

        Client c = (Client)ar.AsyncState;
        c.client_socket.EndAccept(ar);
        //c.client_turn = false;
        Thread t = new Thread(OnUpdateClient);
        t.Start(c);
    }

    /// <summary>
    /// Tell the connected client to wait the reconnection of the other
    /// </summary>
    /// <param name="c"></param>
    void WaitForClientReconnection(Client c)
    {
        if (c.client_socket.Connected)
        {
            byte[] b = Serialize(-1);
            //Send wait state
            c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(WaitForClientReconnectionCallback), c);
        }
        else
        {
            //means the other client isn't connected so we must wait for his connection too 
            //or shutdown the aplication maybe
        }
    }
    /// <summary>
    /// Async function called when the waiting client recieved our order
    /// </summary>
    /// <param name="ar"></param>
    void WaitForClientReconnectionCallback(IAsyncResult ar)
    {
        Debug.Log("Waiting client recieved package send");
    }
    #endregion
}
