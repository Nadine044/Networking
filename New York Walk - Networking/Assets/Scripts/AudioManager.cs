using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource positionatingPlayersMusic;
    public AudioSource inGameMusic;
    public AudioSource lastCitizenMusic;
    public AudioSource pickUp;

    bool checkGameMusic = false;
    bool lastMusicPlaying = false;

    // Start is called before the first frame update
    void Start()
    {
        positionatingPlayersMusic.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (Player._instance.SFX_PickUp)
        {
            pickUp.Play();
            Player._instance.SFX_PickUp = false;
        }

        if (Player._instance.inGameMusicPlaying && !checkGameMusic)
        {
            checkGameMusic = true;
            positionatingPlayersMusic.Stop();
            inGameMusic.Play();
        }

        if (Player._instance.win_counter == 2 && !lastMusicPlaying)
        {
            lastMusicPlaying = true;
            inGameMusic.Stop();
            lastCitizenMusic.Play();
            Debug.Log("Play intense music here");
        }
    }
}
