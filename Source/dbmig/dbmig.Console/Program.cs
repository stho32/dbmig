using dbmig.BL;
using dbmig.BL.CommandLineArguments;
using dbmig.BL.Common;
using dbmig.BL.Database;

namespace dbmig.Console;

class Program
{
    static int Main(string[] args)
    {
        var parseResult = CommandLineArgumentsParser.Parse(args);

        if (!parseResult.IsSuccess)
        {
            if (!string.IsNullOrEmpty(parseResult.Message))
                System.Console.Error.WriteLine(parseResult.Message);
            return 1;
        }

        if (parseResult.Value == null)
            return 0; // Help/version was shown

        var options = parseResult.Value;
        var databaseInteractor = InteractorFactory.GetDatabaseInteractor();

        var result = ExecuteCommand(databaseInteractor, options);

        System.Console.WriteLine(result.Message);
        return result.IsSuccess ? 0 : 1;
    }

    private static Result ExecuteCommand(DatabaseInteractor databaseInteractor, ParsedOptions options)
    {
        return options.Action switch
        {
            "cleardb" => databaseInteractor.ClearDatabase(options.ConnectionString),
            "init" => databaseInteractor.InitializeMigration(
                options.ConnectionString,
                options.MigrationTableName),
            "migrate" => databaseInteractor.RunMigrations(
                options.ConnectionString,
                options.Directory ?? string.Empty,
                options.MigrationTableName,
                options.DryRun),
            _ => new Result(false, $"Unknown action: {options.Action}")
        };
    }
}
