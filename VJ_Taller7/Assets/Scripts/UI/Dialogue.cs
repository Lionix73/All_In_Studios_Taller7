using FMODUnity;
using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter music;
    [SerializeField] private TextMeshProUGUI textComp;
    [SerializeField] private float textSpeed;
    [SerializeField] private string[] lines;
    [SerializeField] private float[] delays;
    [SerializeField] private bool finishDialogue = false;

    public string [] Lines { get { return lines; } set { lines = value; } }
    public float [] Delays { get { return delays; } set { delays = value; } }
    public float TextSpeed { get { return textSpeed; } set { textSpeed = value; } }
    public bool FinishDialogue { get { return finishDialogue; } set { finishDialogue = value; } }
    
    private int index;

    private void Start()
    {
        textComp.text = string.Empty;
        StartDialogue();
    }

    public void StartDialogue()
    {
        finishDialogue = false;
        gameObject.SetActive(true);
        index = 0;
        StartCoroutine(DialogDuration());
    }

    IEnumerator TypeLine()
    {
        foreach (char c in lines[index].ToCharArray()) 
        {
            textComp.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    private void NextLine()
    {
        index++;
        
        if(index < lines.Length)
        {
            textComp.text = string.Empty;
            StartCoroutine(DialogDuration());
        }
        else
        {
            StopAllCoroutines();
            finishDialogue = true;
            if(!music.IsPlaying()) music.Play();
            gameObject.SetActive(false);
        }
    }

    IEnumerator DialogDuration()
    {
        float time = 0;
        StartCoroutine(TypeLine());

        while (time < delays[index])
        {
            time += Time.deltaTime;
            yield return null;
        }

        NextLine();
    }
}
