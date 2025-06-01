using dbmig.BL.Database;
using NUnit.Framework;

namespace dbmig.BL.IntegrationTests.Database;

[TestFixture]
public class DatabaseInteractorIntegrationTests
{
    private const string ConnectionString = "Server=localhost;Database=dbmig_test;Integrated Security=true;";
    private DatabaseInteractor _interactor;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _interactor = new DatabaseInteractor();
    }

    [Test]
    public void ClearDatabase_Should_RemoveAllUserObjects()
    {
        // Arrange - Erstelle Testobjekte in der Datenbank
        using var connection = new Microsoft.Data.SqlClient.SqlConnection(ConnectionString);
        connection.Open();

        // Erstelle eine Tabelle mit Fremdschlüssel
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Categories')
            CREATE TABLE Categories (
                Id INT PRIMARY KEY,
                Name NVARCHAR(100)
            );

            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Products')
            CREATE TABLE Products (
                Id INT PRIMARY KEY,
                Name NVARCHAR(100),
                CategoryId INT,
                CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
            );", connection))
        {
            cmd.ExecuteNonQuery();
        }

        // Erstelle eine View
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'ProductsView')
            EXEC('CREATE VIEW ProductsView AS SELECT p.*, c.Name as CategoryName FROM Products p JOIN Categories c ON p.CategoryId = c.Id');", connection))
        {
            cmd.ExecuteNonQuery();
        }

        // Erstelle eine Stored Procedure
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sys.procedures WHERE name = 'GetProductsByCategory')
            EXEC('CREATE PROCEDURE GetProductsByCategory @CategoryId INT AS BEGIN SELECT * FROM Products WHERE CategoryId = @CategoryId END');", connection))
        {
            cmd.ExecuteNonQuery();
        }

        // Erstelle eine Funktion
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
            IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'GetCategoryName')
            EXEC('CREATE FUNCTION GetCategoryName(@CategoryId INT) RETURNS NVARCHAR(100) AS BEGIN DECLARE @Name NVARCHAR(100); SELECT @Name = Name FROM Categories WHERE Id = @CategoryId; RETURN @Name; END');", connection))
        {
            cmd.ExecuteNonQuery();
        }

        // Act - Führe ClearDatabase aus
        var result = _interactor.ClearDatabase(ConnectionString);

        // Assert
        Assert.That(result.Success, Is.True, "ClearDatabase sollte erfolgreich sein");
        Assert.That(result.Message, Is.EqualTo("Database cleared successfully."));

        // Prüfe, ob alle benutzerdefinierten Objekte entfernt wurden
        using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
            SELECT 
                (SELECT COUNT(*) FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo')) +
                (SELECT COUNT(*) FROM sys.views WHERE schema_id = SCHEMA_ID('dbo')) +
                (SELECT COUNT(*) FROM sys.procedures WHERE schema_id = SCHEMA_ID('dbo')) +
                (SELECT COUNT(*) FROM sys.objects WHERE type IN ('FN', 'IF', 'TF') AND schema_id = SCHEMA_ID('dbo'))
            AS ObjectCount;", connection))
        {
            var objectCount = (int)cmd.ExecuteScalar();
            Assert.That(objectCount, Is.EqualTo(0), "Es sollten keine benutzerdefinierten Objekte mehr in der Datenbank existieren");
        }
    }
}
