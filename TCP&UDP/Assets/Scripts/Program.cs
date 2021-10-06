using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Program : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Testing_Socket_UDP s = new Testing_Socket_UDP();
        s.Server("127.0.0.1", 27000);

        Testing_Socket_UDP c = new Testing_Socket_UDP();
        c.Client("127.0.0.1", 27000);
        c.Send("TEST!");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
