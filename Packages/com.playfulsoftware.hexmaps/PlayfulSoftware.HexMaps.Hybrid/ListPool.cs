using System.Collections.Generic;
namespace PlayfulSoftware.HexMaps.Hybrid
{
    internal static class ListPool<T>
    {
        static Stack<List<T>> s_Stack = new Stack<List<T>>();

        internal static void Add(List<T> list)
        {
            list.Clear();
            s_Stack.Push(list);
        }

        internal static List<T> Get() => s_Stack.Count > 0 ? s_Stack.Pop() : new List<T>();
    }
}