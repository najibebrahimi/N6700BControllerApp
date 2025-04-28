
using System.Collections.Specialized;
using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Media;

namespace N6700BControllerApp.Behaviours
{

    public class ScrollToEndBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            // Subscribe to the Items collection change if it implements INotifyCollectionChanged.
            if (AssociatedObject?.Items is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged += OnItemsCollectionChanged;
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject?.Items is INotifyCollectionChanged notifyCollection)
            {
                notifyCollection.CollectionChanged -= OnItemsCollectionChanged;
            }
            base.OnDetaching();
        }
        private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // Locate the ScrollViewer inside the ListBox's visual tree.
            var scrollViewer = AssociatedObject.FindDescendantOfType<ScrollViewer>();
            if (scrollViewer != null)
            {
                // Call ScrollToEnd() to scroll to the bottom.
                scrollViewer.ScrollToEnd();
            }
        }
    }

}
