using TMPro;
using UnityEngine;

public class CharacterMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameCharacter;
    [SerializeField] private TMP_Text _selectionText;
    private int _currentIndex;
    [SerializeField] Vector3 posCharacter;
    [SerializeField] Quaternion rotCharacter;

    private int selectedIndex = 0;
    CharacterManager characterManager;
    GameObject characterToDisplay;

    private void OnEnable()
    {

        characterManager = CharacterManager.Instance;
        //characterManager.characters[selectedIndex]
        RenderAvatar();
    }
    private void OnDisable()
    {
        Destroy(characterToDisplay);
    }
    public void SelectSkin()
    {
        if (characterManager.characters[_currentIndex].unlocked == true)
        {
            characterManager.selectedIndexCharacter = _currentIndex;
            DisplaySelectionText();
        }
    }
    public void NextAvatar()
    {
        _currentIndex = (_currentIndex + 1) % characterManager.characters.Count;
        Debug.Log(_currentIndex);
        Destroy(characterToDisplay);
        RenderAvatar();

    }
    public void PreviousAvatar()
    {
        if (_currentIndex <= 0)
        {
            _currentIndex = characterManager.characters.Count - 1;
        }
        else
        {
            _currentIndex = (_currentIndex - 1) % characterManager.characters.Count;
        }
        Debug.Log(_currentIndex);
        Destroy(characterToDisplay);
        RenderAvatar();
    }

    private void RenderAvatar()
    {
        characterToDisplay = Instantiate(characterManager.characters[_currentIndex].displayCharacter, posCharacter, rotCharacter);
        _nameCharacter.text = characterManager.characters[_currentIndex].name;
        DisplaySelectionText();

    }
    private void DisplaySelectionText()
    {
        if (characterManager.selectedIndexCharacter == _currentIndex)
        {
            _selectionText.text = "Selected";
        }
        else if (characterManager.characters[_currentIndex].unlocked != true)
        {
            _selectionText.text = "Coming Soon";
        }
        else
        {
            _selectionText.text = "Select";
        }
    }

    public void SelectAvatar()
    {
        //Debug.Log($"Has seleccionado el {_availableAvatars[_currentIndex]}");
    }
}
