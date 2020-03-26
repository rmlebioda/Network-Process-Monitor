using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace NetworkProcessMonitor.Models
{
    public partial class StableSortableBindingList<T> : BindingList<T>
    {
        public static ListSortDirection GetCompatibleListSortOrderFrom(SortOrder sortOrder)
        {
            switch (sortOrder)
            {
                case SortOrder.Ascending:
                    return ListSortDirection.Ascending;
                default:
                    return ListSortDirection.Descending;
            }
        }

        public void InternalSort(IComparer<T> comparer)
        {
            List<T> itemsList = (List<T>)this.Items;
            itemsList.Sort(comparer);
        }

        public void InternalStableSort(IComparer<T> comparer)
        {
            List<T> list = (List<T>)this.Items;
            var pairs = list.Select((value, index) => Tuple.Create(value, index)).ToList();
            pairs.Sort((x, y) =>
            {
                int result = comparer.Compare(x.Item1, y.Item1);
                return result != 0 ? result : x.Item2 - y.Item2;
            });
            list.Clear();
            list.AddRange(pairs.Select(key => key.Item1));
        }
    }
}
