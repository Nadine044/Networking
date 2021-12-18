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
    //board
    int[] board = new int[25];

    //Debugin
    public Text logtext;
    void Start()
    {
        //GETTING RANDOM CARDS
        for (int i = 0; i < 6; i++)
        {
            cards_for_both.Add(UnityEngine.Random.Range(0, 25));
        }
        List<int> result = cards_for_both.Distinct().ToList();
        while (result.Count < 6)
        {
            //while the 6 numbers arent different keep making randoms
            cards_for_both.Add(UnityEngine.Random.Range(0, 25));
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

        client_list[rand_int].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(SetUpGameCardsClient), rand_int);
        
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

        //not his turn
        if (!client.client_turn)
        {
            
            client.recieveDone.Set();
        }
        if(client.client_turn)
        {
            client.client_turn = false;
            Package package = Deserialize(client.buffer);

            board = package.board_array;
            switch(package.index)
            {
                case 1: //means we have to check what the turn counter is and tell the other client which card must he use and board update

                    for(int i=0; i < client_list.Count(); i++)
                    {
                        if(client_list[i] != client)
                        {
                            //PETA torna a entrar
                            int card_id=0;
                            //the other client that we must update
                            if(turn_counter <= 5)
                               card_id = cards_for_both[turn_counter];

                            rand_tmp = i;
                            Action func = () =>
                            {
                                int tmp = turn_counter;
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
                            byte[] b;
                            if(turn_counter == 6)
                            {
                                b = Serialize(2, "your turn client 2", board, true, card_id);
                            }
                            else
                            {
                                b = Serialize(1, "your turn client 2", board, true, card_id);
                            }
                            client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(UpdateToOtherClientCallback), client_list[i]);
                        }
                    }
                    break;
                case 3: //means all is setted up
                    for(int i =0; i < client_list.Count(); i++)
                    {
                        if (client_list[i] != client)
                        {
                            byte[] b = Serialize(3, "your turn nº " + turn_counter, board, true,
                                client_list[i].tokens_list[client_list[i].tokencounter].identifier_n);

                            client_list[i].tokencounter++;
                            if (client_list[i].tokencounter >= 3)
                            {
                                Debug.Log("token counter reseted");
                                client_list[i].tokencounter = 0;
                            }

                            turn_counter++;
                            client_list[i].client_socket.BeginSend(b, 0, b.Length, 0, new AsyncCallback(UpdateToOtherClientCallback), client_list[i]);

                        }
                    }
                    break;
            }
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

}
