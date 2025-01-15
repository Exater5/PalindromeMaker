using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

class InversedMatrix : MonoBehaviour
{
    private void Start()
    {
        Main();
    }

    static void Main()
    {
        List<List<int>> matrix = new List<List<int>> {
            new List<int> { 1, 3, 5, 2 },
            new List<int> { 4, 8, 1, 7 },
            new List<int> { 6, 2, 9, 3 },
            new List<int> { 5, 7, 4, 8 }
        };

        int result = MaximizeTopLeftSum(matrix);
        print(result);
    }

    static int MaximizeTopLeftSum(List<List<int>> matrix)
    {
        int rows = matrix.Count;
        int cols = matrix[0].Count;

        List<List<int>> rowCombinations = GenerateCombinations(rows);

        int maxSum = int.MinValue;

        foreach (var rowConfig in rowCombinations)
        {
            List<List<int>> modifiedMatrix = new List<List<int>>();
            for (int i = 0; i < rows; i++)
            {
                modifiedMatrix.Add(rowConfig[i] == 1 ? matrix[i].AsEnumerable().Reverse().ToList() : new List<int>(matrix[i]));
            }
            List<List<int>> transposed = Transpose(modifiedMatrix);
            List<List<int>> colCombinations = GenerateCombinations(cols);

            foreach (var colConfig in colCombinations)
            {
                var modifiedColumns = new List<List<int>>();
                for (int j = 0; j < cols; j++)
                {
                    modifiedColumns.Add(colConfig[j] == 1 ? transposed[j].AsEnumerable().Reverse().ToList() : new List<int>(transposed[j]));
                }
                var finalMatrix = Transpose(modifiedColumns);
                int currentSum = finalMatrix[0][0] + finalMatrix[0][1] + finalMatrix[1][0] + finalMatrix[1][1];
                maxSum = Math.Max(maxSum, currentSum);
            }
        }
        return maxSum;
    }

    static List<List<int>> Transpose(List<List<int>> matrix)
    {
        int rows = matrix.Count;
        int cols = matrix[0].Count;
        var transposed = new List<List<int>>();

        for (int j = 0; j < cols; j++)
        {
            var newRow = new List<int>();
            for (int i = 0; i < rows; i++)
            {
                newRow.Add(matrix[i][j]);
            }
            transposed.Add(newRow);
        }

        return transposed;
    }

    static List<List<int>> GenerateCombinations(int n)
    {
        int total = (int)Math.Pow(2, n);
        var combinations = new List<List<int>>();

        for (int i = 0; i < total; i++)
        {
            var combination = new List<int>();
            for (int j = 0; j < n; j++)
            {
                combination.Add((i >> j) & 1);
            }
            combinations.Add(combination);
        }
        return combinations;
    }
}