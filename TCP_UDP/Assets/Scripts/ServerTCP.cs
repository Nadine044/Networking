using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ServerTCP : MonoBehaviour
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

    protected Queue<Action> functionsToRunInMainThread = new Queue<Action>();
    protected string CurrentLog;

    [SerializeField]
    protected TextLogControl logControl;

    void Start()
    {
        _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 27000);

        startThreadingFunction(Server);
    }

    // Update is called once per frame
    void Update()
    {
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc = functionsToRunInMainThread.Peek();
            functionsToRunInMainThread.Dequeue();

            //Now run it;
            someFunc();
        }

        //Here we check that once a thread is over we dequeue it and start the following one
        if (thread_queue.Count > 0 && !thread_queue.Peek().IsAlive) 
        {
            thread_queue.Dequeue();
            Debug.Log("thread dequeued");
            if (thread_queue.Count > 0 && !thread_queue.Peek().IsAlive)
                thread_queue.Peek().Start();
        }
        
    }
    private void startThreadingFunction(Action someFunction)
    {
        Thread t = new Thread(someFunction.Invoke);
        t.Start();

    }
    public void QueueMainThreadFunction(Action someFunction)
    {
        //We need to make sure that some function is running from the main Thread

        //someFunction(); //This isn't okay, if we're in a child thread
        functionsToRunInMainThread.Enqueue(someFunction);
    }

    //Maybe use thread pool
    void Server()
    {
        Socket client;
        _socket.Bind(ipep);
        _socket.Listen(maxClients);
        Debug.Log("Waiting for a client");
        Action WaitingClient = () =>
        {
            logControl.LogText("waiting for a client", Color.black);
        };
        QueueMainThreadFunction(WaitingClient);
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

        Action ClosingServer = () =>
        {
            logControl.LogText("Closing server", Color.black);
        };
        QueueMainThreadFunction(ClosingServer);

    }


    void ClientHandler(object c)
    {
        current_clients++;
        Debug.Log("Client accepted " + current_clients);
        Action ClientAccepted = () =>
        {
            logControl.LogText("Client accepted " + current_clients, Color.black);
        };
        QueueMainThreadFunction(ClientAccepted);

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
            Action ConnectedtoClient = () =>
            {
                logControl.LogText("Connected: to client " + client_ep.ToString(), Color.black);
            };
            QueueMainThreadFunction(ConnectedtoClient);
        }
        else
        {
            Debug.Log("Clients isn't connected");
            Action ClientNoConnect = () =>
            {
                logControl.LogText("Clients isn't connected", Color.black);
            };
            QueueMainThreadFunction(ClientNoConnect);
            Thread.CurrentThread.Abort();
        }

        //Recieves first message from client & waits for 500ms
        try
        {
            recv = client.Receive(data);
            Debug.Log("recieved Server " + Encoding.ASCII.GetString(data, 0, recv));
            Action RecieveMsg = () =>
            {
                logControl.LogText("recieved Server " + Encoding.ASCII.GetString(data, 0, recv), Color.black);
            };
            QueueMainThreadFunction(RecieveMsg);
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
                Action RecieveMsg = () =>
                {
                    logControl.LogText("recieved Server " + Encoding.ASCII.GetString(data), Color.black);
                };
                QueueMainThreadFunction(RecieveMsg);
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
                Action CantSend = () =>
                {
                    logControl.LogText("Can't send to client " + e, Color.black);
                };
                QueueMainThreadFunction(CantSend);
            }

        }

        Debug.Log("Closing Socket with client");
        Action CloseServer = () =>
        {
            logControl.LogText("Closing Socket with client", Color.black);
        };
        QueueMainThreadFunction(CloseServer);

        client.Close();

        //To resume the server thread & close server socket
        if (current_clients == maxClients)
            wh.Set();
        

        current_client_thread_alive = false;

    }





}