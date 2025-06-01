using NUnit.Framework;
using dbmig.BL.Database;
using dbmig.BL.Configuration;
using Microsoft.Data.SqlClient;

namespace dbmig.BL.Tests.Database;

[TestFixture]
public class SqlServerDatabaseAccessorTests
{
    private string _validConnectionString = null!;
    private string _invalidConnectionString = null!;

    [SetUp]
    public void SetUp()
    {
        _validConnectionString = ConnectionStrings.UnitTest;
        _invalidConnectionString = ConnectionStrings.Invalid;
    }

    [Test]
    public void Constructor_WithValidConnectionString_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new SqlServerDatabaseAccessor(_validConnectionString));
    }

    [Test]
    public void Constructor_WithNullConnectionString_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new SqlServerDatabaseAccessor(null!));
    }

    [Test]
    public void Constructor_WithEmptyConnectionString_DoesNotThrow()
    {
        Assert.DoesNotThrow(() => new SqlServerDatabaseAccessor(""));
    }

    [Test]
    public void QuerySingleAsync_WithInvalidConnectionString_ThrowsException()
    {
        using var accessor = new SqlServerDatabaseAccessor(_invalidConnectionString);
        
        Assert.ThrowsAsync<SqlException>(async () => 
            await accessor.QuerySingleAsync<int>("SELECT 1"));
    }

    [Test]
    public void QueryAsync_WithInvalidConnectionString_ThrowsException()
    {
        using var accessor = new SqlServerDatabaseAccessor(_invalidConnectionString);
        
        Assert.ThrowsAsync<SqlException>(async () => 
            await accessor.QueryAsync<int>("SELECT 1"));
    }

    [Test]
    public void ExecuteAsync_WithInvalidConnectionString_ThrowsException()
    {
        using var accessor = new SqlServerDatabaseAccessor(_invalidConnectionString);
        
        Assert.ThrowsAsync<SqlException>(async () => 
            await accessor.ExecuteAsync("SELECT 1"));
    }

    [Test]
    public void ExecuteInTransactionAsync_WithInvalidConnectionString_ThrowsException()
    {
        using var accessor = new SqlServerDatabaseAccessor(_invalidConnectionString);
        
        Assert.ThrowsAsync<SqlException>(async () => 
            await accessor.ExecuteInTransactionAsync(async db => 
            {
                await db.ExecuteAsync("SELECT 1");
                return true;
            }));
    }

    [Test]
    public void TableExistsAsync_WithInvalidConnectionString_ThrowsException()
    {
        using var accessor = new SqlServerDatabaseAccessor(_invalidConnectionString);
        
        Assert.ThrowsAsync<SqlException>(async () => 
            await accessor.TableExistsAsync("TestTable"));
    }

    [Test]
    public async Task DatabaseExistsAsync_WithInvalidConnectionString_ReturnsFalse()
    {
        using var accessor = new SqlServerDatabaseAccessor(_invalidConnectionString);
        
        var result = await accessor.DatabaseExistsAsync();
        
        Assert.That(result, Is.False);
    }

    [Test]
    public void Dispose_DoesNotThrow()
    {
        var accessor = new SqlServerDatabaseAccessor(_validConnectionString);
        
        Assert.DoesNotThrow(() => accessor.Dispose());
    }

    [Test]
    public void Dispose_CalledTwice_DoesNotThrow()
    {
        var accessor = new SqlServerDatabaseAccessor(_validConnectionString);
        
        Assert.DoesNotThrow(() => 
        {
            accessor.Dispose();
            accessor.Dispose();
        });
    }
}