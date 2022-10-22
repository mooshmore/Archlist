using System;
using System.Collections.Generic;
using System.Linq;

namespace Helpers
{
    /// <summary>
    /// A class that holds method extensions for a System.Collections.Generic.List
    /// </summary>
    public static class ListExtensions
    {
        public static List<T> Move<T>(this List<T> list, int itemsCount)
        {
            var movedItems = list.Take(itemsCount).ToList();
            if (list.Count < itemsCount)
                list.Clear();
            else
                list.RemoveRange(0, itemsCount);
            return movedItems;
        }

        /// <summary>
        /// Adds a range to the collection.
        /// </summary>
        public static void AddRange<T>(this ICollection<T> destination, IEnumerable<T> source)
        {
            foreach (T item in source)
            {
                destination.Add(item);
            }
        }

        /// <summary>
        /// Converts the IList to a list.
        /// </summary>
        public static List<T> CreateList<T>(this IList<T> list) => new(list);
             
        /// <summary>
        /// Adds given items to the list.
        /// </summary>
        public static List<T> Add<T>(this List<T> list, params T[] items)
        {
            foreach (T item in items)
            {
                list.Add(item);
            }
            return list;
        }

        /// <summary>
        /// Removes a given amount of items from the front of the list.
        /// </summary>
        /// <param name="amountOfItems">The amount of items to delete. 1 by default.</param>
        /// <returns>A List<T> with the items removed.</returns>
        public static List<T> RemoveAtTheFront<T>(this List<T> list, int amountOfItems = 1)
        {
            for (int i = 0; i < amountOfItems; i++)
            {
                if (list.Count == 0) break;
                list.RemoveAt(0);
            }
            return list;
        }

        /// <summary>
        /// Removes a given amount of items from the back of the list.
        /// </summary>
        /// <param name="amountOfItems">The amount of items to delete. 1 by default.</param>
        /// <returns>A List<T> with the items removed.</returns>
        public static List<T> RemoveAtTheBack<T>(this List<T> list, int amountOfItems = 1)
        {
            for (int i = 0; i < amountOfItems; i++)
            {
                if (list.Count == 0) 
                    break;
                list.RemoveAt(list.Count - 1);
            }
            return list;
        }

        /// <summary>
        /// Removes the last item from the list.
        /// </summary>
        public static List<T> RemoveLast<T>(this List<T> list)
        {
            if (list.Count != 0)
                list.RemoveAt(list.Count - 1);
            return list;
        }

        /// <summary>
        /// Merges two lists with each other.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="addToList">The list that will merge the two lists.</param>
        /// <param name="addedList">The list that will be added to merged list.</param>
        public static void Merge<T>(ref List<T> addToList, List<T> addedList) => addToList = addToList.Concat(addedList).ToList();

        /// <summary>
        /// Removes the item and inserts it at the beginning of the list.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list to work on.</param>
        /// <param name="item">The item to change the position of.</param>
        public static void BringToFront<T>(this List<T> list, T item)
        {
            list.Remove(item);
            list.Insert(0, item);
        }

        /// <summary>
        /// Removes the item at the given index and inserts it at the beginning of the list.
        /// </summary>
        /// <typeparam name="T">The type of the list.</typeparam>
        /// <param name="list">The list to work on.</param>
        /// <param name="index">The index of the item that will be moved.</param>
        public static void BringToFront<T>(this List<T> list, int index)
        {
            T item = list[index];
            list.RemoveAt(index);
            list.Insert(0, item);
        }

        /// <summary>
        /// Checks if all of the items in the given list have the same values.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list to be checked.</param>
        /// <returns>True if all of the items are the same; false if they aren't.</returns>
        public static bool HasTheSameValues<T>(this List<T> list)
        {
            T baseItem = list[0];
            foreach (var item in list)
            {
                if (!baseItem.Equals(item))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if all of the items in the list are null, empty or zero.
        /// </summary>
        /// <returns>True if all of the items are null, empty or zero; False if not.</returns>
        public static bool IsEmptyOrZero<T>(this List<T> list)
        {
            foreach (var item in list)
            {
                if (item == null || item.ToString() == "" || item.ToString() == "0")
                    continue;
                else
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the given item is the last <paramref name="item"/> in the list.
        /// </summary>
        /// <param name="item">The item to check for.</param>
        /// <returns>True if the <paramref name="item"/> was the last item  in the list; False if not.</returns>
        public static bool IsLastItem<T>(this List<T> list, T item) => EqualityComparer<T>.Default.Equals(list.Last(), item);

        /// <summary>
        /// Returns a random item from the list.
        /// </summary>
        public static T Random<T>(this List<T> list)
        {
            Random random = new();
            int randomNumber = random.Next(list.Count);
            return list[randomNumber];
        }

        /// <summary>
        /// Removes all items from <paramref name="baseList"/> that also exist in the <paramref name="comparedList"/>. <br/>
        /// This returns a new list - it does not modify neither of the given lists.
        /// </summary>
        public static List<T> RemoveCoexistingItems<T>(this List<T> baseList, List<T> comparedList)
        {
            return baseList.Except(comparedList).ToList();
        }

        /// <summary>
        /// Removes items from the list until the list count will reach target items count.
        /// Items are removed from the back of the list.
        /// </summary>
        public static List<T> ReduceTo<T>(this List<T> list, int targetItemsCount)
        {
            if (targetItemsCount < list.Count)
                list.RemoveRange(targetItemsCount, list.Count - targetItemsCount);

            return list;
        }
    } 
}
