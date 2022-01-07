using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class TokenScript : MonoBehaviour
{
    private PhotonView photonView;

    private void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }

    ////TODO pass more things but for know just like this
    //public void LastTokenSetted() /////NO et pot fer aixi perque llavors ho rebrien tots els tokens, no només el meu, potser fer-ho a través del tablero?4
    //{
    //    photonView.RPC(nameof(RPC_LastTokenSetted))
    //}

    //[PunRPC]
    //private object RPC_LastTokenSetted()
    //{
    //    //change game state
    //}



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
