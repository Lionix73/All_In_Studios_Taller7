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

    [SerializeField] private float rotationSpeed = 10f;
    private bool isRotating = false;
    private Vector3 lastMousePosition;

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
    private void Update()
    {
        HandleCharacterRotation();
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
    private void HandleCharacterRotation()
    {
        if (characterToDisplay == null) return;

        // 1. Cuando se presiona el botón del mouse
        if (Input.GetMouseButtonDown(0))
        {
            // Lanzamos un rayo desde la cámara hacia la posición del mouse
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 2. Comprobamos si el rayo golpea al personaje
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.gameObject == characterToDisplay ||
                    hit.collider.transform.IsChildOf(characterToDisplay.transform))
                {
                    isRotating = true;
                    lastMousePosition = Input.mousePosition;
                }
            }
        }

        // 3. Cuando se suelta el botón del mouse
        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        // 4. Si está rotando, aplicamos la rotación en el eje Y
        if (isRotating && Input.GetMouseButton(0))
        {
            Vector3 delta = Input.mousePosition - lastMousePosition;
            characterToDisplay.transform.Rotate(Vector3.up, -delta.x * rotationSpeed * Time.deltaTime);
            lastMousePosition = Input.mousePosition;
        }
    }
    public void SelectAvatar()
    {
        //Debug.Log($"Has seleccionado el {_availableAvatars[_currentIndex]}");
    }
}
