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
    
    public enum PackageIndex
    {
        PlayerTurnReconnect = -3,
        SetUpReconnect = -2,
        WaitReconnection = -1,
        WaitOtherPlayerToEnterGame = 0,
        PlayerTurnSetUp = 1,
        LastTurnOfSetUp = 2,
        PlayerTurnInGame = 3
    };
    protected class Package
    {
        public int index;
        public string msg_to_log;
        public int[] board_array = new int[25];
        public bool turn;
        public int card;
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

    /// <summary>
    /// Serlializes the data into bytes 
    /// </summary>
    /// <param name="index">StateOfTheGame</param>
    /// <param name="msg_to_log"> log</param>
    /// <param name="board_array">the board table</param>
    /// <param name="turn">player's turn</param>
    /// <param name="card_type">token or card identifier to move</param>
    /// <returns>byte[] array</returns>
    protected byte[] Serialize(int index, string msg_to_log, int[] board_array,bool turn,int card_type)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);
        writer.Write(msg_to_log);

        writer.Write(turn);
        switch(index)
        {
            case (int)PackageIndex.WaitOtherPlayerToEnterGame: //initializing game
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }

                break;
            case (int)PackageIndex.PlayerTurnSetUp:
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }
                writer.Write(card_type);
                break;
            case (int)PackageIndex.LastTurnOfSetUp:
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }
                writer.Write(card_type);
                break;
            case (int)PackageIndex.PlayerTurnInGame:
                for (int i = 0; i < board_array.Length; i++)
                {
                    writer.Write(board_array[i]);
                }
                writer.Write(card_type);
                break;
        }

        return stream.GetBuffer();
    }

    /// <summary>
    /// Serialize into byte array, only for sending a concrete state, like wait for another server call
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected byte[] Serialize(int index)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);

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
        package.turn = reader.ReadBoolean();
        switch(package.index)
        {
            case (int)PackageIndex.WaitOtherPlayerToEnterGame: //initializing game
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;
            case (int)PackageIndex.PlayerTurnSetUp:
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;
            case (int)PackageIndex.LastTurnOfSetUp:
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;
            case (int)PackageIndex.PlayerTurnInGame:
                for (int i = 0; i < 25; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;
        }
        Debug.Log(package.msg_to_log);


        return package;
    }
}
