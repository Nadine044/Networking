using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System;

public class MultiplayerGameController : MonoBehaviour, IOnEventCallback
{
    private GameTurn turnState;
    private const byte SET_GAME_STATE_EVENT_CODE = 1;
    private const byte GIVE_CARDS = 2;
    private const byte SET_TOKEN = 3;
    private const byte UPDATE_TOKEN = 4;

    private UserManager userManager;

    private User localUser; //get the local user TODO
    private User activePlayer; //this will swap to the other client when turn ends

    private UIManager uiManager;
    private MultiplayerBoard board;

    private bool firstTurn = false;

    private void Awake()
    {
       // userManager = ScriptableObject.CreateInstance(typeof(UserManager)) as UserManager;
        turnState = GameTurn.OtherTurn;
        userManager = GetComponent<UserManager>();
        userManager.SetController(this);
    }

    public void SetFirstTurnTrue()
    {
        firstTurn = true;
    }
    //this should be called when we try to click on the board
    public bool CanPerformMove()
    {
        if(!IsLocalPlayerTurn())
        {
            return false;
        }
        return true;
    }

    private bool IsLocalPlayerTurn()
    {
        return turnState == GameTurn.MyTurnSetUp || turnState == GameTurn.MyTurn;
    }
    ////
    //private bool IsGameInProgress()
    //{
    //    //should check if the game still active
    //}

    public void EndTurn()
    {
        ChangeOtherGameState(GameTurn.MyTurn);
        //end current token animation
    }

    public void EndSetUpTurn()
    {
        ChangeOtherGameState(GameTurn.MyTurnSetUp);
    }

    public GameTurn GetGameTurn()
    {
        return turnState;
    }


    public void SetDependencies(UIManager uiManager, User user, MultiplayerBoard multi_board)
    {
        this.uiManager = uiManager;
        localUser = user;
        board = multi_board;
        userManager.FillBoardSquares(board.GetComponentsInChildren<BoxCollider>());
    }

    #region PUN EVENTS

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += SetTokenEvent;
        PhotonNetwork.NetworkingClient.EventReceived += RecieveRandomCards;
        PhotonNetwork.AddCallbackTarget(this);
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= SetTokenEvent;
        PhotonNetwork.NetworkingClient.EventReceived -= RecieveRandomCards;
        PhotonNetwork.RemoveCallbackTarget(this);
    }
    private void SetTokenEvent(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SET_TOKEN)
        {
            object[] data = (object[])photonEvent.CustomData;
        }
        //do if from here
        
    }
    public void OnEvent(EventData photonEvent) 
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == SET_GAME_STATE_EVENT_CODE)
        {
            object[] data = (object[])photonEvent.CustomData;
            GameTurn state = (GameTurn)data[0];
            this.turnState = state;
            if (turnState == GameTurn.MyTurnSetUp && userManager.GetTokenCounter() < 3)
                userManager.SetUpToken();

            else if (userManager.GetTokenCounter() == 3)
            {
                turnState = GameTurn.MyTurn;
                userManager.UpdateToken(); //TODO how do we use other events?
            }
        }
    }

    private void SetTokenEvent()
    {
        object[] content = new object[] { 2, 14 }; //token id, token pos
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.All }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(SET_TOKEN, content, raiseEventOptions, SendOptions.SendReliable);
    }

    public void SetTurnState(GameTurn turnState)
    {
        this.turnState = turnState;
    }

    //bassically here we tell the other player its his turn
    private void ChangeOtherGameState(GameTurn state)
    {
        this.turnState = GameTurn.OtherTurn;
        object[] content = new object[] { (int)state };
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(SET_GAME_STATE_EVENT_CODE, content, raiseEventOptions, SendOptions.SendReliable);
    }


    public void SetRandomCards(List<int>randomCards)
    {
        //we kept the first half and give the other player the other half
        userManager.SetCards(randomCards.GetRange(0, 3));
        GiveCards(randomCards.GetRange(3, 3));
        userManager.SetUpToken();
    }

    private void GiveCards(List<int> randomcards)
    {

        object[] content = new object[] { randomcards[0],randomcards[1],randomcards[2] }; //token id, token pos
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions { Receivers = ReceiverGroup.Others }; //recieverGroup Maybe just other?
        PhotonNetwork.RaiseEvent(GIVE_CARDS, content, raiseEventOptions, SendOptions.SendReliable);
    }

    private void RecieveRandomCards(EventData photonEvent)
    {
        byte eventCode = photonEvent.Code;
        if (eventCode == GIVE_CARDS)
        {
            object[] data = (object[])photonEvent.CustomData;

            List<int> dataRecieved = new List<int>();
            dataRecieved.Add((int)data[0]);
            dataRecieved.Add((int)data[1]);
            dataRecieved.Add((int)data[2]);
            userManager.SetCards(dataRecieved);
            Debug.LogError("Entered here hehe");
        }
    }
    #endregion


}
