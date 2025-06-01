using dbmig.BL;
using dbmig.BL.Common;
using dbmig.BL.Database;

namespace dbmig.Console;

class Program
{
    static int Main(string[] args)
    {
        try
        {
            var commandInterpreter = InteractorFactory.GetCommandInterpreterInteractor();
            var parsedCommand = commandInterpreter.ParseArguments(args);

            if (!parsedCommand.IsSuccess)
            {
                System.Console.WriteLine($"Error: {parsedCommand.Message}");
                return 1;
            }

            if (parsedCommand.CommandInfo == null)
            {
                System.Console.WriteLine(parsedCommand.Message);
                return 0;
            }

            var databaseInteractor = InteractorFactory.GetDatabaseInteractor();
            var result = ExecuteCommand(databaseInteractor, parsedCommand.CommandInfo);

            System.Console.WriteLine(result.Message);
            return result.IsSuccess ? 0 : 1;
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Unexpected error: {ex.Message}");
            return 1;
        }
    }

    private static Result ExecuteCommand(DatabaseInteractor databaseInteractor, CommandInfo commandInfo)
    {
        return commandInfo.Action.ToLower() switch
        {
            "cleardb" => databaseInteractor.ClearDatabase(commandInfo.ConnectionString),
            "init" => databaseInteractor.InitializeMigration(
                commandInfo.ConnectionString,
                commandInfo.Parameters.GetValueOrDefault("MigrationTableName") as string),
            "migrate" => databaseInteractor.RunMigrations(
                commandInfo.ConnectionString,
                commandInfo.Parameters.GetValueOrDefault("Directory") as string ?? string.Empty,
                commandInfo.Parameters.GetValueOrDefault("MigrationTableName") as string),
            _ => new Result(false, $"Unknown action: {commandInfo.Action}")
        };
    }
}
