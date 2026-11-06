using ConvMVVM3.Core.Mvvm.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using Xunit;

namespace ConvMVVM3.Tests;

public class ObservableRangeCollectionTests
{
    [Theory]
    [InlineData("add")]
    [InlineData("replace")]
    [InlineData("remove")]
    [InlineData("clear")]
    public void BatchOperation_UpdatesContentsAndRaisesOneReset(string operation)
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 2, 3 });
        var changes = new List<NotifyCollectionChangedAction>();
        var properties = new List<string?>();
        collection.CollectionChanged += (_, e) => changes.Add(e.Action);
        ((INotifyPropertyChanged)collection).PropertyChanged += (_, e) => properties.Add(e.PropertyName);
        int[] expected;
        switch (operation)
        {
            case "add": collection.AddRange(new[] { 4, 5 }); expected = [1, 2, 2, 3, 4, 5]; break;
            case "replace": collection.ReplaceRange(new[] { 4, 5 }); expected = [4, 5]; break;
            case "remove": collection.RemoveRange(new[] { 2, 99 }); expected = [1, 3]; break;
            default: collection.ClearAndNotify(); expected = []; break;
        }
        Assert.Equal(expected, collection);
        Assert.Equal(new[] { NotifyCollectionChangedAction.Reset }, changes);
        Assert.Equal(new[] { "Count", "Item[]" }, properties);
    }

    [Fact]
    public void EmptyAddAndRemove_DoNotNotify()
    {
        var collection = new ObservableRangeCollection<int>();
        var count = 0;
        collection.CollectionChanged += (_, _) => count++;
        collection.AddRange(Array.Empty<int>());
        collection.RemoveRange(Array.Empty<int>());
        collection.ClearAndNotify();
        Assert.Equal(0, count);
        Assert.Throws<ArgumentNullException>(() => collection.AddRange(null!));
        Assert.Throws<ArgumentNullException>(() => collection.RemoveRange(null!));
        Assert.Throws<ArgumentNullException>(() => collection.ReplaceRange(null!));
    }
}
