using System.Collections.Generic;

namespace HisaCat.DataBindEx
{
    public static class ContextEx
    {
        public static class Base
        {
            public interface IIdContext<TIDType> where TIDType : System.IComparable
            {
                TIDType ContextId { get; set; }
            }

            /// <summary>
            /// Context of contain one of id based collection
            /// </summary>
            public class IdCollectionContextBase<TContext, TIDType> : CollectionContextBase<TContext>
                where TContext : Slash.Unity.DataBind.Core.Data.Context, IIdContext<TIDType>
                where TIDType : System.IComparable
            {
                private Dictionary<TIDType, TContext> contextDict = null;
                private HashSet<TIDType> contextIds = null;

                public IdCollectionContextBase() : base()
                {
                    InitContextDic();
                }
                public IdCollectionContextBase(IEnumerable<TContext> items) : base(items)
                {
                    InitContextDic();
                }

                private void ItemAddedInternal(TIDType id, TContext context)
                {
                    this.contextDict.Add(id, context);
                    this.contextIds.Add(id);
                }
                private void ItemRemovedInternal(TIDType id)
                {
                    this.contextDict.Remove(id);
                    this.contextIds.Remove(id);
                }
                private void ItemClearedInternal()
                {
                    this.contextDict.Clear();
                    this.contextIds.Clear();
                }

                private void InitContextDic()
                {
                    this.contextDict = new Dictionary<TIDType, TContext>();
                    this.contextIds = new HashSet<TIDType>();
                    this.Collection.ItemAdded += Collection_ItemAdded;
                    this.Collection.ItemRemoved += Collection_ItemRemoved;
                    this.Collection.ItemInserted += Collection_ItemInserted;
                    this.Collection.ItemReplaced += Collection_ItemReplaced;
                    this.Collection.Cleared += Collection_Cleared;

                    using (var enumerator = this.Collection.GetEnumerator())
                    {
                        while (enumerator.MoveNext())
                        {
                            var context = enumerator.Current;
                            ItemAddedInternal(context.ContextId, context);
                        }
                    }
                }
                private void Collection_ItemAdded(object item)
                {
                    var _item = item as TContext;
                    ItemAddedInternal(_item.ContextId, _item);
                }
                private void Collection_ItemRemoved(object item)
                {
                    var _item = item as TContext;
                    ItemRemovedInternal(_item.ContextId);
                }
                private void Collection_ItemInserted(object item, int index)
                {
                    var _item = item as TContext;
                    ItemAddedInternal(_item.ContextId, _item);
                }
                private void Collection_ItemReplaced(int index, object previousItem, object newItem)
                {
                    Collection_ItemRemoved(previousItem);
                    Collection_ItemAdded(newItem);
                }
                private void Collection_Cleared()
                {
                    ItemClearedInternal();
                }

                public bool ContextIDExist(TIDType id)
                {
                    return this.contextDict.ContainsKey(id);
                }
                public TContext GetContextFromID(TIDType id)
                {
                    return this.contextDict[id];
                }
                public TContext GetContextFromIDOrNull(TIDType id)
                {
                    if (ContextIDExist(id))
                    {
                        return this.contextDict[id];
                    }
                    return null;
                }
                public bool TryGetContextFromID(TIDType id, out TContext result)
                {
                    if (ContextIDExist(id))
                    {
                        result = this.contextDict[id];
                        return true;
                    }
                    result = null;
                    return false;
                }
                public bool RemoveContextFromId(TIDType id)
                {
                    var context = GetContextFromIDOrNull(id);
                    if (context == null) return false;
                    return this.Collection.Remove(context);
                }
            }

            /// <summary>
            /// Context of contain one of collection
            /// </summary>
            public class CollectionContextBase<T> : Slash.Unity.DataBind.Core.Data.Context where T : Slash.Unity.DataBind.Core.Data.Context
            {
                private Slash.Unity.DataBind.Core.Data.Property<Slash.Unity.DataBind.Core.Data.Collection<T>> collectionProperty = new();
                public Slash.Unity.DataBind.Core.Data.Collection<T> Collection
                {
                    get { return this.collectionProperty.Value; }
                    set { this.collectionProperty.Value = value; }
                }

                public CollectionContextBase()
                {
                    this.Collection = new Slash.Unity.DataBind.Core.Data.Collection<T>();
                }
                public CollectionContextBase(IEnumerable<T> items)
                {
                    this.Collection = new Slash.Unity.DataBind.Core.Data.Collection<T>(items);
                }
            }
        }

