using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenScript : MonoBehaviour
{
    private Vector3 destination; 
    private Animator anim;
    // Start is called before the first frame update
    private float time_to_reach_target = 0.4f;
    float t =0;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetDestPos(Vector3 dst)
    {
        t = 0;
        destination = dst;
        StartMovement();
    }

    public void StartMovement()
    {
        anim.SetBool("jump", true);
        StartCoroutine("MoveToPos");
        //anim.SetBool("jump",false);
    }

    IEnumerator MoveToPos()
    {
        t += Time.deltaTime / time_to_reach_target;
        while (transform.position != destination)
        {
            float x = Mathf.MoveTowards(transform.position.x, destination.x, t);
            float z = Mathf.MoveTowards(transform.position.z, destination.z, t);
            transform.position = new Vector3(x, transform.position.y, z);
            Debug.Log("Inside here");
            yield return null;
        }

        anim.SetBool("jump", false);
    }
}
