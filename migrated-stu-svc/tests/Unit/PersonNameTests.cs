using Ct.Modules.Admissions.Domain;
using Xunit;

namespace Ct.Tests.Unit;

public class PersonNameTests
{
    [Theory]
    [InlineData("john doe", "John", null, "Doe")]
    [InlineData("  john   michael   doe  ", "John", "Michael", "Doe")]
    [InlineData("John3 Doe!", "John", null, "Doe")]
    [InlineData("o'brien mcdonald", "O'Brien", null, "Mcdonald")]
    [InlineData("Madonna", "Madonna", null, "Madonna")]
    [InlineData("", "", null, "")]
    public void SetName_NormalizesAccordingToLegacyRules(string input, string expectedFirst, string? expectedMiddle, string expectedLast)
    {
        var person = new Person();
        person.SetName(input);

        Assert.Equal(expectedFirst, person.FirstName);
        Assert.Equal(expectedMiddle, person.MiddleName);
        Assert.Equal(expectedLast, person.LastName);
    }

    [Fact]
    public void Name_Getter_JoinsComponentsWithNormalizedWhitespace()
    {
        var person = new Person();
        person.SetName("jane   ann   smith");

        Assert.Equal("Jane Ann Smith", person.Name);
    }

    [Fact]
    public void DeriveVirtualId_UsesFirstFourCharsOfIdAndCampustrackDomain()
    {
        var person = new Person { Id = Guid.Parse("abcdef01-0000-0000-0000-000000000000") };
        person.SetName("john doe");

        var virtualId = person.DeriveVirtualId();

        // Guid "N" format has no dashes, so the first 4 characters of
        // abcdef01-0000-... are "abcd".
        Assert.Equal("john.doe.abcd@campustrack.net", virtualId);
    }

    [Fact]
    public void EnsureVirtualId_DoesNotOverwriteExplicitlySetValue()
    {
        var person = new Person { VirtualId = "explicit@campustrack.net" };
        person.SetName("jane doe");

        person.EnsureVirtualId();

        Assert.Equal("explicit@campustrack.net", person.VirtualId);
    }
}