        #region Updator function
        public class SourceElementNullException : System.Exception
        {
            public SourceElementNullException() { }
            public SourceElementNullException(string message) : base() { }
        }
        public class SourceElementDuplicatedException : System.Exception
        {
            public SourceElementDuplicatedException() { }
            public SourceElementDuplicatedException(string message) : base() { }
        }

        /// <summary>
        /// Updates the destination collection context by synchronizing it with the source collection based on unique IDs.<br/>
        /// Removes items from the destination if their ID is not present in the source collection.<br/>
        /// Adds new items to the destination if their ID does not already exist.<br/>
        /// Updates existing items in the destination if their ID matches an item in the source.<br/>
        /// Ensures that all IDs in the source collection are unique.<br/>
        /// Throws an exception if the destination or source is null.<br/>
        /// Throws an exception if duplicate IDs are found in the source collection.<br/>
        /// </summary>
        /// <param name="destination">The collection context that will be modified to match the source collection.</param>
        /// <param name="source">The data source that provides elements to synchronize with the destination collection.</param>
        /// <param name="sourceId">A function that extracts the unique ID from each item in the source collection.</param>
        /// <param name="bindSourceToContext">An optional function that applies data from the source to the corresponding context instance.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the destination or source is null.</exception>
        /// <exception cref="SourceElementNullException">Thrown if duplicate IDs are found in the source collection.</exception>
        public static void UpdateIdCollectionContext<TIDContext, TData, TIDType>
            (Base.IdCollectionContextBase<TIDContext, TIDType> destination,
            IEnumerable<TData> source,
            System.Func<TData, TIDType> sourceId,
            System.Action<TIDContext, TData> bindSourceToContext = null)
            where TIDContext : Slash.Unity.DataBind.Core.Data.Context, Base.IIdContext<TIDType>, new()
            where TIDType : System.IComparable
        {
            if (destination == null)
                throw new System.ArgumentNullException($"{nameof(UpdateIdCollectionContext)}: {nameof(destination)} is null!");
            if (source == null)
                throw new System.ArgumentNullException($"{nameof(UpdateIdCollectionContext)}: {nameof(source)} is null!");

            var sourceKeys = new HashSet<TIDType>();
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var id = sourceId(enumerator.Current);
                    if (sourceKeys.Contains(id))
                        throw new SourceElementNullException($"{nameof(UpdateIdCollectionContext)}: source id \"{id}\" duplicated!");
                    else
                        sourceKeys.Add(id);
                }
            }

            #region Remove removed items
            {
                List<TIDContext> removedItems = new List<TIDContext>();
                int oldItemCount = destination.Collection.Count;
                for (int i = 0; i < oldItemCount; i++)
                {
                    var curContext = destination.Collection[i];
                    if (sourceKeys.Contains(curContext.ContextId) == false)
                        removedItems.Add(curContext);
                }

                int removedItemCount = removedItems.Count;
                for (int i = 0; i < removedItemCount; i++)
                    destination.Collection.Remove(removedItems[i]);
            }
            #endregion

            #region Add & Update items
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var curData = enumerator.Current;

