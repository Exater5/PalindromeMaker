using UnityEditor;
using UnityEngine;
using System.IO;

public class PersistentDataPathOpener : EditorWindow
{
    [MenuItem("Tools/Open Persistent Data Path")]
    public static void OpenPersistentDataPath()
    {
        string path = Application.persistentDataPath;

        if (Directory.Exists(path))
        {
            EditorUtility.RevealInFinder(path);
        }
        else
        {
            Debug.LogError($"The directory does not exist: {path}");
        }
    }
}
