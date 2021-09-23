using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("waitNSeconds");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    IEnumerator waitNSeconds()
    {
        System.DateTime myTime = System.DateTime.UtcNow;
        while (true)
        {
            while ((System.DateTime.UtcNow - myTime).Seconds < 5.0)
            {
                yield return null; //si es null ejecuta frame y después vuelve a ver si he acabado. 
                                   //Debug.Log(System.DataTime.UtcNow;

            }
            Debug.Log("Han pasado x segundos");
            myTime = System.DateTime.UtcNow;
        }      
    }
}