                    if (destination.TryGetContextFromID(sourceId(curData), out var context) == false)
                    {
                        var newContext = new TIDContext();
                        newContext.ContextId = sourceId(curData);

                        bindSourceToContext?.Invoke(newContext, curData);
                        destination.Collection.Add(newContext);
                    }
                    else
                    {
                        bindSourceToContext?.Invoke(context, curData);
                    }
                }
            }
            #endregion
        }

        /// <summary>
        /// Updates the destination collection context by synchronizing it with the source collection using direct reference matching.<br/>
        /// Removes items from the destination if they are not present in the source collection.<br/>
        /// Adds new items to the destination if they do not already exist.<br/>
        /// Ensures that all references in the source collection are unique.<br/>
        /// Throws an exception if the destination or source is null.<br/>
        /// Throws an exception if duplicate references are found in the source collection.<br/>
        /// </summary>
        /// <param name="destination">The collection context that will be modified to match the source collection.</param>
        /// <param name="source">The source collection that provides reference-based elements to synchronize with the destination.</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the destination or source is null.</exception>
        /// <exception cref="SourceElementDuplicatedException">Thrown if duplicate references are found in the source collection.</exception>
        public static void UpdateCollectionContextFromReference<TContext>
            (Base.CollectionContextBase<TContext> destination,
            IEnumerable<TContext> source)
            where TContext : Slash.Unity.DataBind.Core.Data.Context, new()
        {
            if (destination == null)
                throw new System.ArgumentNullException($"{nameof(UpdateCollectionContextFromReference)}: {nameof(destination)} is null!");
            if (source == null)
                throw new System.ArgumentNullException($"{nameof(UpdateCollectionContextFromReference)}: {nameof(source)} is null!");

            var sourceContexts = new HashSet<TContext>();
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var context = enumerator.Current;
                    if (sourceContexts.Contains(context))
                        throw new SourceElementDuplicatedException($"{nameof(UpdateCollectionContextFromReference)}: source context reference \"{context}\" duplicated!");
                    else
                        sourceContexts.Add(context);
                }
            }

            #region Remove removed items
            {
                List<TContext> removedItems = new List<TContext>();
                int oldItemCount = destination.Collection.Count;
                for (int i = 0; i < oldItemCount; i++)
                {
                    var curContext = destination.Collection[i];
                    if (sourceContexts.Contains(curContext) == false)
                        removedItems.Add(curContext);
                }

                int removedItemCount = removedItems.Count;
                for (int i = 0; i < removedItemCount; i++)
                    destination.Collection.Remove(removedItems[i]);
            }
            #endregion

            #region Add items
            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var curData = enumerator.Current;

                    if (destination.Collection.Contains(curData))
                        continue;

                    destination.Collection.Add(curData);
                }
            }
            #endregion
        }

        /// <summary>
        /// Updates the destination ID collection by synchronizing it with the source collection.<br/>
        /// Removes items that no longer exist in the source collection or do not meet the predicate condition.<br/>
        /// Adds new items from the source collection if they do not exist in the destination.<br/>
        /// Replaces existing items if they are already present in the destination.<br/>
        /// </summary>
        /// <param name="destination">The collection to be modified. It will be updated to match the source collection.</param>
        /// <param name="source">The reference collection that serves as the source for updates.</param>
        /// <param name="predicate">A predicate function to determine whether an item should be included in the destination collection. Defaults to always true if null.</param>
        public static void UpdateIdCollectionWhere<TIDContext, TIDType>(
            Base.IdCollectionContextBase<TIDContext, TIDType> destination,
            Base.IdCollectionContextBase<TIDContext, TIDType> source,
            System.Predicate<TIDContext> predicate = null)
            where TIDContext : Slash.Unity.DataBind.Core.Data.Context, Base.IIdContext<TIDType>, new()
            where TIDType : System.IComparable
        {
            predicate ??= (x) => true;

            #region Remove removed items
            {
                List<TIDContext> removedItems = new List<TIDContext>();
                int oldItemCount = destination.Collection.Count;
                for (int i = 0; i < oldItemCount; i++)
                {
                    var curContext = destination.Collection[i];
                    // It removed from origin collection or invalid anymore.
                    if (source.ContextIDExist(curContext.ContextId) == false || predicate(curContext) == false)
                        removedItems.Add(curContext);
                }

                int removedItemCount = removedItems.Count;
                for (int i = 0; i < removedItemCount; i++)
                    destination.Collection.Remove(removedItems[i]);
            }
            #endregion

            #region Add & Update new items
            int itemCategoriesCount = source.Collection.Count;
            for (int i = 0; i < itemCategoriesCount; i++)
            {
                var newContext = source.Collection[i];
                if (predicate(newContext))
                {
                    // Add if it not exist
                    if (destination.TryGetContextFromID(newContext.ContextId, out var context) == false)
                        destination.Collection.Add(newContext);
                    else // Replace if it already exist
                        destination.Collection[destination.Collection.IndexOf(context)] = newContext;
                }
            }
            #endregion
        }

        /// <summary>
        /// Populates the destination collection context with data from the source collection.<br/>
        /// Clears the destination collection and creates new context instances for each item in the source.<br/>
        /// Optionally applies a transformation function to map data from the source to the new context instances.<br/>
        /// </summary>
        /// <param name="destination">The collection context that will be populated with new context instances.</param>
        /// <param name="source">The data source that provides elements to populate the destination collection.</param>
        /// <param name="setData">An optional function to set data from the source into the newly created context instances.</param>
        public static void SetCollectionContext<TContext, TData>(
            Base.CollectionContextBase<TContext> destination,
            IEnumerable<TData> source,
            System.Action<TContext, TData> setData = null)
            where TContext : Slash.Unity.DataBind.Core.Data.Context, new()
            where TData : new()
        {
            destination.Collection.Clear();

            using (var enumerator = source.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var curData = enumerator.Current;

                    var newContext = new TContext();
                    setData?.Invoke(newContext, curData);
                    destination.Collection.Add(newContext);
                }
            }
        }
        #endregion Updator function
    }
}
