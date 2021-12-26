using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource positionatingPlayersMusic;
    public AudioSource inGameMusic;
    public AudioSource lastCitizenMusic;

    public Player player;

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
        if (player.inGameMusicPlaying && !checkGameMusic)
        {
            checkGameMusic = true;
            positionatingPlayersMusic.Stop();
            inGameMusic.Play();
        }

        if (player.win_counter == 2 && !lastMusicPlaying)
        {
            lastMusicPlaying = true;
            inGameMusic.Stop();
            lastCitizenMusic.Play();
            Debug.Log("Play intense music here");
        }
    }
}
