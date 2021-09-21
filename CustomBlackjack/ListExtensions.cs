using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomBlackjack
{
    public static class ListExtensions
    {
        public static string ToJson(this IList<string> list)
        {
            if (list.Count == 0)
            {
                return "[]";
            }
            return "['" + string.Join("','", list) + "']";
        }

        public static void Shuffle<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            int num = list.Count / 2 + 1;
            int count = list.Count;
            for (int i = 0; i < num; i++)
            {
                list.Swap(i, ListExtensions.rnd.Next(i, count));
            }
        }

        public static void ShuffleFull<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            int count = list.Count;
            int count2 = list.Count;
            for (int i = 0; i < count; i++)
            {
                list.Swap(i, ListExtensions.rnd.Next(i, count2));
            }
        }

        public static void ShuffleOne<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            list.Swap(0, ListExtensions.rnd.Next(0, list.Count));
        }

        public static void ShuffleRandomOne<T>(this IList<T> list)
        {
            if (list.Count == 0)
            {
                return;
            }
            int num = UnityEngine.Random.Range(0, list.Count - 1);
            list.Swap(num, ListExtensions.rnd.Next(num, list.Count));
        }

        public static void Swap<T>(this IList<T> list, int i, int j)
        {
            T value = list[i];
            list[i] = list[j];
            list[j] = value;
        }

        public static T GetRandom<T>(this IList<T> list)
        {
            return list[ListExtensions.rnd.Next(0, list.Count)];
        }

        public static void RemoveAtSwapBack<T>(this List<T> list, T pObject)
        {
            int num = list.IndexOf(pObject);
            if (num == -1)
            {
                return;
            }
            int index = list.Count - 1;
            list[num] = list[index];
            list[index] = pObject;
            list.RemoveAt(index);
        }

        // Note: this type is marked as 'beforefieldinit'.
        static ListExtensions()
        {

        }

        private static System.Random rnd = new System.Random();
    }
}
