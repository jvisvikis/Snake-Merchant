using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomerAvatar : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> characters;
    [SerializeField]
    private List<Sprite> hats;
    [SerializeField]
    private Image character;
    [SerializeField]
    private Image hat;

    public void RandomiseCharacter()
    {
        character.sprite = characters[Random.Range(0,characters.Count)];
        hat.sprite = hats[Random.Range(0, hats.Count)];
    }

}
