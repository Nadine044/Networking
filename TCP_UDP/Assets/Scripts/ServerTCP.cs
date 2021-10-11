using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ServerTCP : ServerProgram
{
    // Start is called before the first frame update
    readonly int maxClients = 3;
    private Socket _socket;
    private IPEndPoint ipep;

    private int current_clients = 0;
    bool listening = true;
    bool current_client_thread_alive = false;
    Queue<Thread> thread_queue = new Queue<Thread>();

    private EventWaitHandle wh = new AutoResetEvent(false);
    void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 27000);

        startThreadingFunction(Server);

    }

    // Update is called once per frame
    void Update()
    {

        //Here we check that once a thread is over we dequeue it and start the following one
        if (thread_queue.Count > 0 && !thread_queue.Peek().IsAlive) 
        {
            thread_queue.Dequeue();
            Debug.Log("thread dequeued");
            if (thread_queue.Count > 0 && !thread_queue.Peek().IsAlive)
                thread_queue.Peek().Start();
        }


    }



    //Maybe use thread pool
    void Server()
    {
        Socket client;
        _socket.Bind(ipep);
        _socket.Listen(maxClients);
        Debug.Log("Waiting for a client");
        //maybe do a loop here until a maximum capcity of clients has come
        while (listening)
        {
            if (!current_client_thread_alive)
            {
                current_client_thread_alive = true;
                client = _socket.Accept();
                thread_queue.Enqueue(new Thread(() => ClientHandler(client)));
                if (thread_queue.Count == 1)
                    thread_queue.Peek().Start();
            }

        }

        //We pause the execution until is resumed once all the clients are done
        wh.WaitOne();

        _socket.Close();
        Debug.Log("Closing server");

    }


    void ClientHandler(object c)
    {
        current_clients++;
        Debug.Log("Client accepted " + current_clients);

        //This way we stop iterating on the Server Thread once all the clients has been accepted
        if (current_clients == maxClients)
            listening = false;

        
        Socket client = (Socket)c;
        IPEndPoint client_ep;
        string pong = "pong";
        byte[] data = new byte[1024];
        int count = 0;
        int recv = 0;

        //Checks if the client is still connected to get the remote endpoint
        if (client.Connected)
        {
            client_ep = (IPEndPoint)client.RemoteEndPoint;
            Debug.Log("Connected: to client " + client_ep.ToString());
        }
        else
        {
            Debug.Log("Clients isn't connectet");
            Thread.CurrentThread.Abort();
        }

        //Recieves first message from client & waits for 500ms
        try
        {
            recv = client.Receive(data);
            Debug.Log("recieved Server " + Encoding.ASCII.GetString(data, 0, recv));
            Thread.Sleep(500);
        }
        catch (SocketException e)
        {
            Debug.LogWarning("can't recive first time from client " + e);
        }

        //Sends first message to client 
        data = new byte[1024];
        data = Encoding.ASCII.GetBytes(pong);
        client.Send(data, data.Length, SocketFlags.None);

        count++;
        while (count < 5) //Recieves & Sends messages to client
        {
            count++;

            data = new byte[1024];
            try //Recieve ping message
            {
                recv = client.Receive(data);
                Debug.Log("Recieved server " + Encoding.ASCII.GetString(data));
                Thread.Sleep(500);
            }
            catch (SystemException e)
            {
                Debug.LogWarning("Can't recieve from client" + e);
            }

            try //Send pong message & waits for 500ms
            {
                client.Send(Encoding.ASCII.GetBytes(pong));
   
            }
            catch (SystemException e)
            {
                Debug.LogWarning("Can't send to client " + e);
            }

        }

        Debug.Log("Closing Socket with client");
        client.Close();

        //To resume the server thread & close server socket
        if (current_clients == maxClients)
            wh.Set();
        

        current_client_thread_alive = false;

    }





}