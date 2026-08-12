using FluentAssertions;
using QuotesApi.Models;

namespace Quotes.Tests.Unit.Models;

public class QuoteTests
{
    [Fact]
    public void Create_ValidAuthorAndText_ReturnsQuote()
    {
        var author = "Albert Einstein";
        var text = "Life is like riding a bicycle.";
        var userId = 1;

        var (quote, error) = Quote.Create(author, text, userId);

        quote.Should().NotBeNull();
        error.Should().BeNull();
        quote!.Author.Should().Be(author);
        quote.Text.Should().Be(text);
        quote.UserId.Should().Be(userId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_InvalidAuthor_ReturnsAuthorValidationError(string author)
    {
        var text = "Valid quote text";
        var userId = 1;

        var (quote, error) = Quote.Create(author, text, userId);

        quote.Should().BeNull();
        error.Should().NotBeNull();
        error!.PropertyName.Should().Be("author");
        error.Message.Should().Be("Author must be between 1 and 200 characters.");
    }

    [Fact]
    public void Create_AuthorExceeds200Characters_ReturnsAuthorValidationError()
    {
        var author = new string('A', 201);
        var text = "Valid quote text";
        var userId = 1;

        var (quote, error) = Quote.Create(author, text, userId);

        quote.Should().BeNull();
        error.Should().NotBeNull();
        error!.PropertyName.Should().Be("author");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Create_InvalidText_ReturnsTextValidationError(string text)
    {
        var author = "Valid Author";
        var userId = 1;

        var (quote, error) = Quote.Create(author, text, userId);

        quote.Should().BeNull();
        error.Should().NotBeNull();
        error!.PropertyName.Should().Be("text");
        error.Message.Should().Be("Text must be between 1 and 1000 characters.");
    }

    [Fact]
    public void Create_TextExceeds1000Characters_ReturnsTextValidationError()
    {
        var author = "Valid Author";
        var text = new string('A', 1001);
        var userId = 1;

        var (quote, error) = Quote.Create(author, text, userId);

        quote.Should().BeNull();
        error.Should().NotBeNull();
        error!.PropertyName.Should().Be("text");
        error.Message.Should().Be("Text must be between 1 and 1000 characters.");
    }
    [Fact]
public void Create_AuthorAndTextHaveWhitespace_TrimsValues()
{
    var author = "  Albert Einstein  ";
    var text = "  Life is like riding a bicycle.  ";
    var userId = 1;

    var (quote, error) = Quote.Create(author, text, userId);

    quote.Should().NotBeNull();
    error.Should().BeNull();
    quote!.Author.Should().Be("Albert Einstein");
    quote.Text.Should().Be("Life is like riding a bicycle.");
}
[Fact]
public void Create_AuthorExactly200Characters_Succeeds()
{
    var author = new string('A', 200);
    var text = "Valid quote text";
    var userId = 1;

    var (quote, error) = Quote.Create(author, text, userId);

    quote.Should().NotBeNull();
    error.Should().BeNull();
    quote!.Author.Should().HaveLength(200);
}

[Fact]
public void Create_TextExactly1000Characters_Succeeds()
{
    var author = "Valid Author";
    var text = new string('A', 1000);
    var userId = 1;

    var (quote, error) = Quote.Create(author, text, userId);

    quote.Should().NotBeNull();
    error.Should().BeNull();
    quote!.Text.Should().HaveLength(1000);
}

[Fact]
public void Create_UserIdIsPreserved()
{
    var author = "Valid Author";
    var text = "Valid quote text";
    var userId = 42;

    var (quote, error) = Quote.Create(author, text, userId);

    quote.Should().NotBeNull();
    error.Should().BeNull();
    quote!.UserId.Should().Be(42);
}
}