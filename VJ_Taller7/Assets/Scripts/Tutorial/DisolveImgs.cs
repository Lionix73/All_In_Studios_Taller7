using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DisolveImgs : MonoBehaviour
{
    [SerializeField] private float minDistance = 3f;
    [SerializeField] private float fadeSpeed = 2f;

    private Transform player;
    private Image[] images;
    private TextMeshProUGUI[] texts;
    private float currentAlpha = 1f;
    private float targetAlpha = 1f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        images = GetComponentsInChildren<Image>();
        texts = GetComponentsInChildren<TextMeshProUGUI>();
    }

    void Update()
    {
        if (player == null) return;
        float distance = Vector3.Distance(transform.position, player.position);
        targetAlpha = distance < minDistance ? 0f : 1f;
        currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);

        // Update images
        if (images != null)
        {
            foreach (var img in images)
            {
                if (img != null)
                {
                    Color c = img.color;
                    c.a = currentAlpha;
                    img.color = c;
                }
            }
        }

        // Update texts
        if (texts != null)
        {
            foreach (var txt in texts)
            {
                if (txt != null)
                {
                    Color c = txt.color;
                    c.a = currentAlpha;
                    txt.color = c;
                }
            }
        }
    }
}
