using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthUI : MonoBehaviour
{
    public Image heartPrefabs;
    public Sprite fullHeartSprite;
    public Sprite emptyHeartSprite;

    private List<Image> hearts = new List<Image>();

    public void SetMaxHeart(int masHearts)
    {
        foreach (Image heart in hearts)
        {
            Destroy(heart.gameObject);
        }
        hearts.Clear();

        for(int i = 0; i < masHearts; i++)
        {
            Image newheart = Instantiate(heartPrefabs, transform);
            newheart.sprite = fullHeartSprite;
            newheart.color = Color.blue;
            hearts.Add(newheart);
        }
    }

    public void UpdateHearts(int currentHearts)
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            if (i < currentHearts)
            {
                hearts[i].sprite = fullHeartSprite;
                hearts[i].color = Color.blue;
            }
            else
            {
                hearts[i].sprite = emptyHeartSprite;
                hearts[i].color = Color.gray;
            }
        }
    }

}
