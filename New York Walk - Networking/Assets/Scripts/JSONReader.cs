using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public TextAsset CitizensTXT;
    public TextAsset CityCardsTXT;

    [System.Serializable]
    public class Citizen
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int pickUpID;
        public int destinyID;
        public int difficulty;
        public int[] unavailableSquares;
    }

    [System.Serializable]
    public class PowerUp
    {
        public string name;
        public string utility;
        public int turns;
    }

    [System.Serializable]
    public class CitizenList
    {
        public Citizen[] citizens;
    }

    [System.Serializable]
    public class PowerUpList
    {
        public PowerUp[] powerUps;
    }

    public CitizenList playableCitizenList = new CitizenList();
    public PowerUpList cityCardsList = new PowerUpList();

    // Start is called before the first frame update
    void Start()
    {
        playableCitizenList = JsonUtility.FromJson<CitizenList>(CitizensTXT.text);
        cityCardsList = JsonUtility.FromJson<PowerUpList>(CityCardsTXT.text);
    }
}
