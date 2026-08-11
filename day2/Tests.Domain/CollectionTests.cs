using FluentAssertions;
using QuotesApi.Models;

namespace Tests.Domain;

public class CollectionTests
{
    [Fact]
    public void Empty_name_throws()
    {
        var act = () => new Collection("", 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Name_over_80_characters_throws()
    {
        var name = new string('A', 81);

        var act = () => new Collection(name, 1);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Adding_51st_item_throws()
    {
        var collection = new Collection("My Collection", 1);

        for (var i = 1; i <= 50; i++)
            collection.AddItem(i, DateTimeOffset.UtcNow);

        var act = () => collection.AddItem(51, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Duplicate_quote_id_throws()
    {
        var collection = new Collection("My Collection", 1);

        collection.AddItem(1, DateTimeOffset.UtcNow);

        var act = () => collection.AddItem(1, DateTimeOffset.UtcNow);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Removing_nonexistent_item_throws()
    {
        var collection = new Collection("My Collection", 1);

        var act = () => collection.RemoveItem(999);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Adding_then_removing_leaves_zero_items()
    {
        var collection = new Collection("My Collection", 1);

        collection.AddItem(1, DateTimeOffset.UtcNow);
        collection.RemoveItem(1);

        collection.Items.Should().BeEmpty();
    }
}