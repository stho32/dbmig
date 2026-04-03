using NUnit.Framework;
using dbmig.BL;
using dbmig.BL.Common;
using dbmig.BL.CommandInterpreter;
using dbmig.BL.Database;
using dbmig.BL.Configuration;

namespace dbmig.BL.IntegrationTests;

[TestFixture]
public class ConsoleApplicationIntegrationTests
{
    private CommandInterpreterInteractor _commandInterpreter = null!;
    private DatabaseInteractor _databaseInteractor = null!;

    [SetUp]
    public void SetUp()
    {
        _commandInterpreter = InteractorFactory.GetCommandInterpreterInteractor();
        _databaseInteractor = InteractorFactory.GetDatabaseInteractor();
    }

    [Test]
    public void EndToEnd_ClearDbCommand_WorksCorrectly()
    {
        // Setup - Create test database objects
        var setupSql = @"
            -- Create test tables
            CREATE TABLE TestTable1 (
                Id INT PRIMARY KEY,
                Name NVARCHAR(100)
            );
            
            CREATE TABLE TestTable2 (
                Id INT PRIMARY KEY,
                TestTable1Id INT,
                Description NVARCHAR(200),
                FOREIGN KEY (TestTable1Id) REFERENCES TestTable1(Id)
            );
            
            -- Create test stored procedure
            CREATE PROCEDURE sp_TestProcedure
            AS
            BEGIN
                SELECT 1 AS Result;
            END;
            
            -- Create test function
            CREATE FUNCTION fn_TestFunction(@input INT)
            RETURNS INT
            AS
            BEGIN
                RETURN @input * 2;
            END;
            
            -- Insert test data
            INSERT INTO TestTable1 (Id, Name) VALUES (1, 'Test Entry 1');
            INSERT INTO TestTable1 (Id, Name) VALUES (2, 'Test Entry 2');
            INSERT INTO TestTable2 (Id, TestTable1Id, Description) VALUES (1, 1, 'Related to Entry 1');
        ";
        
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            try
            {
                // Execute setup SQL to create test objects
                accessor.ExecuteAsync(setupSql).GetAwaiter().GetResult();
            }
            catch
            {
                // If objects already exist, that's okay for this test
            }
        }
        
