using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DictionaryInverter : MonoBehaviour
{
    [SerializeField] private string inputFileName = "palabras";  // Nombre del archivo de entrada (sin extensión)
    [SerializeField] private string outputFileName = "palabras_invertidas";  // Nombre del archivo de salida (sin extensión)

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GenerateMirroredWords());
    }

    private IEnumerator GenerateMirroredWords()
    {
        // Cargar las palabras desde el archivo de recursos
        TextAsset wordsInputArchive = Resources.Load<TextAsset>(inputFileName);

        if (wordsInputArchive != null)
        {
            string[] allWords = wordsInputArchive.text.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            List<string> invertedWords = new List<string>();

            // Invertir cada palabra y añadirla a la lista
            foreach (string word in allWords)
            {
                string reversedWord = ReverseWord(word.Trim());
                invertedWords.Add(reversedWord);
            }

            // Guardar las palabras invertidas en un nuevo archivo de texto
            SaveInvertedWordsToFile(invertedWords);

            Debug.Log("Proceso de inversión de palabras completo. Archivo guardado.");
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo de palabras.");
        }

        yield return null; // Esperar un frame
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

        // Escribir las palabras invertidas en el archivo
        File.WriteAllLines(filePath, invertedWords);
        Debug.Log($"Archivo de palabras invertidas guardado en: {filePath}");
    }
}
