using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static CharacterManager Instance;
    public int selectedIndexCharacter = 0;
    public List<Characters> characters;
    public int indexPassiveSkill = 0;
    public int indexActiveSkill = 0;


    private void Awake()
    {
        if (CharacterManager.Instance == null)
        {
            CharacterManager.Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

    }
}