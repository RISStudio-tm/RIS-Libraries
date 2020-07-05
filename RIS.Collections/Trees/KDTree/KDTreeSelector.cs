using System;
using System.Collections.Generic;

namespace RIS.Collections.Trees
{
    internal static class KDTreeSelector
    {
        internal static int Select<T>(T[] array, int left, int right, int k, IComparer<T> comparer)
        {
            if (left == right)
                return left;

            int pivotIndex = MedianOfThree(array, left, right, comparer);
            int partitionedPivotIndex = Partition(array, left, right, pivotIndex, comparer);

            return partitionedPivotIndex == k
                ? k
                : k < partitionedPivotIndex
                    ? Select(array, left, partitionedPivotIndex - 1, k, comparer)
                    : Select(array, partitionedPivotIndex + 1, right, k, comparer);
        }

        private static int MedianOfThree<T>(T[] array, int left, int right, IComparer<T> comparer)
        {
            int mid = left + ((right - left) / 2);

            if (comparer.Compare(array[right], array[left]) < 0)
            {
                Swap(ref array[left], ref array[right]);
            }

            if (comparer.Compare(array[mid], array[left]) < 0)
            {
                Swap(ref array[mid], ref array[left]);
            }

            if (comparer.Compare(array[right], array[mid]) < 0)
            {
                Swap(ref array[right], ref array[mid]);
            }

            return mid;
        }

        private static int Partition<T>(T[] array, int left, int right, int pivotIndex, IComparer<T> comparer)
        {
            T pivotValue = array[pivotIndex];

            int i = left - 1;
            int j = right + 1;

            while (true)
            {
                do
                {
                    ++i;
                }
                while (comparer.Compare(array[i], pivotValue) <= 0);

                do
                {
                    --j;
                }
                while (comparer.Compare(array[j], pivotValue) > 0);

                if (i >= j)
                    return j;

                Swap(ref array[i], ref array[j]);
            }
        }

        internal static void Swap<T>(ref T a, ref T b)
        {
            T temp = a;

            a = b;
            b = temp;
        }
    }
}
