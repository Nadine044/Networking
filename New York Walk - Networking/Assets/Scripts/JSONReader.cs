using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JSONReader : MonoBehaviour
{
    public TextAsset textJSON;

    [System.Serializable]
    public class Citizen
    {
        public string citizen;
        public string pickUp;
        public string destiny;
        public int difficulty;
    }

    [System.Serializable]
    public class CitizenList
    {
        public Citizen[] citizens;
    }

    public CitizenList playableCitizenList = new CitizenList();

    // Start is called before the first frame update
    void Start()
    {
        playableCitizenList = JsonUtility.FromJson<CitizenList>(textJSON.text);
    }
}