        // Test - Clear the database
        var executeResult = _databaseInteractor.ClearDatabase(ConnectionStrings.IntegrationTest);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Is.EqualTo("Database cleared successfully."));
        
        // Verify database is empty
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            // Check for user tables
            var tableCountSql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_TYPE = 'BASE TABLE' 
                AND TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";
            
            var tableCount = accessor.QuerySingleAsync<int>(tableCountSql).GetAwaiter().GetResult();
            Assert.That(tableCount, Is.EqualTo(0), "Database should have no user tables after clearing");
            
            // Check for stored procedures
            var procCountSql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.ROUTINES 
                WHERE ROUTINE_TYPE = 'PROCEDURE' 
                AND ROUTINE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";
            
            var procCount = accessor.QuerySingleAsync<int>(procCountSql).GetAwaiter().GetResult();
            Assert.That(procCount, Is.EqualTo(0), "Database should have no user stored procedures after clearing");
            
            // Check for functions
            var funcCountSql = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.ROUTINES 
                WHERE ROUTINE_TYPE = 'FUNCTION' 
                AND ROUTINE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')";
            
            var funcCount = accessor.QuerySingleAsync<int>(funcCountSql).GetAwaiter().GetResult();
            Assert.That(funcCount, Is.EqualTo(0), "Database should have no user functions after clearing");
        }
    }

    [Test]
    public void EndToEnd_InitCommand_WithDefaultTable_WorksCorrectly()
    {
        // Setup - Clear any existing migration table
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            try
            {
                accessor.ExecuteAsync("DROP TABLE IF EXISTS [_Migrations]").GetAwaiter().GetResult();
            }
            catch
            {
                // Table might not exist, that's okay
            }
        }
        
        // Test
        var args = new[] { "-c", ConnectionStrings.IntegrationTest, "init" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var tableName = parseResult.CommandInfo!.Parameters.GetValueOrDefault("MigrationTableName") as string;
        var executeResult = _databaseInteractor.InitializeMigration(parseResult.CommandInfo.ConnectionString, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Is.EqualTo("Migration system initialized with table '_Migrations'."));
        
        // Verify table was created
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            var tableExists = accessor.TableExistsAsync("_Migrations").GetAwaiter().GetResult();
            Assert.That(tableExists, Is.True, "Migration table should exist after initialization");
            
            // Verify baseline migration was inserted
            var baselineExists = accessor.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM [_Migrations] WHERE MigrationName = '00000-Baseline'"
            ).GetAwaiter().GetResult();
            Assert.That(baselineExists, Is.EqualTo(1), "Baseline migration should be present");
        }
        
        // Cleanup
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            accessor.ExecuteAsync("DROP TABLE IF EXISTS [_Migrations]").GetAwaiter().GetResult();
        }
    }

    [Test]
    public void EndToEnd_InitCommand_WithCustomTable_WorksCorrectly()
    {
        // Setup - Clear any existing migration table
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            try
            {
                accessor.ExecuteAsync("DROP TABLE IF EXISTS [CustomMigrations]").GetAwaiter().GetResult();
            }
            catch
            {
                // Table might not exist, that's okay
            }
        }
        
        // Test
        var args = new[] { "-c", ConnectionStrings.IntegrationTest, "init", "CustomMigrations" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var tableName = parseResult.CommandInfo!.Parameters.GetValueOrDefault("MigrationTableName") as string;
        var executeResult = _databaseInteractor.InitializeMigration(parseResult.CommandInfo.ConnectionString, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.True);
        Assert.That(executeResult.Message, Is.EqualTo("Migration system initialized with table 'CustomMigrations'."));
        
        // Verify table was created with custom name
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            var tableExists = accessor.TableExistsAsync("CustomMigrations").GetAwaiter().GetResult();
            Assert.That(tableExists, Is.True, "Custom migration table should exist after initialization");
            
            // Verify baseline migration was inserted
            var baselineExists = accessor.QuerySingleAsync<int>(
                "SELECT COUNT(*) FROM [CustomMigrations] WHERE MigrationName = '00000-Baseline'"
            ).GetAwaiter().GetResult();
            Assert.That(baselineExists, Is.EqualTo(1), "Baseline migration should be present in custom table");
        }
        
        // Cleanup
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            accessor.ExecuteAsync("DROP TABLE IF EXISTS [CustomMigrations]").GetAwaiter().GetResult();
        }
    }

    [Test]
    public void EndToEnd_MigrateCommand_WithValidDirectory_WorksCorrectly()
    {
        // Setup - Initialize migration system first
        using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
        {
            try
            {
                accessor.ExecuteAsync("DROP TABLE IF EXISTS [_Migrations]").GetAwaiter().GetResult();
            }
            catch { }
        }
        
        var initResult = _databaseInteractor.InitializeMigration(ConnectionStrings.IntegrationTest);
        Assert.That(initResult.IsSuccess, Is.True, "Failed to initialize migration system for test");
        
        // Create a temporary directory with a test migration file
        var tempDir = Path.Combine(Path.GetTempPath(), "dbmig_test_migrations_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(tempDir);
        
        var migrationFile = Path.Combine(tempDir, "00001-test-migration.sql");
        File.WriteAllText(migrationFile, "-- Test migration\nCREATE TABLE TestMigrationTable (Id INT PRIMARY KEY, Name NVARCHAR(50));");
        
        try
        {
            // Test
            var args = new[] { "-c", ConnectionStrings.IntegrationTest, "migrate", tempDir };
            
            var parseResult = _commandInterpreter.ParseArguments(args);
            
            Assert.That(parseResult.IsSuccess, Is.True);
            Assert.That(parseResult.CommandInfo, Is.Not.Null);
            
            var directory = parseResult.CommandInfo!.Parameters.GetValueOrDefault("Directory") as string;
            var tableName = parseResult.CommandInfo.Parameters.GetValueOrDefault("MigrationTableName") as string;
            
            var executeResult = _databaseInteractor.RunMigrations(parseResult.CommandInfo.ConnectionString, directory ?? string.Empty, tableName);
            
            Assert.That(executeResult.IsSuccess, Is.True);
            Assert.That(executeResult.Message, Does.Contain("completed successfully"));
            Assert.That(executeResult.Message, Does.Contain("1 migration(s) applied"));

            // Verify the migration was actually executed - table should exist now
            using (var verifyAccessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
            {
                var tableExists = verifyAccessor.TableExistsAsync("TestMigrationTable").GetAwaiter().GetResult();
                Assert.That(tableExists, Is.True, "Migration should have created the TestMigrationTable");
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
            
            using (var accessor = new SqlServerDatabaseAccessor(ConnectionStrings.IntegrationTest))
            {
                try
                {
                    accessor.ExecuteAsync("DROP TABLE IF EXISTS [_Migrations]").GetAwaiter().GetResult();
                    accessor.ExecuteAsync("DROP TABLE IF EXISTS [TestMigrationTable]").GetAwaiter().GetResult();
                }
                catch { }
            }
        }
    }

    [Test]
    public void EndToEnd_MigrateCommand_WithNonExistentDirectory_ReturnsFailure()
    {
        var nonExistentDir = "./nonexistent-integration-test-dir";
        var args = new[] { "-c", ConnectionStrings.IntegrationTest, "migrate", nonExistentDir };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Not.Null);
        
        var directory = parseResult.CommandInfo!.Parameters.GetValueOrDefault("Directory") as string;
        var tableName = parseResult.CommandInfo.Parameters.GetValueOrDefault("MigrationTableName") as string;
        
        var executeResult = _databaseInteractor.RunMigrations(parseResult.CommandInfo.ConnectionString, directory ?? string.Empty, tableName);
        
        Assert.That(executeResult.IsSuccess, Is.False);
        Assert.That(executeResult.Message, Does.Contain("does not exist"));
    }

    [Test]
    public void EndToEnd_InvalidCommand_ReturnsParseFailure()
    {
        var args = new[] { "-c", ConnectionStrings.IntegrationTest, "invalidcommand" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.False);
        Assert.That(parseResult.CommandInfo, Is.Null);
        Assert.That(parseResult.Message, Does.Contain("Action is required"));
    }

    [Test]
    public void EndToEnd_MissingConnectionString_ReturnsParseFailure()
    {
        var args = new[] { "cleardb" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.False);
        Assert.That(parseResult.CommandInfo, Is.Null);
        Assert.That(parseResult.Message, Does.Contain("Connection string is required"));
    }

    [Test]
    public void EndToEnd_HelpCommand_ReturnsHelpText()
    {
        var args = new[] { "--help" };
        
        var parseResult = _commandInterpreter.ParseArguments(args);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        Assert.That(parseResult.CommandInfo, Is.Null);
        Assert.That(parseResult.Message, Does.Contain("dbmig - Database Migration Tool"));
        Assert.That(parseResult.Message, Does.Contain("Usage:"));
        Assert.That(parseResult.Message, Does.Contain("Examples:"));
    }

    [Test]
    public void InteractorFactory_ReturnsWorkingInteractors()
    {
        var commandInteractor = InteractorFactory.GetCommandInterpreterInteractor();
        var databaseInteractor = InteractorFactory.GetDatabaseInteractor();
        
        Assert.That(commandInteractor, Is.Not.Null);
        Assert.That(databaseInteractor, Is.Not.Null);
        
        // Test command parsing
        var testArgs = new[] { "-c", ConnectionStrings.IntegrationTest, "cleardb" };
        var parseResult = commandInteractor.ParseArguments(testArgs);
        
        Assert.That(parseResult.IsSuccess, Is.True);
        
        // Note: We don't actually execute ClearDatabase here as it would affect other tests
        // Just verify that the interactors are properly instantiated and can handle basic operations
    }
}
