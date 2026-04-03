using NUnit.Framework;
using dbmig.BL.Common;

namespace dbmig.BL.Tests.Common;

[TestFixture]
public class ValidationHelperTests
{
    [Test]
    public void IsValidTableName_WithValidName_ReturnsTrue()
    {
        Assert.That(ValidationHelper.IsValidTableName("_Migrations"), Is.True);
    }

    [Test]
    public void IsValidTableName_WithAlphanumericName_ReturnsTrue()
    {
        Assert.That(ValidationHelper.IsValidTableName("MyTable123"), Is.True);
    }

    [Test]
    public void IsValidTableName_WithUnderscorePrefix_ReturnsTrue()
    {
        Assert.That(ValidationHelper.IsValidTableName("_MyTable"), Is.True);
    }

    [Test]
    public void IsValidTableName_WithSimpleName_ReturnsTrue()
    {
        Assert.That(ValidationHelper.IsValidTableName("Users"), Is.True);
    }

    [Test]
    public void IsValidTableName_WithNull_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName(null), Is.False);
    }

    [Test]
    public void IsValidTableName_WithEmpty_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName(""), Is.False);
    }

    [Test]
    public void IsValidTableName_WithWhitespace_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("   "), Is.False);
    }

    [Test]
    public void IsValidTableName_WithSqlInjection_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("'; DROP TABLE Users; --"), Is.False);
    }

    [Test]
    public void IsValidTableName_WithBrackets_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("[MyTable]"), Is.False);
    }

    [Test]
    public void IsValidTableName_WithDot_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("dbo.MyTable"), Is.False);
    }

    [Test]
    public void IsValidTableName_WithSpaces_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("My Table"), Is.False);
    }

    [Test]
    public void IsValidTableName_WithDash_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("My-Table"), Is.False);
    }

    [Test]
    public void IsValidTableName_StartingWithNumber_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("1Table"), Is.False);
    }

    [Test]
    public void IsValidTableName_WithSemicolon_ReturnsFalse()
    {
        Assert.That(ValidationHelper.IsValidTableName("table;"), Is.False);
    }

    [Test]
    public void IsValidTableName_ExceedingMaxLength_ReturnsFalse()
    {
        var longName = new string('A', 129);
        Assert.That(ValidationHelper.IsValidTableName(longName), Is.False);
    }

    [Test]
    public void IsValidTableName_AtMaxLength_ReturnsTrue()
    {
        var maxName = new string('A', 128);
        Assert.That(ValidationHelper.IsValidTableName(maxName), Is.True);
    }
}
