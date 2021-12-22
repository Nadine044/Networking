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
        //
        public List<int> client_cards = new List<int>();
        public List<Token> tokens_list = new List<Token>();
        public int tokencounter = 0;
        public void CreateToken (int id, string name, string first_stop, string final_dst)
        {
            Token t = new Token();

            t.identifier_n = id;
            t.name = name;
            t.first_stop = first_stop;
            t.final_dst = final_dst;

            tokens_list.Add(t);
        }

        //maybe struct is better?¿
        public class Token //Each player has 3 tokens
        {
            public int identifier_n;
            public string first_stop; //for now, maybe better to be a number but we'll see :)
            public string final_dst; //for now, maybe better to be a number but we'll see :)
            public string name;
        }
    }

    int turn_counter=0;
    int current_clients = 0;
    private static EventWaitHandle waithandle = new AutoResetEvent(false);

    List<Client> client_list = new List<Client>();

    //CARDS
    List<int> cards_for_both = new List<int>(); //each number is connected to a type of card
    int rand_tmp = -1;
    int welcome_callback_counter = 0;
    private const int max_token_per_client = 3;
    //board
    int[] board = new int[25];

    bool closingApp = false;
    //Debugin
    public Text logtext;
    void Start()
    {
        //GETTING RANDOM CARDS
        for (int i = 0; i < 6; i++)
        {
            cards_for_both.Add(UnityEngine.Random.Range(0, 24));
        }
        List<int> result = cards_for_both.Distinct().ToList();
        while (result.Count < 6)
        {
            //while the 6 numbers arent different keep making randoms
            cards_for_both.Add(UnityEngine.Random.Range(0, 24));
            result = cards_for_both.Distinct().ToList();
        }
        //finally the 6 cards to give the players
        cards_for_both = result;
        //END GETTING RANDOM CARDS

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
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 26003);
        StartThreadingFunction(Connect);
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

    void Reconnect(Client c)
    {
        List<object> tmp_list = new List<object>();
        tmp_list.Add(c);
        tmp_list.Add(socket); //TODO CHANGE this dirty pas only client clas
        socket.Listen(1);
        socket.BeginAccept(new AsyncCallback(ReconnectCallback), tmp_list);
    }

   /// <summary>
   /// Called when the client has reconnected
   /// </summary>
   /// <param name="ar">we pass client class & server socket as objects</param>
    void ReconnectCallback(IAsyncResult ar)
    {
        List<object> tmp_list = (List<object>)ar.AsyncState;
        Client c = (Client)tmp_list[0];
        Socket sckt = (Socket)tmp_list[1];

        c.client_socket = sckt.EndAccept(ar);

        //get a int list of the client tokens
        List<int> tmp_token_list = new List<int>();
        for (int j = 0; j < c.tokens_list.Count(); j++)
        {
            tmp_token_list.Add(c.tokens_list[j].identifier_n);
        }

        if (c.client_turn) //let the player make the move 
        {
            c.client_turn = false;
            if (turn_counter < 6) //seting up games
            {
                byte[] b = Serialize(-4, board,tmp_token_list.ToArray(), cards_for_both[turn_counter]);
                c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(ReconnectSendCallback), c); 
            }
            else if(turn_counter == 6) //i think it never will be 6 but lest put the case
            {
                Debug.Log("reconnect in turn 6");
                //byte[] b = Serialize(3, "your turn reconnect", board, true, cards_for_both[turn_counter - 1]);
                //c.client_socket.BeginSend()
            }
            else if(turn_counter >6)
            {
                int tmp_token_counter = c.tokencounter; //-1 it's because the counter already augmented after we told the player it was his move,
                //but as the player disconnected we couldn't restore it properly
                if (c.tokencounter < 0)
                {
                    tmp_token_counter = 0;
                }
                byte[] b = Serialize(-3, board, tmp_token_list.ToArray(),cards_for_both[c.tokencounter]);
                c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(ReconnectSendCallback), c);
            }
        }

        else if(!c.client_turn) //first update the reconnecting player (how the board is), then tell the waiting player he can make the play
        {
            byte[] b = Serialize(-2, board, tmp_token_list.ToArray());
            c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(ReconnectUpdatePLayerBoardCallback), c);
        }
    }

    /// <summary>
    /// The waiting player for reconnection does his turn
    /// </summary>
    /// <param name="ar"></param>
    void ReconnectUpdatePLayerBoardCallback(IAsyncResult ar)
    {
        Client c = (Client)ar.AsyncState;
        Thread t = new Thread(OnUpdateClient);
        t.Start(c);

        for(int i =0; i < client_list.Count(); i++)
        {
            if(client_list[i] != c)
            {
                byte[] b;
                //other player turn
                if (turn_counter < 6) //look that card id is okay
                {
                    int card_id = cards_for_both[turn_counter];
                    b = Serialize(1, "waiting for my turn setup yuhu", board, true, card_id);
                    if (client_list[i].client_socket.Connected)
                    {
                        client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, UpdateToOtherClientCallback, c);
                    }
                }
                else if (turn_counter >=6) //look tha
                {
                    if(client_list[i].tokencounter >=max_token_per_client)
                    {
                        client_list[i].tokencounter = 0;
                    }
                    b = Serialize(3, "waiting for my tunr yuhu", board, true,
                        client_list[i].tokens_list[client_list[i].tokencounter].identifier_n);

                    if(client_list[i].client_socket.Connected)
                    {
                        client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, UpdateToOtherClientCallback, c);
                    }
                }
            }
        }
    }


    void ReconnectSendCallback(IAsyncResult ar)
    {
        Client c = (Client)ar.AsyncState;
        c.client_socket.EndAccept(ar);

        Thread t = new Thread(OnUpdateClient);
        t.Start(c);
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

    private void ConnectCallback(IAsyncResult ar) //here we must add the two clients & wait for clients response
    {
        //here we must check that the 2 clients are connected
        //if not we tell the other client that must wait and his input is disabled 

        Client client = new Client();
        client_list.Add(client);

        client.client_socket = socket.EndAccept(ar);


        //TODO WAIT 4 ALL PLAYERS TO CONNECT

        byte[] b = Serialize(0, "wait for the other player",board,false,-1); //client =0 means its not decided who is who TEMPORAL
        //SendPackage(b, client.client_socket);

        client.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(WelcomeCallback), client.client_socket);
    }
    #endregion

    private void WelcomeCallback(IAsyncResult ar) //bit dirty, may be changed
    {
        welcome_callback_counter++;

        if (client_list.Count == 2 && welcome_callback_counter == 2)
            SelectFirstPlayerTurn();
    }

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


    void OnUpdateClient(object obj)
    {
        Client client = (Client)obj;
        while (!client.end_connexion)
        {
            client.recieveDone.Reset();

            if (client.end_connexion)
                break;

            if (client.client_socket.Connected) //to avoid crashes if the socket disconects 
                client.client_socket.BeginReceive(client.buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnUpdateCallbackClient), client);

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
        catch(SocketException e)
        {
            Debug.LogWarning("Couldn't close socket connection with client " + client + " " + e);
        }

        if (!closingApp)//we are not closing the application, just the client disconnected
        {
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
    }

    void SelectFirstPlayerTurn()
    {
        Debug.Log("Selecting first player");
        int card_id = cards_for_both[turn_counter];
        var r = new System.Random();
        int rand_int = r.Next(0, 1);
        byte[] b = Serialize(1, turn_counter + "giving cards", board, true, card_id); //client 1
        client_list[rand_int].client_turn = true;
        
        Action func = () =>
        {
            client_list[rand_int].CreateToken(
           cards_for_both[turn_counter],
           GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
           GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUp,
           GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destiny
           );
            Debug.Log("Token created & turn up");
            turn_counter++;
        };
        QueueMainThreadFunction(func);

        //TODO make sure the client socket is connected before we send anything to him otherwise this will crash
        if (client_list[rand_int].client_socket.Connected)
        {
            client_list[rand_int].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SetUpGameCardsClient), rand_int);
        }
        else
        {
            //TODO ENTER The wait for reconnect state
        }
        for (int i=0; i < client_list.Count(); i++)
        {
            Debug.Log("Starting update thead " + i);
            Thread t = new Thread(OnUpdateClient);
            t.Start(client_list[i]);
        }
    }

    void OnUpdateCallbackClient(IAsyncResult ar)
    {
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
                switch (package.index)
                {
                    case 1: //means we have to check what the turn counter is and tell the other client which card must he use and board update
                        for (int i = 0; i < client_list.Count(); i++)
                        {
                            if (client_list[i] != client)
                            {
                                //PETA torna a entrar
                                int card_id = 0;
                                //the other client that we must update
                                if (turn_counter <= 5)
                                    card_id = cards_for_both[turn_counter];

                                rand_tmp = i;
                                int _enter = turn_counter;
                                Action func = () =>
                                {
                                    int tmp = _enter;
                                    if (tmp > 5)
                                        tmp = 5;
                                    client_list[rand_tmp].CreateToken(
                                       cards_for_both[tmp],
                                       GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
                                       GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUp,
                                       GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destiny
                                       );
                                };
                                QueueMainThreadFunction(func);
                                turn_counter++;
                                byte[] b = Serialize(1, "your turn client 2", board, true, card_id);
                                
                                //We send data to the other client, and let him send us back his input
                                if(client_list[i].client_socket.Connected)
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
                        break;
                    case 3: //means all is setted up
                        for (int i = 0; i < client_list.Count(); i++)
                        {
                            if (client_list[i] != client)
                            {
                                byte[] b = Serialize(3, "your turn nº " + turn_counter, board, true,
                                    client_list[i].tokens_list[client_list[i].tokencounter].identifier_n);
                                int tmp_tokencounter = client_list[i].tokencounter;

                                client_list[i].tokencounter++;
                                if (client_list[i].tokencounter >= max_token_per_client)
                                {
                                    client_list[i].tokencounter = 0;
                                }

                                turn_counter++;
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
                        break;
                }
                client.recieveDone.Set();
            }
        }

        else
        {
                client.end_connexion = true;
                client.recieveDone.Set();
        }
    }

    void UpdateToOtherClientCallback(IAsyncResult ar)
    {
        Client client = (Client)ar.AsyncState;
        client.client_turn = true;       
    }

    void SetUpGameCardsClient(IAsyncResult ar)
    {
        Debug.Log("SetupCallback");
    }

    /// <summary>
    /// Tell the connected client to wait the reconnection of the other
    /// </summary>
    /// <param name="c"></param>
    void WaitForClientReconnection(Client c)
    {
        if(c.client_socket.Connected)
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

    void WaitForClientReconnectionCallback(IAsyncResult ar)
    {

        //what should we do here
    }
}
