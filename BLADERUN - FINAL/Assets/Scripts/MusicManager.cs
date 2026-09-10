using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    private static MusicManager instance;

    [SerializeField] private AudioSource audioSource;

    [Header("Music Order")]
    [SerializeField] private AudioClip watermelonBeats;
    [SerializeField] private AudioClip forgottenHero;
    [SerializeField] private AudioClip deuslower;

    private AudioClip[] playlist;
    private int currentTrack = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        playlist = new AudioClip[]
        {
            watermelonBeats,
            forgottenHero,
            deuslower
        };
    }

    private void Start()
    {
        StartCoroutine(PlayPlaylist());
    }

    private IEnumerator PlayPlaylist()
    {
        while (true)
        {
            AudioClip clip = playlist[currentTrack];

            if (clip != null)
            {
                audioSource.clip = clip;
                audioSource.Play();

                yield return new WaitForSeconds(clip.length);
            }

            currentTrack++;

            if (currentTrack >= playlist.Length)
                currentTrack = 0;
        }
    }
}