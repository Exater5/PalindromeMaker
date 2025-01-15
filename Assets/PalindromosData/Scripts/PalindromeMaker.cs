using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using System.IO;

class PalindromeMaker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _progressText;
    static List<string> words;
    static HashSet<string> invertedWords;
    static HashSet<string> foundPalindromes; // Usamos HashSet para evitar duplicados

    private int processedCombinations = 0;
    private int totalCombinations;

    private IEnumerator Start()
    {
        words = new List<string>();
        invertedWords = new HashSet<string>();
        foundPalindromes = new HashSet<string>();

        // Cargar el archivo de palabras
        TextAsset wordsInputArchive = Resources.Load<TextAsset>("palabras");
        if (wordsInputArchive != null)
        {
            string[] allWords = wordsInputArchive.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int batchSize = 500;

            Debug.Log($"Archivo detectado con {allWords.Length} palabras. Cargando...");

            for (int i = 0; i < allWords.Length; i++)
            {
                words.Add(allWords[i].Trim());
                if ((i + 1) % batchSize == 0)
                {
                    Debug.Log($"Progreso: {i + 1}/{allWords.Length} palabras cargadas.");
                    yield return null;
                }
            }
            Debug.Log($"Carga completa: {words.Count} palabras cargadas.");

            // Cargar el archivo de palabras invertidas
            TextAsset invertedWordsInputArchive = Resources.Load<TextAsset>("palabras_invertidas");
            if (invertedWordsInputArchive != null)
            {
                string[] allInvertedWords = invertedWordsInputArchive.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
                invertedWords = new HashSet<string>(allInvertedWords.Select(w => w.Trim()));

                Debug.Log($"Archivo de palabras invertidas detectado y cargado con {invertedWords.Count} palabras invertidas.");
            }
            else
            {
                Debug.LogError("No se pudo cargar el archivo de palabras invertidas.");
            }

            // Cargar los palíndromos encontrados (si existen)
            LoadFoundPalindromes();

            // Cargar el progreso guardado (si existe)
            LoadProgress();

            // Inicia el proceso principal
            yield return StartCoroutine(Main());
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo de palabras.");
        }
    }

    private IEnumerator Main()
    {
        Debug.Log("Generando palíndromos...");

        // Procesar combinaciones en lotes
        yield return StartCoroutine(GetWordCombinationsCoroutine(words, 5000, batch =>
        {
            foreach (var combination in batch)
            {
                string combined = string.Join(" ", combination);
                if (IsPalindrome(combined) && !foundPalindromes.Contains(combined)) // Verificamos si ya está en la lista
                {
                    Debug.Log($"Palíndromo encontrado: {combined}");

                    // Agregar a la lista de palíndromos encontrados y guardar en el archivo
                    foundPalindromes.Add(combined);
                    SavePalindrome(combined);
                }
            }

            // Guardar el progreso después de cada lote
            SaveProgress();
        }));

        Debug.Log("Generación de palíndromos completa.");
    }

    static bool IsPalindrome(string text)
    {
        string cleanText = new string(text.Where(char.IsLetterOrDigit).ToArray()).ToLower();
        int length = cleanText.Length;

        for (int i = 0; i < length / 2; i++)
        {
            if (cleanText[i] != cleanText[length - 1 - i])
            {
                return false;
            }
        }
        return true;
    }

    private IEnumerator GetWordCombinationsCoroutine(List<string> words, int batchSize, Action<List<List<string>>> onBatchComplete)
    {
        int count = words.Count;
        totalCombinations = Mathf.Abs(count * count);  // Total de combinaciones posibles

        var batch = new List<List<string>>();

        // Variables para calcular el tiempo estimado
        float startTime = Time.time;
        float estimatedTimeRemaining = 0;
        float lastTimeUpdate = 0f;  // Almacena el último momento en que actualizamos el tiempo
        int cCombinations = 0;
        // Continuamos desde el valor guardado de processedCombinations
        // No reiniciamos processedCombinations a cero aquí
        // Ya debería estar correctamente inicializado al cargar el progreso guardado

        for (int i = 0; i < count; i++)
        {
            for (int j = i; j < count; j++)  // Usar j = i para evitar combinaciones duplicadas
            {
                batch.Add(new List<string> { words[i], words[j] });

                processedCombinations++;  // Aumentamos el contador de combinaciones procesadas
                cCombinations++;
                // Cada 3 segundos actualizamos el tiempo restante
                if (Time.time - lastTimeUpdate >= 3f)
                {
                    // Calcular el porcentaje de progreso
                    float progress = (float)processedCombinations / totalCombinations * 100;

                    // Calcular el tiempo transcurrido
                    float elapsedTime = Time.time - startTime;

                    // Estimar el tiempo restante
                    if (cCombinations > 0)  // Evitar división por cero
                    {
                        estimatedTimeRemaining = (elapsedTime / cCombinations) * (totalCombinations - cCombinations);
                    }

                    // Mostrar el progreso y el tiempo estimado
                    _progressText.text = $"{progress:F2}%\n{processedCombinations}/{totalCombinations} combinaciones procesadas\n" +
                                         $"Tiempo estimado restante: \n{FormatTime(estimatedTimeRemaining)}";

                    // Actualizamos el tiempo
                    lastTimeUpdate = Time.time;
                }

                // Cuando se alcanza el tamaño de lote, procesamos y limpiamos el lote
                if (batch.Count >= batchSize)
                {
                    onBatchComplete?.Invoke(batch);
                    batch.Clear();
                    yield return null;  // Esperar un frame para no bloquear el hilo principal
                }
            }
        }

        // Si quedan combinaciones no procesadas, las procesamos aquí
        if (batch.Count > 0)
        {
            onBatchComplete?.Invoke(batch);
        }

        Debug.Log("Generación de combinaciones completa.");
    }




    // Función para formatear el tiempo estimado restante
    private string FormatTime(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }

    private void SaveProgress()
    {
        string progressPath = Application.persistentDataPath + "/progreso.txt";
        File.WriteAllText(progressPath, processedCombinations.ToString());
    }

    // Cargar el progreso desde un archivo
    private void LoadProgress()
    {
        string progressPath = Application.persistentDataPath + "/progreso.txt";
        if (File.Exists(progressPath))
        {
            string progressText = File.ReadAllText(progressPath);
            if (int.TryParse(progressText, out int savedProgress))
            {
                processedCombinations = savedProgress;
                Debug.Log($"Progreso cargado: {processedCombinations}/{totalCombinations} combinaciones procesadas.");
            }
            else
            {
                Debug.LogWarning("No se pudo leer el progreso guardado.");
            }
        }
        else
        {
            Debug.Log("No se encontró archivo de progreso. Comenzando desde el principio.");
        }
    }


    // Cargar los palíndromos encontrados desde un archivo
    private void LoadFoundPalindromes()
    {
        string palindromesPath = Application.persistentDataPath + "/palindromos_encontrados.txt";
        if (File.Exists(palindromesPath))
        {
            string[] palindromes = File.ReadAllLines(palindromesPath);
            foundPalindromes = new HashSet<string>(palindromes);
            Debug.Log($"Palíndromos encontrados cargados: {foundPalindromes.Count} palíndromos.");
        }
        else
        {
            Debug.Log("No se encontró archivo de palíndromos encontrados. Comenzando con un archivo vacío.");
        }
    }

    // Guardar los palíndromos en un archivo
    private void SavePalindrome(string palindrome)
    {
        string palindromesPath = Application.persistentDataPath + "/palindromos_encontrados.txt";

        // Añadir el palíndromo al archivo (si no existe, lo crea)
        File.AppendAllText(palindromesPath, palindrome + Environment.NewLine);
        Debug.Log($"Palíndromo guardado: {palindrome}");
    }
}
