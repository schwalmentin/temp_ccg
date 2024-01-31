using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

public static class Tools
{
    public static void Shuffle<T>(this IList<T> list)
    {
        if (list == null)
        {
            return;
        }


        RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
        int n = list.Count;
        while (n > 1)
        {
            byte[] box = new byte[1];
            do provider.GetBytes(box);
            while (!(box[0] < n * (Byte.MaxValue / n)));
            int k = (box[0] % n);
            n--;
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static T[,] ArrayTo2DArray<T>(int sizeX, int sizeY, ref T[] array)
    {
        if (sizeX * sizeY != array.Length)
        {
            Debug.LogError($"The array size of {array.Length} doesn't match with the size X: {sizeX} and size Y: {sizeY}.");
            return null;
        }

        T[,] array2D = new T[sizeX, sizeY];

        for (int i = 0; i < array.Length; i++)
        {
            int x = i % sizeX;
            int y = Mathf.FloorToInt(i / sizeX);
            array2D[x, y] = array[i];
        }

        return array2D;
    }
}
