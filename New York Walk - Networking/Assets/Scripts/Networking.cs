using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.IO;

public class Networking : MonoBehaviour
{
    protected class Package
    {
        public int index;
        public string msg_to_log;
        public int[] board_array = new int[25];
        public int client;
    }
    public class OBJ
    {
        public const int buffersize = 1024;
        public byte[] buffer = new byte[buffersize];
    }
    protected ConcurrentQueue<Action> functionsToRunInMainThread = new ConcurrentQueue<Action>();
    // Update is called once per frame

    protected Socket socket;
    protected IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
    protected IPEndPoint ipep;
    // Start is called before the first frame update
    void Start()
    {
        
    }
    protected void StartThreadingFunction(Action someFunction)
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

    private void LateUpdate()
    {
        while (functionsToRunInMainThread.Count > 0)
        {
            //Grab the first/oldest function in the list
            Action someFunc;
            functionsToRunInMainThread.TryDequeue(out someFunc);

            //Now run it;
            someFunc();
        }
    }
    //index
    //0 = wait for the other player
    //1 = your turn
    //2 = other players turn
    protected byte[] Serialize(int index, string msg_to_log, int[] board_array,int client)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);
        writer.Write(msg_to_log);


        switch(index)
        {
            case 0: //initializing game
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }
                writer.Write(client);

                break;
            case 1:
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }
                writer.Write(client);
                break;
            case 2:
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }
                writer.Write(client);
                break;
        }

        return stream.GetBuffer();
    }
    protected Package Deserialize(byte[] data)
    {
        Package package = new Package();
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);
        package.index = reader.ReadInt32();
        package.msg_to_log = reader.ReadString();

        switch(package.index)
        {
            case 0: //initializing game
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                break;
            case 1:
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.client = reader.ReadInt32();
                break;
            case 2:
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.client = reader.ReadInt32();
                break;
        }
        Debug.Log(package.msg_to_log);


        return package;
    }
}
