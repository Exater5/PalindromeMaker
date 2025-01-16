using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using TMPro;
using System.IO;

class PalindromeGenerator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _progressText, _lastDebug;
    [SerializeField] private DictionaryInverter _inverter;
    static List<string> words;
    static HashSet<string> invertedWords;
    static HashSet<string> foundPalindromes;

    [SerializeField] private List<Index> _dictionaryIndexes, _inversedIndexes;
    private int processedCombinations = 0;
    private int totalCombinations;
    private Queue<string> lastPalindromes;

    private IEnumerator Start()
    {
        _dictionaryIndexes = new List<Index>();
        _inversedIndexes = new List<Index>();
        lastPalindromes = new Queue<string>(5);
        words = new List<string>();
        invertedWords = new HashSet<string>();
        foundPalindromes = new HashSet<string>();

        TextAsset wordsInputArchive = Resources.Load<TextAsset>("palabras");
        if (wordsInputArchive != null)
        {
            string[] allWords = wordsInputArchive.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            int batchSize = 500;

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
            LoadInvertedWords();
            LoadFoundPalindromes();
            LoadProgress();

            SetupIndexes();
            CalculateTotalCombinations();

            yield return StartCoroutine(Main());
        }
        else
        {
            Debug.LogError("No se pudo cargar el archivo de palabras.");
        }
    }

    private void SetupIndexes()
    {
        words.Sort();
        GenerateIndexes(words, _dictionaryIndexes);

        List<string> invertedWordsList = invertedWords.ToList();
        invertedWordsList.Sort();
        GenerateIndexes(invertedWordsList, _inversedIndexes);

        Debug.Log($"Índices del diccionario normal generados: {_dictionaryIndexes.Count}");
        Debug.Log($"Índices del diccionario invertido generados: {_inversedIndexes.Count}");
    }

    private void GenerateIndexes(List<string> wordList, List<Index> indexList)
    {
        Dictionary<char, Index> indexMap = new Dictionary<char, Index>();

        for (int i = 0; i < wordList.Count; i++)
        {
            char wordStartChar = NormalizeChar(wordList[i][0]);

            if (!indexMap.ContainsKey(wordStartChar))
                indexMap[wordStartChar] = new Index(wordStartChar, i, i);
            else
                indexMap[wordStartChar].lastIndex = i;
        }
        indexList.AddRange(indexMap.Values);
    }

    private char NormalizeChar(char c)
    {
        string normalized = c.ToString().Normalize(System.Text.NormalizationForm.FormD);
        char baseChar = normalized[0];
        return char.ToLower(baseChar);
    }

    public void LoadInvertedWords()
    {
        TextAsset invertedWordsInputArchive = Resources.Load<TextAsset>("palabras_invertidas");
        if (invertedWordsInputArchive != null)
        {
            string[] allInvertedWords = invertedWordsInputArchive.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            invertedWords = new HashSet<string>(allInvertedWords.Select(w => w.Trim()));
        }
        else
        {
            StartCoroutine(CrInvertWords());
            IEnumerator CrInvertWords()
            {
                _inverter.GenerateMirroredWords();
                yield return null;
                LoadInvertedWords();
            }
        }
    }

    private void CalculateTotalCombinations()
    {
        totalCombinations = 0;

        foreach (var normalIndex in _dictionaryIndexes)
        {
            var inversedIndex = _inversedIndexes.FirstOrDefault(index => index.startChar == normalIndex.startChar);

            if (inversedIndex != null)
            {
                int normalRange = normalIndex.lastIndex - normalIndex.firstIndex + 1;
                int inversedRange = inversedIndex.lastIndex - inversedIndex.firstIndex + 1;

                totalCombinations += normalRange * inversedRange;
            }
        }
    }

    private IEnumerator Main()
    {
        Debug.Log("Generando palíndromos...");

        int currentNormalIndex = 0;
        int currentInversedIndex = 0;
        bool resumeFromSavedProgress = processedCombinations > 0;

        foreach (var normalIndex in _dictionaryIndexes)
        {
            var inversedIndex = _inversedIndexes.FirstOrDefault(index => index.startChar == normalIndex.startChar);

            if (inversedIndex != null)
            {
                for (int i = normalIndex.firstIndex; i <= normalIndex.lastIndex; i++)
                {
                    if (resumeFromSavedProgress && currentNormalIndex < i)
                    {
                        currentNormalIndex++;
                        continue;
                    }

                    for (int j = inversedIndex.firstIndex; j <= inversedIndex.lastIndex; j++)
                    {
                        if (resumeFromSavedProgress && currentInversedIndex < j)
                        {
                            currentInversedIndex++;
                            continue;
                        }

                        string combined = $"{words[i]} {words[j]}";

                        if (IsPalindrome(combined) && !foundPalindromes.Contains(combined))
                        {
                            Debug.Log($"Palíndromo encontrado: {combined}");
                            foundPalindromes.Add(combined);
                            SavePalindrome(combined);
                        }

                        processedCombinations++;
                        currentInversedIndex = j;

                        if (processedCombinations % 1000 == 0)
                        {
                            UpdateProgress();
                            yield return null;
                        }
                    }
                    currentInversedIndex = inversedIndex.firstIndex;
                    currentNormalIndex = i;
                }
            }
        }

        Debug.Log("Generación de palíndromos completa.");
    }

    private void UpdateProgress()
    {
        float progress = (float)processedCombinations / totalCombinations * 100;
        _progressText.text = $"{progress:F2}% completado\n{processedCombinations}/{totalCombinations} combinaciones procesadas." +
            $"\n{foundPalindromes.Count} palíndromos encontrados";
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

    private void SavePalindrome(string palindrome)
    {
        if (lastPalindromes.Count == 5)
        {
            lastPalindromes.Dequeue();
        }
        lastPalindromes.Enqueue(palindrome);
        _lastDebug.text = "ÚLTIMOS<color=#FFBF00>";
        foreach (string s in lastPalindromes)
        {
            _lastDebug.text += $"\n{s}";
        }
        string palindromesPath = Application.persistentDataPath + "/palindromos_encontrados.txt";
        File.AppendAllText(palindromesPath, palindrome + Environment.NewLine);
    }
}

[Serializable]
public class Index
{
    public char startChar;
    public int firstIndex, lastIndex;

    public Index(char c, int fI, int lI)
    {
        startChar = c;
        firstIndex = fI;
        lastIndex = lI;
    }
}
