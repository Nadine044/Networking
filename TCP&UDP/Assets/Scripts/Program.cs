using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Program : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        for(int i =0; i <5; i++)
        {
            GameObject go = new GameObject();
            go.AddComponent<ClientTCP>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
