using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "newCharacter", menuName = "Character")]
public class Characters : ScriptableObject
{
    public GameObject playableCharacter;
    public GameObject displayCharacter;
    public string name;
    public string id;
    public bool unlocked;
}