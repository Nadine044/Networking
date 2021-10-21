using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

public class SerializerExample : MonoBehaviour
{
    static MemoryStream stream;
    bool a = true;
    // Start is called before the first frame update
    ClientMessage c = new ClientMessage();
    void Start()
    {
        c.message = "test_message";
        c.name = "test_name";
    }



    // Update is called once per frame
    void Update()
    {
        if (a)
        {
            SerializeTest();
            DeserializeTest();
            //serialize();
            //deserialize();
            //serializeJson();
            //deserializeJson();
            //serializeXML();
            //deserializeXML();
            a = false;
        }
    }

    public class testClass
    {
        public int hp = 12;
        public List<int> pos = new List<int> { 3, 3, 3 };
    }

    public class ClientMessage
    {
        public string name = "test_name";
        public string message = "test_message";
    }
    void SerializeTest()
    {
        string name = c.name;
        string message = c.message;
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(name);
        writer.Write(message);
    }
    void DeserializeTest()
    {
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        string r_name = reader.ReadString();
        Debug.Log("name " + r_name);
        string r_message = reader.ReadString();
        Debug.Log("message " + r_message);

    }
    void serializeJson()
    {
        var t = new testClass();
        t.hp = 40;
        t.pos = new List<int> { 10, 3, 12 };
        string json = JsonUtility.ToJson(t);
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(json);
    }
    void deserializeJson()
    {
        var t = new testClass();
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);

        string json = reader.ReadString();
        Debug.Log(json);
        t = JsonUtility.FromJson<testClass>(json);
        Debug.Log(t.hp.ToString() + " " + t.pos.ToString());
    }

    void serializeXML()
    {
        var t = new testClass();
        t.hp = 40;
        t.pos = new List<int> { 10, 3, 12 };
        XmlSerializer serializer = new XmlSerializer(typeof(testClass));
        stream = new MemoryStream();
        serializer.Serialize(stream, t);
        bytes = stream.ToArray();

    }
    void deserializeXML()
    {
        XmlSerializer serializer = new XmlSerializer(typeof(testClass));
        var t = new testClass();
        stream = new MemoryStream();
        stream.Write(bytes, 0, bytes.Length);
        stream.Seek(0, SeekOrigin.Begin);
        t = (testClass)serializer.Deserialize(stream);
        Debug.Log("Xml " + t.hp.ToString() + " " + t.pos.ToString());
    }
    byte[] bytes;
    void serialize()
    {
        double myfloat = 100f;
        int myint = 15;
        string mystring = "test";
        int[] mylist = new int[3] { 1, 2, 4 };
        stream = new MemoryStream();
        BinaryWriter writer = new BinaryWriter(stream);
        writer.Write(myfloat);
        writer.Write(myint);
        writer.Write(mystring);
        foreach (var i in mylist)
        {
            writer.Write(i);
        }
        Debug.Log("serialized!");
        bytes = stream.ToArray();
    }

    void deserialize()
    {
        stream = new MemoryStream();
        stream.Write(bytes, 0, bytes.Length);
        BinaryReader reader = new BinaryReader(stream);
        stream.Seek(0, SeekOrigin.Begin);
        double newfloat = reader.ReadDouble();

        Debug.Log("float " + newfloat.ToString());

        int newint = reader.ReadInt32();
        Debug.Log("int " + newint.ToString());
        string newstring = reader.ReadString();
        Debug.Log("string " + newstring.ToString());
        int[] newlist = new int[3];
        for (int i = 0; i < newlist.Length; i++)
        {
            newlist[i] = reader.ReadInt32();
        }
        Debug.Log(newlist.ToString());


    }
}