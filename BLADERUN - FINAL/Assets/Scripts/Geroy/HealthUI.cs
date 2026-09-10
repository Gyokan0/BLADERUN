using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image healthImage;

    public Sprite health4;
    public Sprite health3;
    public Sprite health2;
    public Sprite health1;
    public Sprite health0;

    public void UpdateHealth(int currentHealth)
    {
        if (currentHealth >= 4)
            healthImage.sprite = health4;
        else if (currentHealth == 3)
            healthImage.sprite = health3;
        else if (currentHealth == 2)
            healthImage.sprite = health2;
        else if (currentHealth == 1)
            healthImage.sprite = health1;
        else
            healthImage.sprite = health0;
    }
}