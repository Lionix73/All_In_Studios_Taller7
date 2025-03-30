using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int indexPlayer = CharacterManager.Instance.selectedIndexCharacter;
        Instantiate(CharacterManager.Instance.characters[indexPlayer].playableCharacter, transform.position, Quaternion.identity);
    }

}
