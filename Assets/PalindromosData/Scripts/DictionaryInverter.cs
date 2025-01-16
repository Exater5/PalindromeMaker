using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DictionaryInverter : MonoBehaviour
{
    [SerializeField] private string inputFileName = "palabras";
    [SerializeField] private string outputFileName = "palabras_invertidas";
    public bool loaded;
    public void GenerateMirroredWords()
    {
        TextAsset wordsInputArchive = Resources.Load<TextAsset>(inputFileName);
        if (wordsInputArchive != null)
        {
            string[] allWords = wordsInputArchive.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> invertedWords = new List<string>();

            foreach (string word in allWords)
            {
                string reversedWord = ReverseWord(word.Trim());
                invertedWords.Add(reversedWord);
            }

            invertedWords.Sort();
            SaveInvertedWordsToFile(invertedWords);

            Debug.Log("Proceso de inversión de palabras completo. Archivo guardado.");
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo de palabras.");
        }
    }

    private string ReverseWord(string word)
    {
        char[] charArray = word.ToCharArray();
        System.Array.Reverse(charArray);
        return new string(charArray);
    }

    private void SaveInvertedWordsToFile(List<string> invertedWords)
    {
        string filePath = Path.Combine(Application.persistentDataPath, outputFileName + ".txt");
        File.WriteAllLines(filePath, invertedWords);
        Debug.Log($"Archivo de palabras invertidas guardado en: {filePath}");
    }
}
