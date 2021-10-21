using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
public class ClientTCP : ClientBase
{
    string ping = "ping";
    int maxClients = 3;


    Queue<Socket> sockets_q = new Queue<Socket>(); //Need to make safe thread queue == ConcurrentQueue
    // Start is called before the first frame update
    public void Start() //We should create the several clients from here
    {
        GetComponent<ClientProgram>().closingAppEvent.AddListener(CloseApp);

    }

    public void StartClient()
    {
        for (int i = 0; i < maxClients; i++)
        {
            StartThreadingFunction(Client);
        }
    }

    public void SetNClients(int n_clients)
    {
        maxClients = n_clients;
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
            if(someFunc != null)
                someFunc();
        }
    }

    void Client()
    {
        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        sockets_q.Enqueue(server);
        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);

        byte[] data = new byte[1024];
        int count = 0;
        bool connected = false;

        //tries to connect all the time to the server until the client achieve it
        while (!connected)
        {
            try
            {
                server.Connect(ipep);
                connected = true;
                Action Connected_Server = () => { logControl.LogText("connected to server", Color.black); };
                QueueMainThreadFunction(Connected_Server);
                Debug.Log("connected to server");
            }
            catch (SocketException e)
            {
                if (e.NativeErrorCode.Equals(10035))
                {
                    Debug.LogWarning("Still Connected, but the Send would block");
                }
                else
                {
                    Debug.LogWarning("Disconnected: error code "+ e.NativeErrorCode);
                }
                Debug.LogWarning("Unable to connect to server  " + e.ToString());
                Action ConnectionError = () => { logControl.LogText("Unable to connect to server  " + e.ToString(), Color.black); };
                QueueMainThreadFunction(ConnectionError);
            }

        }

        //Sends the first ping or message to the server
        try
        {
            server.Send(Encoding.ASCII.GetBytes(ping));
            //Debug.Log("Send  client ping ");
        } catch(SocketException e)
        {
            Debug.LogWarning(e.SocketErrorCode);
            Action SendingError = () => { logControl.LogText("Unable to send " + e.ToString(), Color.black); };
            QueueMainThreadFunction(SendingError);
        }

        int recv;
        //Recieves the first message from the server & waits for 500ms
        try
        {
            recv = server.Receive(data);
            Debug.Log("Recieved  Client" + Encoding.ASCII.GetString(data, 0, recv));
            Action Recieved = () => { logControl.LogText(Encoding.ASCII.GetString(data, 0, recv), Color.black); };
            QueueMainThreadFunction(Recieved);
            Thread.Sleep(500);
        }
        catch(SystemException e)
        {
            Debug.LogWarning("Client coulnd't recieve from server " + e);
            Action RecievingError = () => { logControl.LogText("Unable to recieve " + e.ToString(), Color.black); };
            QueueMainThreadFunction(RecievingError);
        }
        
        count++;
        while (count < 5)
        {
            try //Sends message to server
            {
                count++;
                server.Send(Encoding.ASCII.GetBytes(ping));
                //Debug.Log("Send client ping");
            }
            catch(SystemException e)
            {
                Debug.LogWarning("Client Couldn't send message to server " + e);
                Action SendError = () => { logControl.LogText("Unable to send" + e.ToString(), Color.black); };
                QueueMainThreadFunction(SendError);
                break;
            }


            data = new byte[1024];
            try //Recieves message from server & waits for 500ms
            {
                recv = server.Receive(data);
                Debug.Log("Recived client " + Encoding.ASCII.GetString(data, 0, recv)); //Crashes here in the last update
                Action Recieved_ = () => { logControl.LogText(Encoding.ASCII.GetString(data, 0, recv), Color.black); };
                QueueMainThreadFunction(Recieved_);
                Thread.Sleep(500);
            }
            catch (SystemException e)
            {
                Debug.LogWarning("Couldn't recieve from server " + e);
                Action RecievedError_ = () => { logControl.LogText("Couldn't recieve from server " + e, Color.black); };
                QueueMainThreadFunction(RecievedError_);
            }

        }

        Debug.Log("Disconnecting From server");
        Action Disconnecting = () => { logControl.LogText("Disconnecting from server", Color.black); };
        QueueMainThreadFunction(Disconnecting);

        try
        {
            server.Shutdown(SocketShutdown.Both);
        }
        catch(SystemException e)
        {
            Debug.LogWarning("Couldn't shutdown the server, socket already closed " +e);
        }

        server.Close();
        sockets_q.Dequeue();
        Action CloseSocket = () => { logControl.LogText("Socket Closed", Color.black); };
        QueueMainThreadFunction(CloseSocket);
    }

    void CloseApp()
    {
        while (temp_threads.Count > 0)
        {
            try
            {
                sockets_q.Dequeue().Close();
            }
            catch (SystemException e)
            {
                Debug.Log("Couldn't Close socket" + e);
            }
            if (temp_threads.Peek().IsAlive)
                temp_threads.Dequeue().Abort();
            else temp_threads.Dequeue();
        }

    }
}
