using UnityEngine;

public class EditorLogger
{
    public static string AddColor(string word, LoggerColor color, bool bold = false)
    {
        var formatStr = string.Format("<color={0}>{1}</color>", color, word);
        if (bold)
        {
            formatStr = "<b>" + formatStr + "</b>";
        }
        return formatStr;
    }

    private static object[] ReplaceWordList(object[] replaceWords)
    {
        for (var i = 0; i < replaceWords.Length; i++)
        {
            replaceWords[i] = AddColor(replaceWords[i].ToString(), LoggerColor.yellow, true);
        }
        return replaceWords;
    }

    public static void Log(object obj, params object[] replaceWords)
    {
        Debug.Log(string.Format(AddColor(obj.ToString(), LoggerColor.lightblue), ReplaceWordList(replaceWords)));
    }
}

public enum LoggerColor
{
    blue,
    magenta,
    maroon,
    orange,
    red,
    purple,
    yellow,
    white,
    cyan,
    lightblue
}