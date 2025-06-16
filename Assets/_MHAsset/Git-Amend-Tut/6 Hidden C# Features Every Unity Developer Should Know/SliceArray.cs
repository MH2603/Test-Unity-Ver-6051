using System;
using UnityEngine;

namespace MH.Git_Amend
{
    public class SliceArray : MonoBehaviour
    {
        private void Start()
        {
            int[] numbers = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };

            int last = numbers[^1];
            int[] last3 = numbers[^3..];
            int last3int = numbers[^3];
            int[] first5 = numbers[..5];
            int[] middle = numbers[3..^3];
        }
    }
}