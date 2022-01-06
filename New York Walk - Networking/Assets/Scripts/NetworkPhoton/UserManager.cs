using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : ScriptableObject
{
    [HideInInspector] public int[] cards = new int[3];

    private int turnCounter= 0;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCards(List<int> cards)
    {
        this.cards = cards.ToArray();
        Debug.LogError($"Cards recieved {cards[0]},{cards[1]},{cards[2]}");
    }
}
