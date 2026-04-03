using CommandLine;

namespace dbmig.BL.CommandLineArguments;

[Verb("cleardb", HelpText = "Clear all tables and data from the database.")]
public class ClearDbOptions
{
    [Option('c', "connection-string", Required = true, HelpText = "SQL Server connection string.")]
    public string ConnectionString { get; set; } = string.Empty;
}

[Verb("init", HelpText = "Initialize the migration system (create migration table).")]
public class InitOptions
{
    [Option('c', "connection-string", Required = true, HelpText = "SQL Server connection string.")]
    public string ConnectionString { get; set; } = string.Empty;

    [Value(0, MetaName = "table-name", Required = false, HelpText = "Custom migration table name (default: _Migrations).")]
    public string? MigrationTableName { get; set; }
}

[Verb("migrate", HelpText = "Run migrations from a directory.")]
public class MigrateOptions
{
    [Option('c', "connection-string", Required = true, HelpText = "SQL Server connection string.")]
    public string ConnectionString { get; set; } = string.Empty;

    [Value(0, MetaName = "directory", Required = true, HelpText = "Directory containing migration SQL files.")]
    public string Directory { get; set; } = string.Empty;

    [Value(1, MetaName = "table-name", Required = false, HelpText = "Custom migration table name (default: _Migrations).")]
    public string? MigrationTableName { get; set; }

    [Option("dry-run", Required = false, HelpText = "Show what would be executed without making changes.")]
    public bool DryRun { get; set; }
}
