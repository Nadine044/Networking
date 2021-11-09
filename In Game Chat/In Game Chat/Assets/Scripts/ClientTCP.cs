using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using UnityEngine.UI;
public class Message
{
    public string name_;
    public string message;
    public string finalofmsg;
}

public class ClientTCP : ClientBase
{
    #region Client_Socket_OBJ
    public class ClientOBJ
    {
        // Size of receive buffer.  
        public const int BufferSize = 1024;

        // Receive buffer.  
        public byte[] buffer = new byte[BufferSize];

        // Received data string.
        public StringBuilder sb = new StringBuilder();

        // Client socket.
        public Socket socket = null;

        public bool endC = false;
    }
    #endregion

    #region Class_Variables
    [SerializeField]
    InputField inputField_text;

    string msg_to_send = string.Empty;
    string client_name = string.Empty;

    Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    IPEndPoint ipep;



    private static ManualResetEvent recieveDone = new ManualResetEvent(false);
    bool connected = false; //TODO CHANGE THIS, use another method
    private static ManualResetEvent connectDone = new ManualResetEvent(false);
    #endregion
    // Start is called before the first frame update
    public void Start() //We should create the several clients from here
    {
        GetComponent<ClientProgram>().closingAppEvent.AddListener(CloseApp);
        ipep = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 27000);
    }

    public void StartClient()
    {
        //Only for testing
        msg_to_send = "connected";

        StartThreadingFunction(Client);
        
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
            someFunc?.Invoke();
        }
        if (Input.GetKeyDown(KeyCode.S))
            client.Close();
    }

    void Client()
    {

        IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());

        byte[] data = new byte[1024];
        //int count = 0;

        int error_counter = 0;
        //tries to connect all the time to the server until the client achieve it
        //while (!connected)
        //{
        //    try
        //    {
        //        client.Connect(ipep);
        //        connected = true;
        //        Action Connected_Server = () => { logControl.LogText("connected to server", Color.black); };
        //        QueueMainThreadFunction(Connected_Server);
        //        Debug.Log("connected to server");

        //       // client.BeginConnect(ipep,AsyncCall)
        //    }
        //    catch (SocketException e)
        //    {
        //        if (e.NativeErrorCode.Equals(10035))
        //        {
        //            Debug.LogWarning("Still Connected, but the Send would block");
        //        }
        //        else
        //        {
        //            Debug.LogWarning("Disconnected: error code "+ e.NativeErrorCode);
        //        }
        //        Debug.LogWarning("Unable to connect to server  " + e.ToString());
        //        if (error_counter == 0)
        //        {
        //            Action ConnectionError = () => { logControl.LogText("Unable to connect to server  " + e.ToString(), Color.black); };
        //            QueueMainThreadFunction(ConnectionError);
        //            error_counter++;
        //        }

        //    }

        //}

        //FIRST MESSAGE, NEEDS TO CHANGE
        // data = Serialize(msg_to_send);

        ////Sends the first ping or message to the server
        //try
        //{
        //    client.Send(data);
        //    //Debug.Log("Send  client ping ");
        //} catch(SocketException e)
        //{
        //    Debug.LogWarning(e.SocketErrorCode);
        //    Action SendingError = () => { logControl.LogText("Unable to send " + e.ToString(), Color.black); };
        //    QueueMainThreadFunction(SendingError);
        //}

        //Send Connect Message to server
        client.BeginConnect(ipep, new AsyncCallback(ConnectCallback), client);
        connectDone.WaitOne();
        try { Send(client, msg_to_send); }
        catch (SystemException e)
        {
            Debug.LogWarning(e);
        }


        //recieving loop

        //while (!leave)
        //{

        //    try
        //    {
        //        data = new byte[1024];
        //        recv = client.Receive(data);
        //        Message tmp_m = Deserialize(data);
        //        Action Recieved_ = () => { logControl.LogText(tmp_m.name_ + tmp_m.message, Color.black); };
        //        QueueMainThreadFunction(Recieved_);
        //    }
        //    catch (SocketException e)
        //    {
        //        Debug.LogWarning(e);
        //    }
        //}
        ClientOBJ obj = new ClientOBJ();
        obj.socket = client;
        while (true)
        {
            recieveDone.Reset();

            client.BeginReceive(obj.buffer, 0, ClientOBJ.BufferSize, 0,
                new AsyncCallback(ReadCallback), obj);

            if (obj.endC)
                break;

            recieveDone.WaitOne();

    }

        try
        {
            client.Shutdown(SocketShutdown.Both);
        }
        catch (SocketException e)
        {
            Debug.Log("Couldnt shutdown server" + e);
        }
        
        client.Close();


    }

    void ConnectCallback(IAsyncResult ar)
    {
        try
        {
            Socket client_r = (Socket)ar.AsyncState;
            client.EndConnect(ar);
            Debug.Log("Socket connected to " + client_r.RemoteEndPoint.ToString());

            connectDone.Set();
        }
        catch(Exception e)
        {
            Action Errorconection = () => { logControl.LogText("Couldn't connect to server " + e, Color.black); };
            QueueMainThreadFunction(Errorconection);
            Debug.Log(e);
        }
    }
    void ReadCallback(IAsyncResult ar)
    {
        string content = string.Empty;

        ClientOBJ state = (ClientOBJ)ar.AsyncState;

    
        Socket handler = state.socket;
        int bytesRead = 0 ;

        bytesRead = handler.EndReceive(ar); //Peta AQUIII


        if (bytesRead >0)
        {
            state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
            content = Encoding.ASCII.GetString(state.buffer, 0, bytesRead);

            if (content.IndexOf("__END") > -1)
            {
                Message msg = Deserialize(state.buffer);

                string s = msg.name_ + ": " + msg.message;

                Action RecieveMsg = () => { logControl.LogText(s, Color.black); };
                QueueMainThreadFunction(RecieveMsg);

                recieveDone.Set();
            }
            else
            {
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);

            }

        }

        else //Server is disconnected
        {
            Action ServerDisconnect = () => { logControl.LogText("The server has shutet down", Color.black); };
            QueueMainThreadFunction(ServerDisconnect);
            state.endC = true;
        }
    }

    byte[] Serialize(string message)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(client_name);
        writer.Write(message);
        writer.Write("__END"); //To check if the message is fully read
        byte[] b = stream.GetBuffer();
       // writer.Close();
        stream.Close();
        GC.SuppressFinalize(stream);
        return b;
    }

    Message Deserialize(byte[] data)
    {
        Message msg = new Message();
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        msg.name_ = reader.ReadString();
        msg.message = reader.ReadString();
        msg.finalofmsg = reader.ReadString();
        //reader.Close();
        stream.Close();
        //GC.SuppressFinalize(stream);
        return msg;
    }

   


    public void NewMessageToSend(string s)
    {
        Action MsgSended = () => { logControl.LogText(client_name + ": " + s, Color.black); };
        QueueMainThreadFunction(MsgSended);
        Send(client, s);
        inputField_text.Select();
        inputField_text.text = "";
    }

    public void SetClientName(string s)
    {
        client_name = s;
    }

    private  void Send(Socket client, string msg)
    {
        // Convert the string data to byte data using ASCII encoding.  
        byte[] byteData = Serialize(msg);
        // Begin sending the data to the remote device.  
       // client.Send(byteData);
        client.BeginSend(byteData, 0, byteData.Length, 0,
            new AsyncCallback(SendCallback), client);
    }

    private  void SendCallback(IAsyncResult ar)
    {
        

            // Retrieve the socket from the state object.  
            Socket client = (Socket)ar.AsyncState;

            // Complete sending the data to the remote device.  
            int bytesSent = client.EndSend(ar);
            Debug.Log(bytesSent + "bytes sent to server");
            Action Disconnecting = () => { logControl.LogText(bytesSent.ToString() + "bytes sent to server", Color.black); };
            QueueMainThreadFunction(Disconnecting);
        


            // Signal that all bytes have been sent.          

    }

    public void ExitClient()
    {
        //Disconnecting
        Debug.Log("Disconnecting From server");
        Action Disconnecting = () => { logControl.LogText("Disconnecting from server", Color.black); };
        QueueMainThreadFunction(Disconnecting);

        try
        {
            client.Shutdown(SocketShutdown.Both);
        }
        catch (SystemException e)
        {
            Debug.LogWarning("Couldn't shutdown the server, socket already closed " + e);
        }


        try
        {
            client.Close();
        }
        catch (SystemException e)
        {
            Debug.Log("Couldn't Close socket" + e);
        }
        Action CloseSocket = () => { logControl.LogText("Socket Closed", Color.black); };
        QueueMainThreadFunction(CloseSocket);
    }
    void CloseApp()
    {

        try
        {
            client.Close();
        }
        catch (SystemException e)
        {
            Debug.Log("Couldn't Close socket" + e);
        }

    }
}
