using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public JSONReader player_cards;
    private int randomNumber;

    public struct Card
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int difficulty;
    }

    public Card card1;
    public Card card2;
    public Card card3;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ObtainCitizenCard(Card randomCard, List<int> randomNumbers)
    {
        randomNumber = Random.RandomRange(0, 8);

        if (!randomNumbers.Contains(randomNumber))
        {
            randomNumbers.Add(randomNumber);

            randomCard.citizen = player_cards.playableCitizenList.citizens[randomNumber].citizen;
            randomCard.pickUp = player_cards.playableCitizenList.citizens[randomNumber].pickUp;
            randomCard.destiny = player_cards.playableCitizenList.citizens[randomNumber].destiny;
            randomCard.difficulty = player_cards.playableCitizenList.citizens[randomNumber].difficulty;

            Debug.Log(randomCard.citizen);
            Debug.Log(randomCard.pickUp);
            Debug.Log(randomCard.destiny);
            Debug.Log(randomCard.difficulty);
        }
        else
            ObtainCitizenCard(randomCard, randomNumbers);
    }

    
}
