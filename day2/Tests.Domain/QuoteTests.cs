using FluentAssertions;
using QuotesApi.Models;

namespace Tests.Domain;

public class QuoteTests
{
    [Fact]
    public void Create_returns_quote_for_valid_data()
    {
        var (quote, error) = Quote.Create("Ada Lovelace", "The best way to predict the future is to invent it.");

        quote.Should().NotBeNull();
        error.Should().BeNull();
        quote!.Author.Should().Be("Ada Lovelace");
        quote.Text.Should().Be("The best way to predict the future is to invent it.");
        quote.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_returns_error_when_author_is_blank()
    {
        var (quote, error) = Quote.Create("   ", "Text");

        quote.Should().BeNull();
        error.Should().NotBeNull();
        error!.PropertyName.Should().Be("author");
    }

    [Fact]
    public void Create_returns_error_when_text_is_too_long()
    {
        var longText = new string('A', 1001);

        var (quote, error) = Quote.Create("Author", longText);

        quote.Should().BeNull();
        error.Should().NotBeNull();
        error!.PropertyName.Should().Be("text");
    }

    [Fact]
    public void Soft_delete_marks_quote_as_deleted()
    {
        var (quote, _) = Quote.Create("Author", "Text");

        quote!.SoftDelete();

        quote.IsDeleted.Should().BeTrue();
    }
}