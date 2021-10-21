using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;

public class StateObject
{
    // Size of receive buffer.  
    public const int BufferSize = 1024;

    // Receive buffer.  
    public byte[] buffer = new byte[BufferSize];

    // Received data string.
    public StringBuilder sb = new StringBuilder();

    // Client socket.
    public Socket workSocket = null;
}
public class ServerTCP : ServerBase
{
    // Start is called before the first frame update
    readonly int maxClients = 3;


    private int current_clients = 0;


    bool listening = true;
    Queue<Thread> thread_queue = new Queue<Thread>();

    private static EventWaitHandle wh = new AutoResetEvent(false);

    Socket client;
    Dictionary<Socket,string> users = new Dictionary<Socket,string>();
    //In case of suddenly exiting the App
    ConcurrentQueue<Socket> socket_queue = new ConcurrentQueue<Socket>();
    void Start()
    {
        GetComponent<ServerProgram>().closingAppEvent.AddListener(CloseApp);
    }

    public void StartServer()
    {
        startThreadingFunction(Server);
    }

    // Update is called once per frame
    void Update()
    {
        //WE execute the functions created in the secondary threads, otherwise we couldn't modify UnityObjects through threads
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc;
            functionsToRunInMainThread.TryDequeue(out someFunc);

            //Now run it;
            someFunc();
        }

        //Temporal to end server thread
        if(Input.GetKeyDown(KeyCode.Space))
        {
            wh.Set();
        }
    }


    void Server()
    {
        listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        ipep = new IPEndPoint(IPAddress.Any, 27000);

        listener.Bind(ipep);
        listener.Listen(maxClients);
        Debug.Log("Waiting for a client");

        Action WaitingClient = () =>{logControl.LogText("waiting for a client", Color.black);};
        QueueMainThreadFunction(WaitingClient);
        //maybe do a loop here until a maximum capcity of clients has come
        while (listening) //TODO Keep listening while the server is not full or if has been full but the number of clients has decreased
        {
            wh.Reset();
            listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
            wh.WaitOne();
        }

        //We pause the execution until is resumed once all the clients are done
        wh.WaitOne();
        
        Action ClosingServer = () =>
        {
            logControl.LogText("Closing server", Color.black);
        };
        QueueMainThreadFunction(ClosingServer);

        listener.Close();
        Debug.Log("Closing server");


    }
    public  void AcceptCallback(IAsyncResult ar)
    {
        // Signal the main thread to continue.  

        // Get the socket that handles the client request.  
        Socket listener = (Socket)ar.AsyncState;
        Socket handler = listener.EndAccept(ar);



        Thread t = new Thread(HandleClient);
        t.Start(handler);
        thread_queue.Enqueue(t);

        wh.Set();

    } //Keep Listening

    void HandleClient(object c)
    {
        Socket handler = (Socket)c;

        // Create the state object.  
        StateObject state = new StateObject();
        state.workSocket = handler;
        while (true) //aixo peta
        {
            try
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
                while(handler.Available ==0)
                {
                    Thread.Sleep(1);
                }
            }
            catch (SystemException e) //The problem may be here
            {
                Debug.LogWarning(e);
                //handler.Close();
                //break;
            }
        }
    }
    public  void ReadCallback(IAsyncResult ar)
    {
        string content = string.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);

        
       

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            //state.sb.Append(Encoding.ASCII.GetString(
            //    state.buffer, 0, bytesRead));

            Message msg = Deserialize(state.buffer);

            if (!users.ContainsKey(handler)) //Chekc if there is already added the client to dictionary
                users.Add(handler, msg.name_);

            string s = msg.name_ + ": " + msg.message;
            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            Action RecieveMsg = () => { logControl.LogText(s, Color.black); };
            QueueMainThreadFunction(RecieveMsg);

            //Send message to the rest of clients
            foreach (KeyValuePair<Socket, string> entry in users)
            {
                if (entry.Key != handler)
                {
                    Send(entry.Key, state.buffer);                   
                }

            }
        }
        else //Client is disconnected
        {
            string tmp_name = "";
            foreach(KeyValuePair<Socket,string> entry in users )
            {
                if (entry.Key == handler)
                {
                    tmp_name = entry.Value;
                    break;
                }

            }
            users.Remove(handler);

            Action RecieveMsg = () => { logControl.LogText("User" +tmp_name + "diconnected", Color.black); };
            QueueMainThreadFunction(RecieveMsg);
            try {
                handler.Close();
            }
            catch (SocketException e)
            {
                Debug.LogWarning("handler socket already closed" + e);
            }

            Thread.CurrentThread.Abort();
            }
    }

    private static void Send(Socket handler, byte[] data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        

        // Begin sending the data to the remote device.  
        handler.BeginSend(data, 0, data.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        try
        {
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }
        catch(SystemException e)
        {
            Debug.LogWarning(e);
        }
    }
    private static void SendCallback(IAsyncResult ar)
    {
        Debug.Log("Here");
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
           Debug.Log("Sent " + bytesSent +" bytes to client.");


        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }





    byte[] Serialize(string message,string client_name)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(client_name);
        writer.Write(message);
        return stream.GetBuffer();
    }

    Message Deserialize(byte[] data)
    {
        Message msg = new Message();
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        msg.name_ = reader.ReadString();
        msg.message = reader.ReadString();
        return msg;
    }
    void CloseApp()
    {

        while(thread_queue.Count>0 && !thread_queue.Peek().IsAlive)
        {


            //try
            //{
            //    Socket s;
            //    socket_queue.TryDequeue(out s);
            //    s.Close();
            //}
            //catch(SocketException e)
            //{
            //    Debug.LogWarning("Socket already closed");
            //}


            thread_queue.Peek().Abort();
        }

        try
        {
            listener.Close();
        }
        catch (SocketException e)
        {
            Debug.Log("Couldn't Close socket while exiting info" + e);
        }

        if (temp_thread.ThreadState == ThreadState.Running)
            temp_thread.Abort();


        current_clients = 0;
        listening = true;

    }


}