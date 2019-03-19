using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextDisplay : MonoBehaviour
{
#if UNITY_EDITOR
    [ShowOnly]
    [SerializeField]
    private List<string> textList = new List<string>();

    private void Awake()
    {
        Destroy(this, 5);
    }

    internal void SetText(string text)
    {
        textList.Clear();
        textList.AddRange(text.Split('\n'));
    }
#endif
}

public static class DisplayTextExtension
{
    public static void DisplayTextComponent(this GameObject gameObject, string s)
    {
#if UNITY_EDITOR
        if (Selection.Contains(gameObject))
        {
            var textDisplay = gameObject.GetComponent<TextDisplay>();
            if (textDisplay == null) textDisplay = gameObject.AddComponent<TextDisplay>();
            textDisplay.SetText(s);
        }
#endif
    }
    public static void DisplayTextComponent(this GameObject gameObject, object o)
    {
        gameObject.DisplayTextComponent(o.ToString());
    }

}