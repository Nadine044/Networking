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

        while (true)
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
            catch (SystemException e)
            {
                Debug.Log(e);
                handler.Close();
            }
        }
    }
    public  void ReadCallback(IAsyncResult ar)
    {
        String content = String.Empty;

        // Retrieve the state object and the handler socket  
        // from the asynchronous state object.  
        StateObject state = (StateObject)ar.AsyncState;
        Socket handler = state.workSocket;

        // Read data from the client socket.
        int bytesRead = handler.EndReceive(ar);


       

        if (bytesRead > 0)
        {
            // There  might be more data, so store the data received so far.  
            state.sb.Append(Encoding.ASCII.GetString(
                state.buffer, 0, bytesRead));

            Message msg = Deserialize(state.buffer);
            string s = msg.name_ + ": " + msg.message;
            // Check for end-of-file tag. If it is not there, read
            // more data.  
            content = state.sb.ToString();
            Action RecieveMsg = () => { logControl.LogText(s, Color.black); };
            QueueMainThreadFunction(RecieveMsg);

            //Send to other Clients
           // Send(handler, content);
            //if (content.IndexOf("<EOF>") > -1)
            //{
            //    // All the data has been read from the
            //    // client. Display it on the console.  
            //   Debug.Log("Read "+ content.Length + " bytes from socket. \n Data : "+ content);
            //    // Echo the data back to the client.  
            //    Send(handler, content);
            //}
            //else
            //{
            //    // Not all data received. Get more.  
            //    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
            //    new AsyncCallback(ReadCallback), state);
            //}
        }
    }

    private static void Send(Socket handler, String data)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Encoding.ASCII.GetBytes(data);

        // Begin sending the data to the remote device.  
        handler.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), handler);
    }
    private static void SendCallback(IAsyncResult ar)
    {
        try
        {
            // Retrieve the socket from the state object.  
            Socket handler = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = handler.EndSend(ar);
           Debug.Log("Sent " + bytesSent +" bytes to client.");

            handler.Shutdown(SocketShutdown.Both);
            handler.Close();

        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
        }
    }


    //------------------

    //Handles each client  when is connected to the server
    void ClientHandler(object c)
    {
        current_clients++;
        Debug.Log("Client accepted " + current_clients);
        Action ClientAccepted = () => {logControl.LogText("Client accepted " + current_clients, Color.black); };
        QueueMainThreadFunction(ClientAccepted);

        //This way we stop iterating on the Server Thread once all the clients has been accepted
        if (current_clients == maxClients)
            listening = false;

        
        Socket client = (Socket)c;
        socket_queue.Enqueue(client);
        IPEndPoint client_ep;
        byte[] data = new byte[1024];
        //int count = 0;
        int recv = 0;

        //Checks if the client is still connected to get the remote endpoint
        if (client.Connected)
        {
            client_ep = (IPEndPoint)client.RemoteEndPoint;
            Debug.Log("Connected: to client " + client_ep.ToString());
            Action ConnectedtoClient = () => {logControl.LogText("Connected: to client " + client_ep.ToString(), Color.black); };
            QueueMainThreadFunction(ConnectedtoClient);
        }
        else
        {
            Debug.Log("Clients isn't connected");
            Action ClientNoConnect = () => {logControl.LogText("Clients isn't connected", Color.black);};
            QueueMainThreadFunction(ClientNoConnect);
            client.Close();
            Thread.CurrentThread.Abort();
        }

        //Recieves first message from client & waits for 500ms
        try
        {
            recv = client.Receive(data);
            Message msg = Deserialize(data);
            string s = msg.name_ + ": " + msg.message;
            //Debug.Log("recieved Server " + Encoding.ASCII.GetString(data, 0, recv));
            Action RecieveMsg = () => {logControl.LogText(s, Color.black);};
            QueueMainThreadFunction(RecieveMsg);
        }
        catch (SocketException e)
        {
            Debug.LogWarning("can't recive first time from client " + e);
        }

        //Now we have to send to all other clients who are in the server

        //Sends first message to client 
      
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
        


    }

    void startRecieve(Socket client)
    {
      //  client.BeginReceive();
    }
    //byte[] Serialize(string message)
    //{
    //    MemoryStream stream = new MemoryStream();
    //    BinaryWriter writer = new BinaryWriter(stream);
    //    writer.Write(client_name);
    //    writer.Write(message);
    //    return stream.GetBuffer();
    //}

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