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
    private static readonly int board_length = 25;
    public enum PackageIndex
    {
        ResumePlay = -5,
        PlayerTurnReconnectSetUp = -4,//update the reconnecting player on the setup stage of the game
        PlayerTurnReconnect = -3,
        Reconnect_NotYourTurn = -2, //update the reconnecting player but he left the game when it wasn't his turn
        WaitReconnection = -1,//do nothing just wait until the other player reconnects
        WaitOtherPlayerToEnterGame = 0,
        PlayerTurnSetUp = 1,
        ShowRemainingCards = 2, //when setup is done we enter in this stage
        PlayerTurnInGame = 3,
        TokenArriveDestiny = 4,
        ClientWin = 5

    };
    protected class Package
    {
        public int index;
        public string msg_to_log;
        public int[] board_array = new int[25];
        public bool turn;
        public int card;
        public List<int> token_list_id;
        public int[] showcards = new int[2];
        public int win_counter = 0;
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
    protected byte[] Serialize(int index, string msg_to_log, int[] board_array,int card_type)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);
        writer.Write(msg_to_log);

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

    protected byte[] Serialize(int index, int[] board)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(index);
        for (int i = 0; i < board.Length; i++)
        {
            writer.Write(board[i]);
        }
        return stream.GetBuffer();
    }

    /// <summary>
    /// This serialize is used when one client reconnects & he must be updated
    /// </summary>
    /// <param name="index"></param>
    /// <param name="board"></param>
    /// <param name="token_list_id">array of the tokens_id the reconnecting client had</param>
    /// <returns></returns>
    protected byte[] Serialize(int index, int[] board, int win_counter, int[] token_list_id)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);
        for (int i = 0; i < board.Length; i++)
        {
            writer.Write(board[i]);
        }
        writer.Write(token_list_id.Length);
        for (int i = 0; i < token_list_id.Length; i++)
        {
            writer.Write(token_list_id[i]);
        }
        writer.Write(win_counter);
        return stream.GetBuffer();
    }

    protected byte[] Serialize(int index, int[] board, int[] token_list_id, int current_token_to_move, int wincounter )
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);
        for (int i = 0; i < board.Length; i++)
        {
            writer.Write(board[i]);
        }
        writer.Write(token_list_id.Length);
        for (int i = 0; i < token_list_id.Length; i++)
        {
            writer.Write(token_list_id[i]);
        }
        writer.Write(current_token_to_move);
        writer.Write(wincounter);
        return stream.GetBuffer();
    }

    /// <summary>
    /// This serialize is used when one client reconnects & he must be updated and the move a token
    /// </summary>
    /// <param name="index"></param>
    /// <param name="board"></param>
    /// <param name="token_list_id"></param>
    /// <param name="current_token_to_move"></param>
    /// <returns></returns>
    protected byte[] Serialize(int index, int[] board, int[] token_list_id,int current_token_to_move)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);

        writer.Write(index);
        for (int i = 0; i < board.Length; i++)
        {
            writer.Write(board[i]);
        }
        writer.Write(token_list_id.Length);
        for (int i = 0; i < token_list_id.Length; i++)
        {
            writer.Write(token_list_id[i]);
        }
        writer.Write(current_token_to_move);
        return stream.GetBuffer();
    }

    protected byte[] Serialize(int index, int[] board, int token_that_arrived)
    {
        MemoryStream stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(index);
        for (int i = 0; i < board.Length; i++)
        {
            writer.Write(board[i]);
        }
        writer.Write(token_that_arrived);
        return stream.GetBuffer();
    }



    protected Package Deserialize(byte[] data)
    {
        Package package = new Package();
        MemoryStream stream = new MemoryStream(data);
        BinaryReader reader = new BinaryReader(stream);

        stream.Seek(0, SeekOrigin.Begin);
        package.index = reader.ReadInt32();

        switch(package.index)
        {
            case (int)PackageIndex.WaitOtherPlayerToEnterGame: //0
                package.msg_to_log = reader.ReadString();
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;

            case (int)PackageIndex.PlayerTurnSetUp://1
                package.msg_to_log = reader.ReadString();
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;

            case (int)PackageIndex.PlayerTurnInGame://3
                package.msg_to_log = reader.ReadString();
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;

            case (int)PackageIndex.TokenArriveDestiny://4
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.card = reader.ReadInt32();
                break;

            case (int)PackageIndex.Reconnect_NotYourTurn: //-2
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.token_list_id = new List<int>();
                int tmp = reader.ReadInt32();
                for(int i = 0; i < tmp; i++)
                {
                    package.token_list_id.Add(reader.ReadInt32());
                }
                package.win_counter = reader.ReadInt32();
                break;

            case (int)PackageIndex.PlayerTurnReconnect://-3
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                int count_turn_reconnect = reader.ReadInt32();
                package.token_list_id = new List<int>();

                for (int i = 0; i < count_turn_reconnect; i++)
                {
                    package.token_list_id.Add(reader.ReadInt32());
                }
                package.card = reader.ReadInt32();
                package.win_counter = reader.ReadInt32();
                break;

            case (int)PackageIndex.PlayerTurnReconnectSetUp: //-4
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                package.token_list_id = new List<int>();
                int tmp_token_counter = reader.ReadInt32();
                if (tmp_token_counter > 0)
                {
                    for (int i = 0; i < tmp_token_counter; i++)
                    {
                        package.token_list_id.Add(reader.ReadInt32());
                    }
                }
                package.card = reader.ReadInt32();
                break;

            case (int)PackageIndex.ClientWin://5
                for (int i = 0; i < board_length; i++)
                {
                    package.board_array[i] = reader.ReadInt32();
                }
                break;
        }
        //Debug.Log(package.msg_to_log);


        return package;
    }
}
