using UnityEngine;

public static class ClipboardManager
{
    public static void CopyToClipboard(string text)
    {
        GUIUtility.systemCopyBuffer = text;
    }

    public static string PasteFromClipboard()
    {
        return GUIUtility.systemCopyBuffer;
    }
}