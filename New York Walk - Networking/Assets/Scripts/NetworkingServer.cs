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

        public class Token
        {

            public int identifier_n;
            public int current_dst;
            public int first_stop;
            public int final_dst;
        }
    }

    // Start is called before the first frame update
    int current_clients = 0;
    private static EventWaitHandle waithandle = new AutoResetEvent(false);

    List<Client> client_list = new List<Client>();

    //CARDS
    List<int> cards_n = new List<int>(); //each number is connected to a type of card

    //board
    int[] board = new int[25];
    void Start()
    {
        for(int i =0; i<24;i++)
        {
            cards_n.Add(i);
        }

        for(int i=0; i< 25; i++)
        {
            board[i] = 0;
        }
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 26002);

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

        byte[] b = Serialize(0, "wait for the other player",board,0); //client =0 means its not decided who is who TEMPORAL
        //SendPackage(b, client.client_socket);

        client.client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(WelcomeCallback), client.client_socket);

        //when the 2 clients are connected we have to launch a manager thread

    }
    #endregion


    void SetUpGame()
    {
        //make a random to choose a card

        var rand = new System.Random();
        int tmp = rand.Next(0, 2);
        //int tmp = UnityEngine.Random.Range(0, 1);

        board[0] = 1; //client 1
        board[4] = 2; //client 2;

        if (tmp == 0)
        {
            client_list[0].client_type = 1;
            client_list[1].client_type = 2;
            byte[] b1 = Serialize(1, "your turn",board, client_list[0].client_type); //client 1
            SendPackage(b1, client_list[0].client_socket);
            byte[] b2 = Serialize(2, "not your turn",board, client_list[1].client_type);//client 2
            SendPackage(b1, client_list[1].client_socket);
        }
        else if(tmp ==1)
        {
            client_list[0].client_type = 2;
            client_list[1].client_type = 1;
            byte[] b1 = Serialize(1, "your turn",board, client_list[0].client_type); //client 2
            SendPackage(b1, client_list[1].client_socket);
            byte[] b2 = Serialize(2, "not your turn",board, client_list[1].client_type);//client 1
            SendPackage(b1, client_list[0].client_socket);
        }

        //now we wait for clients packages

        foreach (Client c in client_list)
        {
            Thread t = new Thread(OnUpdate);
            t.Start(c);
        }
    }
    //here we manage both clients
    void OnUpdate(object c)
    {
        Client client = (Client)c;
        while(!client.end_connexion)
        {
            client.recieveDone.Reset();

            if (client.end_connexion)
                break;

            client.client_socket.BeginReceive(client.buffer, 0, Client.BufferSize, 0, new AsyncCallback(OnUpdateCallback), client);

            client.recieveDone.WaitOne();
        }

    }

    private void OnUpdateCallback(IAsyncResult ar)
    {
        Client client = (Client)ar.AsyncState;

        int bytesRead = client.client_socket.EndReceive(ar);

        if (bytesRead >0)
        {
            //replicate what the player has done and send it to the other client
            //also change states

            Package package = Deserialize(client.buffer);
            board = package.board_array;
            foreach(Client c in client_list)
            {
                if(c != client)
                {
                    //now we replicate the move
                    byte[] b = Serialize(1, "your turn, lets go!", board,c.client_type);
                    SendPackage(b, c.client_socket);
                }
                else if(c == client)
                {
                    //tell him that is not his turn
                    byte[] b = Serialize(2, "others turn, we relpicated your move", board,c.client_type);
                    SendPackage(b,c.client_socket);
                }
            }
            client.recieveDone.Set();
        }
        else
        {
            client.client_socket.Close();
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
            StartThreadingFunction(SetUpGame);
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
    }
}
