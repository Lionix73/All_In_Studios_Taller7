using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Dialogue : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textComp;
    [SerializeField] private float textSpeed;
    [SerializeField] private string[] lines;
    [SerializeField] private float[] delays;
    
    private int index;

    private void Start()
    {
        textComp.text = string.Empty;
        StartDialogue();
    }

    private void StartDialogue()
    {
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
