using UnityEngine;
using UnityEngine.InputSystem;

public class RingMenu : MonoBehaviour
{
    public RingAnimation[] ringAnimations;
    public RingPiece ringPiecePrefab;
    public int activeElement;

    private Animator _animator;
    private PlayerSoundsManager _soundManager;
    private RingPiece[] ringPieces;
    private float degreesPerPiece;
    private float gapDegrees = 1f;

    private void Start()
    {
        _animator = GameObject.FindWithTag("Player").GetComponent<Animator>();
        _soundManager = GameObject.FindWithTag("Player").GetComponentInChildren<PlayerSoundsManager>();
        degreesPerPiece = 360f / ringAnimations.Length;

        float distanceToIcon = Vector3.Distance(ringPiecePrefab.icon.transform.position, ringPiecePrefab.bg.transform.position);

        ringPieces = new RingPiece[ringAnimations.Length];

        for (int i = 0; i < ringAnimations.Length; i++)
        {
            ringPieces[i] = Instantiate(ringPiecePrefab, transform);

            ringPieces[i].bg.fillAmount = (1f / ringAnimations.Length) - (gapDegrees / 360f);
            ringPieces[i].bg.transform.localRotation = Quaternion.Euler(0, 0, degreesPerPiece / 2f + gapDegrees / 2f + i * degreesPerPiece);

            ringPieces[i].icon.sprite = ringAnimations[i].icon;

            Vector3 dirVector = Quaternion.AngleAxis(i * degreesPerPiece, Vector3.forward) * Vector3.up;
            Vector3 movVector = dirVector * distanceToIcon;

            ringPieces[i].icon.transform.localPosition = ringPieces[i].bg.transform.localPosition + movVector;
        }
    }

    private void Update()
    {
        Vector2 inputVector;
        bool isGamepad = Gamepad.current != null && Gamepad.current.rightStick.ReadValue().magnitude > 0.3f;

        if (isGamepad)
        {
            // Usa el stick derecho como direcci�n
            inputVector = Gamepad.current.rightStick.ReadValue();
        }
        else
        {
            // Usa la posici�n del mouse
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
            Vector3 cursorVector = Input.mousePosition - screenCenter;
            inputVector = new Vector2(cursorVector.x, cursorVector.y);
        }

        activeElement = GetActiveElement(inputVector);
        HighlightActiveElement(activeElement);
    }

    private int GetActiveElement(Vector2 direction)
    {
        const float deadZone = 0.35f;

        if (direction.magnitude < deadZone)
            return activeElement; // Mant�n el actual si est� cerca del centro

        float angle = Vector2.SignedAngle(Vector2.up, direction);
        float normalizedAngle = NormalizeAngle(angle + degreesPerPiece / 2f);

        int selectedIndex = (int)(normalizedAngle / degreesPerPiece);

        // Si ya est� seleccionado, solo retorna de nuevo
        if (selectedIndex == activeElement)
            return activeElement;

        // Si cambi� significativamente, actualiza
        return selectedIndex;
    }

    private float NormalizeAngle(float x) => (x + 360f) % 360f;

    private void HighlightActiveElement(int activeElement)
    {
        for (int i = 0; i < ringPieces.Length; i++)
        {
            if(i == activeElement)
            {
                ringPieces[i].bg.color = new Color(1f, 1f, 1f, 0.9f);
            }
            else
            {
                ringPieces[i].bg.color = new Color(1f, 1f, 1f, 0.7f);
            }
        }
    }

    public void Emotear()
    {
        _animator.SetBool("isEmoting", true);
        _animator.SetFloat("EmoteIndex", activeElement);
        _soundManager.EmoteMusic(ringAnimations[activeElement].name);
    }
}
