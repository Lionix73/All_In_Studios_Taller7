using UnityEngine;
using UnityEditor;
/*
public class MissingScript : MonoBehaviour
{
    [MenuItem("Tools/Missing Scripts/ Find Missing Scripts in Scene")]

    static void FindMissingScriptsInSceneMenuItem()
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true))
        {
            Component[] components = gameObject.GetComponentsInChildren<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                {
                    bool isHidden = gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy);

                    Debug.Log($"GameObject found with missing component {gameObject.name} {(isHidden ? "<color=red>[Hidden Game Object]</color>": string.Empty) }" , gameObject);
                    break;
                }
            }
        }
    }
    [MenuItem("Tools/Missing Scripts/ Show Hidden Missing Scripts in Scene")]

    static void ShowHiddenMissingScriptsInSceneMenuItem()
    {
        foreach (GameObject gameObject in GameObject.FindObjectsOfType<GameObject>(true))
        {
            Component[] components = gameObject.GetComponentsInChildren<Component>();
            foreach (Component component in components)
            {
                if (component != null)
                {
                    if (gameObject.hideFlags.HasFlag(HideFlags.HideInHierarchy)) 
                    {
                        gameObject.hideFlags = gameObject.hideFlags & HideFlags.HideInHierarchy;
                        Debug.Log($"Shown GameObject with missing component {gameObject.name}", gameObject);
                        break;

                    }

                }
            }
        }
    }
}
*/