using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawning : MonoBehaviour
{
    public GameObject itemPrefab;
    //List<GameObject> spawnedPrefabs = new List<GameObject>();

    private Vector3 spawnPosition = new Vector3(0.0f, 0.0f, 0.0f);
    float speed = 1f;
    private bool isSpawned;

    int maxSpawned = 5;
    int currentSpawned = 0;
    float lifeTime = 5f;
    void Start()
    {
        isSpawned = false;
    }

    
    void Update()
    {

        

        //CASE 3
        if(Input.GetKeyDown(KeyCode.Mouse0) && currentSpawned < maxSpawned)
        {
            currentSpawned++;
            StartCoroutine("ManageObj");
        }
        //---------
    }

    //CASE 3
    IEnumerator ManageObj()
    {
        GameObject go =  Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        float time = 0;

        while(time <= lifeTime)
        {
            go.transform.position += go.transform.forward * Time.deltaTime * speed;
            time += Time.deltaTime;
            yield return null;
        }
        currentSpawned--;
        Destroy(go);
        
    }
    //void moveObject()
    //{
    //    itemPrefab.transform.position += itemPrefab.transform.forward * Time.deltaTime * speed;
    //}

    //IEnumerator destroyPrefab()
    //{
    //    System.DateTime myTime = System.DateTime.UtcNow;
    //    while (true)
    //    {
    //        while ((System.DateTime.UtcNow - myTime).Seconds < 5.0)
    //        {
    //            yield return null; //"si es null ejecuta frame y después vuelve a ver si he acabado."
    //                               //Debug.Log(System.DataTime.UtcNow;

    //        }

    //        Destroy(itemPrefab);
    //        Debug.Log("The object has been destroyed after 5 seconds");
    //        isSpawned = false;

    //        myTime = System.DateTime.UtcNow;
    //    }
    //}
}
