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
        public int difficulty;
        public int[] unavailableSquares;
    }

    public class PowerUp
    {
        public string name;
        public string utility;
        public int turns;
        public int howMany;
    }

    [System.Serializable]
    public class CitizenList
    {
        public Citizen[] citizens;
    }

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
        cityCardsList = JsonUtility.FromJson<PowerUpList>(CitizensTXT.text);
    }
}
