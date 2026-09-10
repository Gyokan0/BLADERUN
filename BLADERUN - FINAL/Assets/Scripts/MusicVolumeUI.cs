using UnityEngine;

public class MusicVolumeUI : MonoBehaviour
{
    [SerializeField] private GameObject volumePanel;

    public void TogglePanel()
    {
        volumePanel.SetActive(!volumePanel.activeSelf);
    }

    public void SetVolume(float volume)
    {
        MusicManager musicManager = FindAnyObjectByType<MusicManager>();

        if (musicManager != null)
        {
            AudioSource music = musicManager.GetComponent<AudioSource>();
            music.volume = volume;
        }
    }
}