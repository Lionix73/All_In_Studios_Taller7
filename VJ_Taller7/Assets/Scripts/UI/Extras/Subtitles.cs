using FMODUnity;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Subtitles : MonoBehaviour
{
    [SerializeField] private GameObject subtitlesBox;
    [SerializeField] private float textSpeed;
    [SerializeField] private SubtitleInfo[] subtitles;

    private TextMeshProUGUI textComponent;
    private int index;

    void Awake()
    {
        textComponent = subtitlesBox.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        subtitlesBox.SetActive(true);
        StartDialogue();
    }

    public void StartDialogue()
    {
        textComponent.text = string.Empty;
        index = 0;
        SetupSubtitle();
    }

    private IEnumerator TypeLine()
    {
        foreach (char c in subtitles[index].Text.ToCharArray())
        {
            textComponent.text += c;
            yield return new WaitForSeconds(textSpeed);
        }

        yield return new WaitForSeconds(subtitles[index].Delay);
        NextLine();
    }

    private void PlaySound()
    {
        if(!subtitles[index].Sound.IsNull)
        {
            RuntimeManager.PlayOneShot(subtitles[index].Sound);
        }
    }

    private void NextLine()
    {
        index++;

        if (index < subtitles.Length)
        {
            textComponent.text = string.Empty;
            SetupSubtitle();
        }
        else
        {
            StopAllCoroutines();
            gameObject.SetActive(false);
        }
    }

    private void SetupSubtitle()
    {
        PlaySound();
        StartCoroutine(TypeLine());
    }
}

[Serializable]
public struct SubtitleInfo
{
    public string Text;
    public float Delay;
    public EventReference Sound;
}
