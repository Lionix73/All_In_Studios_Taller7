using System;
using UnityEngine;

public class RingMenu : MonoBehaviour
{
    public RingAnimation[] ringAnimations;
    public RingPiece ringPiecePrefab;
    public int activeElement;

    private Animator anim;
    private PlayerSoundsManager soundManager;
    private RingPiece[] ringPieces;
    private float degreesPerPiece;
    private float gapDegrees = 1f;

    private void Start()
    {
        anim = GameObject.FindWithTag("Player").GetComponent<Animator>();
        soundManager = GameObject.FindWithTag("Player").GetComponent<PlayerSoundsManager>();
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
        activeElement = GetActiveElement();
        HighlightActiveElement(activeElement);
    }

    private int GetActiveElement()
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);
        Vector3 cursorVector = Input.mousePosition - screenCenter;

        float mouseAngle = Vector3.SignedAngle(Vector3.up, cursorVector, Vector3.forward) + degreesPerPiece / 2f;
        float normalizedMouseAngle = NormalizeAngle(mouseAngle);

        return (int)(normalizedMouseAngle / degreesPerPiece);
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
        anim.SetBool("isEmoting", true);
        anim.SetFloat("EmoteIndex", activeElement);
        soundManager.EmoteMusic(ringAnimations[activeElement].name);
    }
}
