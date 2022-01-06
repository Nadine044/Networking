using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    [HideInInspector] public int[] cards = new int[3];

    private int turnCounter= 0;
    // Start is called before the first frame update
    private GameTurn state;
    private MultiplayerGameController controller;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(controller.CanPerformMove()&& Input.GetKeyDown(KeyCode.Mouse0))
        {
            Debug.LogError("Playing");
            controller.EndTurn();
        }
    }

    public void SetController(MultiplayerGameController controller)
    {
        this.controller = controller;
    }
    public void SetCards(List<int> cards)
    {
        this.cards = cards.ToArray();
    }
    public void SetState(GameTurn state)
    {
        this.state = state;
    }
}
