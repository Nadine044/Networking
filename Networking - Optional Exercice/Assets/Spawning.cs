using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawning : MonoBehaviour
{
    public GameObject itemPrefab;

    private Vector3 spawnPosition = new Vector3(0.0f, 0.0f, 0.0f);
    float speed = 1f;

    bool isCoroutineRunning = false;

    int maxSpawned = 5;
    int currentSpawned = 0;

    float time = 0;
    float lifeTime = 5f;

    float sharedTime = 0;
    
    void Update()
    {
        //CASE 1
        //if (Input.GetKeyDown(KeyCode.Mouse0))
        //{
        //    StartCoroutine("CASE_1");
        //}
        //--------------- 

        //CASE 2
        //if (Input.GetKeyDown(KeyCode.Mouse0) && !isCoroutineRunning)
        //{
        //    StartCoroutine("CASE_2");
        //}
        //--------------- 

        //CASE 3
        //if (Input.GetKeyDown(KeyCode.Mouse0) && currentSpawned < maxSpawned)
        //{
        //    currentSpawned++;
        //    StartCoroutine("CASE_3");
        //}
        //--------------- 

        //CASE 4
        if (Input.GetKeyDown(KeyCode.Mouse0) && currentSpawned < maxSpawned)
        {
            currentSpawned++;
            sharedTime = sharedTime + 5f;
            StartCoroutine("CASE_4");
        }
    }

    //CASE 1
    IEnumerator CASE_1()
    {
        GameObject go = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        time = 0;

        while (time <= lifeTime)
        {
            go.transform.position += go.transform.forward * Time.deltaTime * speed;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(go);
    }
    //---------
    //CASE 2
    IEnumerator CASE_2()
    {
        isCoroutineRunning = true;
        GameObject go = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        time = 0;

        while (time <= lifeTime)
        {
            go.transform.position += go.transform.forward * Time.deltaTime * speed;
            time += Time.deltaTime;
            yield return null;
        }
        Destroy(go);
        isCoroutineRunning = false;
    }
    //---------
    //CASE 3
    IEnumerator CASE_3()
    {
        GameObject go =  Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        time = 0;
        

        while(time <= lifeTime)
        {
            go.transform.position += go.transform.forward * Time.deltaTime * speed;
            time += Time.deltaTime;
            yield return null;
        }
        currentSpawned--;
        Destroy(go);
    }
    //---------
    //CASE 4
    IEnumerator CASE_4()
    {
        GameObject go = Instantiate(itemPrefab, spawnPosition, Quaternion.identity);
        time = 0;

        while (time <= sharedTime)
        {
            go.transform.position += go.transform.forward * Time.deltaTime * speed;
            time += Time.deltaTime;
            yield return null;
        }
        currentSpawned--;
        Destroy(go);
    }
}
