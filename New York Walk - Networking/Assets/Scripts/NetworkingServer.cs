using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using System.Net;
using System.Threading;
using System;
using System.IO;

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
    List<int> cards_n = new List<int>(); //each number is connected to a type of card

    //board
    int[] board = new int[25];
    void Start()
    {
        for(int i =0; i<25;i++)
        {
            cards_n.Add(i);
        }

        for(int i=0; i< 25; i++)
        {
            board[i] = 0;
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

    void CloseConnection()
    {
        socket.Close();
        Debug.Log("Closing Server");
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

        //when the 2 clients are connected we have to launch a manager thread

    }
    #endregion


    void SetUpGameCards(IAsyncResult ar)
    {
        int client_n = (int)ar.AsyncState;

        Debug.Log("Other client response");
        //send the card to the other player

        if (turn_counter < 6)
        {
            client_list[client_n].client_socket.BeginReceive(client_list[client_n].buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnSetUpCallback), client_list[client_n]);
            Debug.Log("SettingUp to other player");
        }
        else if (turn_counter == 6)//now we change game phase, each player has 3 cards and has placed its token
        {
            //TODO updateja la board del altre client --> per aixo no funciona
            client_list[client_n].client_socket.BeginReceive(client_list[client_n].buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnUpdateCallback), client_list[client_n]);
            Thread t = new Thread(OnUpdate);
            t.Start(client_n); //the following client
        }

    }

    void OnSetUpCallback(IAsyncResult ar)
    {
        Client client = (Client)ar.AsyncState;
        int bytesRead = client.client_socket.EndReceive(ar);
        if(bytesRead>0)
        {
            Package package = Deserialize(client.buffer);

            board = package.board_array;
            int tmp_int = 0;
            foreach(Client c in client_list)
            {
                if(c != client)
                {
                    Debug.Log("Sending update to other client & wait for response");
                    int card_id = GetComponent<CardsServerSide>().cards_forboth[turn_counter];
                    //Now we send the update to the other client and give him the card
                    byte[] b = Serialize(1, turn_counter + "give you card", board, true, card_id);
                    turn_counter++;
                    c.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SetUpGameCards), tmp_int);
                    c.CreateToken(
                        card_id,
                        GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
                        GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUp,
                        GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destiny
                        );
                }
                tmp_int++;
            }
        }
    }
    void DecideFirst() 
    {
        Debug.Log("DecideFirst");

        int card_id = GetComponent<CardsServerSide>().cards_forboth[turn_counter];
        int tmp = UnityEngine.Random.Range(0, 2);
        byte[] b = Serialize(1, turn_counter + "giving cards", board, true, card_id); //client 1
        turn_counter++;

        client_list[tmp].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SetUpGameCards), tmp);

        client_list[tmp].CreateToken(
            card_id,
            GetComponent<JSONReader>().playableCitizenList.citizens[card_id].citizen,
            GetComponent<JSONReader>().playableCitizenList.citizens[card_id].pickUp,
            GetComponent<JSONReader>().playableCitizenList.citizens[card_id].destiny
            );
        //Action DecideTurnFunc = () =>
        //{

        //    //get cards info

        //};
        //QueueMainThreadFunction(DecideTurnFunc);
    }

    //REMOVE THIS FUNCTION AND USE A NON THREAD BASED FUNCITON
    //here we manage both clients
    void OnUpdate(object c) //TODO here or in another thread we must check that the clients are connected, maybe create a secondary socket or another thread that given a certain time waits for a client response (given the index) 
    {
        int client_n = (int)c;

        Debug.Log("OnUpdate");
        Client client = client_list[client_n];

        while(!client.end_connexion)
        {
            client.recieveDone.Reset();

            if (client.end_connexion)
                break;

            if(client.client_socket.Connected) //to avoid crashes if the socket disconects 
                client.client_socket.BeginReceive(client.buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnUpdateCallback), client);

            else
            {
                break;
            }
            client.recieveDone.WaitOne();
        }


        //try
        //{
        //    client.client_socket.Close();
        //}
        //catch(SystemException e)
        //{
        //    Debug.LogWarning("Try close client socket while it's already closed " + e);
        //}
    }
    private void SendOtherPlayerCallback(IAsyncResult ar)
    {
        Client client = (Client)ar.AsyncState;

        //the other socket client waits for the client move to be done and send to server
        //not use a thread maybe
        OnUpdateNoThread(client);

    }

    void OnUpdateNoThread(Client c)
    {
        if (c.client_socket.Connected)
        {
            c.client_socket.BeginReceive(c.buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnUpdateCallback), c);
        }

        else
        {
            Debug.Log("the other player isn't connected anymore");
        }
    }
    private void OnUpdateCallback(IAsyncResult ar)
    {
        Client client = (Client)ar.AsyncState;

        int bytesRead = client.client_socket.EndReceive(ar);

        if (bytesRead >0)
        {


            Package package = Deserialize(client.buffer);
            board = package.board_array;
            int tmp = 0;
            foreach(Client c in client_list) //we send a package to the new player
            {
                if(c == client)
                {
                    Debug.Log("OnUpdateCallback sending package to other player");
                    byte[] b = Serialize(3, "your turn, lets go!", board,true,c.tokens_list[c.tokencounter].identifier_n);

                    c.tokencounter++;
                    if (c.tokencounter > 3)
                        c.tokencounter = 0;
                    //now we send to the other client. What about the  begin recieve update of this new client?
                    c.client_socket.BeginSend(b, 0, Client.BufferSize, 0, new AsyncCallback(SendOtherPlayerCallback), client_list[tmp]);
                    c.end_connexion = true;//TODO DIRTY

                }
                tmp++;
            }
            //what about this
            client.recieveDone.Set();
        }
        else
        {
            try
            {
                client.client_socket.Close();
            }
            catch (SystemException e)
            {
                Debug.LogWarning("Try close client socket while it's already closed " + e);
            }
            client.recieveDone.Set();
            client.end_connexion= true;
            //close connection with client
        }
    }

    #region Send
    private void SendPackage(byte[] b,Socket _socket)
    {
        _socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SendCallback), _socket);
    }

    private void WelcomeCallback(IAsyncResult ar) //bit dirty, may be changed
    {
        if (client_list.Count == 2)
            DecideFirst();
    }
    private void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Send Package to client");

    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            foreach(Client c in client_list)
            {
                try
                {
                    c.client_socket.Close();
                }
                catch(SocketException e)
                {
                    Debug.Log(e);
                }
            }
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
}
