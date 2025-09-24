using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using TMPro;

public class SortManager : MonoBehaviour
{
    public TMP_Text resultText;
    private int[] data;

    void Start()
    {
        data = GenerateRandomArray(10000);
    }

    int[] GenerateRandomArray(int size)
    {
        int[] arr = new int[size];
        System.Random rand = new System.Random();
        for (int i = 0; i < size; i++)
            arr[i] = rand.Next(0, 10000);
        return arr;
    }

    public void RunSelectionSort()
    {
        int[] copy = (int[])data.Clone();
        long ms = MeasureTime(() => SelectionSortTest.StartSelectionSort(copy));
        ShowResult("select", ms);
    }

    public void RunBubbleSort()
    {
        int[] copy = (int[])data.Clone();
        long ms = MeasureTime(() => BubbleSortTest.StartBubbleSort(copy));
        ShowResult("bubble", ms);
    }

    public void RunQuickSort()
    {
        int[] copy = (int[])data.Clone();
        long ms = MeasureTime(() => QuickSortTest.StartQuickSort(copy, 0, copy.Length - 1));
        ShowResult("quick", ms);
    }

    long MeasureTime(System.Action action)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        action();
        sw.Stop();
        return sw.ElapsedMilliseconds;
    }

    void ShowResult(string sortName, long ms)
    {
        string message = $"{sortName}: {ms} ms";
        resultText.text = message;
        UnityEngine.Debug.Log(message);
    }
}