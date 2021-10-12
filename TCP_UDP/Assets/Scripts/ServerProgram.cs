using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerProgram : MonoBehaviour
{

    [SerializeField]
    List<GameObject> UI_to_hide;

    [SerializeField]
    GameObject starterPanel;


    // Start is called before the first frame update
    void Start()
    {
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(false);
        }
    } 


    public void StartUDPServer()
    {
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ServerUDP>().enabled = true;

    }
    public void StartTCPServer()
    {
        starterPanel.SetActive(false);
        foreach (GameObject go in UI_to_hide)
        {
            go.SetActive(true);
        }
        this.GetComponent<ServerTCP>().enabled = true;

    }


}
