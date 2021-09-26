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

    void Start()
    {
        isSpawned = false;
    }

    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && !isSpawned)
        {
            StartCoroutine("destroyPrefab");
            itemPrefab = (GameObject)Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
            //spawnedPrefabs.Add(itemPrefab);

            Debug.Log("Object Spawned");
            isSpawned = true;
        }

        if (isSpawned)
            moveObject();
    }

    void moveObject()
    {
        itemPrefab.transform.position += itemPrefab.transform.forward * Time.deltaTime * speed;
    }

    IEnumerator destroyPrefab()
    {
        System.DateTime myTime = System.DateTime.UtcNow;
        while (true)
        {
            while ((System.DateTime.UtcNow - myTime).Seconds < 5.0)
            {
                yield return null; //"si es null ejecuta frame y después vuelve a ver si he acabado."
                                   //Debug.Log(System.DataTime.UtcNow;

            }

            Destroy(itemPrefab);
            Debug.Log("The object has been destroyed after 5 seconds");
            isSpawned = false;

            myTime = System.DateTime.UtcNow;
        }
    }
}
