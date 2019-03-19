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
    float remainingLifeTime = 5f;
    float mostRecentTimestamp = 0f;

    private void Update()
    {
        if (Selection.Contains(gameObject))
            remainingLifeTime = 5f;
        else
            remainingLifeTime -= Time.deltaTime;

        if(remainingLifeTime <= 0)
            Destroy(this);
    }

    internal void SetText(string text)
    {
        var newAdditions = new List<string>();
        if (mostRecentTimestamp != Time.time)
        {
            mostRecentTimestamp = Time.time;
            newAdditions.Add("NEW FRAME -- timestamp: " + mostRecentTimestamp);
        }

        newAdditions.AddRange(text.Split('\n'));
        newAdditions.AddRange(textList);
        var maxLines = 100;
        if(newAdditions.Count > maxLines)
            newAdditions.RemoveRange(maxLines, newAdditions.Count - maxLines);
        textList = newAdditions;
    }
    
#endif
}

public static class DisplayTextExtension
{
    public static void DisplayTextComponent(this GameObject gameObject, string message, string sender = "")
    {
#if UNITY_EDITOR
        if (Selection.Contains(gameObject))
        {
            var textDisplay = gameObject.GetComponent<TextDisplay>();
            if (textDisplay == null) textDisplay = gameObject.AddComponent<TextDisplay>();
            textDisplay.SetText( "sender: " + sender + "\n" + message + "\n-----------------END-----------------" + "\n");
        }
#endif
    }

    public static void DisplayTextComponent(this GameObject gameObject, string message, object sender)
    {
        if (sender == null)
            gameObject.DisplayTextComponent(message, "NULL");
        else
            gameObject.DisplayTextComponent(message, sender.GetType().Name);
    }

    public static void DisplayTextComponent(this GameObject gameObject, object o, object sender)
    {
        if (sender == null)
            gameObject.DisplayTextComponent(o + "", "NULL");
        else
            gameObject.DisplayTextComponent(o + "", sender.GetType().Name);
    }

    public static void DisplayTextComponent(this GameObject gameObject, object o)
    {
        if(o == null)
            gameObject.DisplayTextComponent(o + "", "NULL");
        else
            gameObject.DisplayTextComponent(o + "", o.GetType().Name);
    }

}