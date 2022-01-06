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
        if (User._instance.SFX_PickUp)
        {
            pickUp.Play();
            User._instance.SFX_PickUp = false;
        }

        if (User._instance.inGameMusicPlaying && !checkGameMusic)
        {
            checkGameMusic = true;
            positionatingPlayersMusic.Stop();
            inGameMusic.Play();
        }

        if (User._instance.win_counter == 2 && !lastMusicPlaying)
        {
            lastMusicPlaying = true;
            inGameMusic.Stop();
            lastCitizenMusic.Play();
            Debug.Log("Play intense music here");
        }
    }
}
