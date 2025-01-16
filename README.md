Palindrome Generator
This project is a Palindrome Generator built in Unity. It explores an efficient approach to find palindromes by combining words from a given dictionary and their inverses. The main goal of this project is to process large datasets and find all unique palindromic combinations.

About
The project was created for fun and as a learning exercise, inspired by a problem-solving challenge on HackerRank. It aims to blend creativity with algorithmic thinking, leveraging Unity’s tools for progress tracking and file management.

Features
Efficiently processes large datasets of words to find palindromes.
Tracks progress across sessions, allowing the process to pause and resume seamlessly.
Stores discovered palindromes and progress locally to prevent reprocessing.
Uses Unity’s UI system to display real-time progress feedback.

How It Works
Input: The program loads a list of words from a text file (palabras.txt) and their inverses (palabras_invertidas.txt).
Indexing: It indexes the words and their inverses for quick lookup.
Palindrome Search: It combines words from the dictionary and their inverses, checking if the result is a palindrome.
Persistence: Found palindromes and processing progress are saved to local files, allowing the search to resume later.
Output: All found palindromes are saved in palindromos_encontrados.txt

Made in C# Unity V.2022.3.50f1
