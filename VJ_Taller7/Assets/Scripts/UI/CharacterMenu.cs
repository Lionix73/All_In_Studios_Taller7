using System.Linq;
using TMPro;
using UnityEngine;

public class CharacterMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject _player;
    [SerializeField] string[] _availableAvatars;
    [SerializeField] private TMP_Text _textMeshPro;
    private int _currentIndex;

    private void OnEnable()
    {
        
    }
    public void NextAvatar()
    {
        _currentIndex = (_currentIndex + 1) % _availableAvatars.Length;
        Debug.Log(_currentIndex);
        RenderAvatar();

    }
    public void PreviousAvatar()
    {
        if (_currentIndex <= 0)
        {
            _currentIndex = _availableAvatars.Length - 1;
        }
        else
        {
            _currentIndex = (_currentIndex - 1) % _availableAvatars.Length;
        }
        Debug.Log(_currentIndex);
        RenderAvatar();
    }

    private void RenderAvatar()
    {
        _textMeshPro.text = _availableAvatars[_currentIndex];
    }

    public void SelectAvatar()
    {
        Debug.Log($"Has seleccionado el {_availableAvatars[_currentIndex]}");
    }
}
