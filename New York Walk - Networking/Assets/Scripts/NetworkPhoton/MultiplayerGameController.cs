using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;


public class MultiplayerGameController : MonoBehaviour, IOnEventCallback
{
    private GameState state;
    private const byte SET_GAME_STATE_EVENT_CODE = 1;
    private const byte SET_TOKEN = 2;
    private const byte UPDATE_TOKEN = 3;
    private User localUser; //get the local user TODO
    private User activePlayer; //this will swap to the other client when turn ends

    private UIManager uiManager;
    private MultiplayerBoard board;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SeTTokenEvent();
        }
    }
    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += SetTokenEvent;
        PhotonNetwork.AddCallbackTarget(this);
    }
    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= SetTokenEvent;
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    private void SetTokenEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SET_TOKEN)
        {
            object[] data = (object[])photonEvent.CustomData;

            Debug.LogError($"Token Event Setted id {(int)data[0]}, pos {data[1]} :)");
        }
    }
    public void OnEvent(EventData photonEvent) //we can also subscribe custom function events so that we don't have a hughe function
    {
        byte eventCode = photonEvent.Code;
        if(eventCode == SET_GAME_STATE_EVENT_CODE)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameState state = (GameState)data[0];
            this.state = state;
        }
    }
    
    private void SeTTokenEvent()
    {
        object[] content = new object[] { 2 , 14 }; //token id, token pos
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(SET_TOKEN, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void SetGameState(GameState state)
    {
        object[] content = new object[] { (int)state };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(SET_GAME_STATE_EVENT_CODE, content, raiseEventOptions, SendOptions.SendReliable);
    }

    //this should be called when we try to click on the board
    bool CanPerformMove()
    {
        if(!IsGameInProgress() || !IsLocalPlayerTurn())
        {
            return false;
        }
        return true;
    }

    private bool IsLocalPlayerTurn()
    {
        return localUser = activePlayer;
    }
    private bool IsGameInProgress()
    {
        return state == GameState.Play;
    }

    public void EndTurn()
    {

    }

    public void SetDependencies(UIManager uiManager, User user, MultiplayerBoard multi_board)
    {
        this.uiManager = uiManager;
        localUser = user;
        board = multi_board;
    }
}
