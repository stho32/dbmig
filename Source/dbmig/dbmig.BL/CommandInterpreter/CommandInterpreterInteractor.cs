using dbmig.BL.Common;
using dbmig.BL.Logging;

namespace dbmig.BL.CommandInterpreter;

public class CommandInterpreterInteractor
{
    private readonly ILogger _logger;

    public CommandInterpreterInteractor()
    {
        _logger = LoggerFactory.Get();
    }

    public ParsedCommand ParseArguments(string[] args)
    {
        try
        {
            _logger.LogInfo($"Parsing command arguments: {string.Join(" ", args)}");

            if (args.Length == 0)
            {
                _logger.LogWarning("No arguments provided");
                return new ParsedCommand(null, false, "No arguments provided. Use --help for usage information.");
            }

            if (args.Contains("--help") || args.Contains("-h"))
            {
                _logger.LogInfo("Help requested");
                return new ParsedCommand(null, true, GetHelpText());
            }

            var connectionString = GetConnectionString(args);
            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogWarning("Connection string not provided");
                return new ParsedCommand(null, false, "Connection string is required. Use --connection-string or -c parameter.");
            }

            var action = GetAction(args);
            if (string.IsNullOrEmpty(action))
            {
                _logger.LogWarning("Action not provided or invalid");
                return new ParsedCommand(null, false, "Action is required. Supported actions: cleardb, init, migrate");
            }

            var parameters = GetActionParameters(args, action);
            var commandInfo = new CommandInfo(action, connectionString, parameters);

            _logger.LogInfo($"Command parsed successfully: Action={action}, Parameters={parameters.Count}");
            return new ParsedCommand(commandInfo, true, "Command parsed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to parse command arguments", ex);
            return new ParsedCommand(null, false, "Error parsing command. Please check your arguments and try again.");
        }
    }

    private string GetConnectionString(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--connection-string" || args[i] == "-c")
            {
                return args[i + 1];
            }
        }
        return string.Empty;
    }

    private string GetAction(string[] args)
    {
        var actions = new[] { "cleardb", "init", "migrate" };
        return args.FirstOrDefault(arg => actions.Contains(arg.ToLower())) ?? string.Empty;
    }

    private Dictionary<string, object> GetActionParameters(string[] args, string action)
    {
        var parameters = new Dictionary<string, object>();
        
        var actionIndex = Array.FindIndex(args, arg => arg.Equals(action, StringComparison.OrdinalIgnoreCase));
        
        switch (action.ToLower())
        {
            case "init":
                if (actionIndex + 1 < args.Length && !args[actionIndex + 1].StartsWith("-"))
                {
                    parameters["MigrationTableName"] = args[actionIndex + 1];
                }
                break;
                
            case "migrate":
                if (actionIndex + 1 < args.Length && !args[actionIndex + 1].StartsWith("-"))
                {
                    parameters["Directory"] = args[actionIndex + 1];
                }
                if (actionIndex + 2 < args.Length && !args[actionIndex + 2].StartsWith("-"))
                {
                    parameters["MigrationTableName"] = args[actionIndex + 2];
                }
                break;
        }
        
        return parameters;
    }

    private string GetHelpText()
    {
        return @"
dbmig - Database Migration Tool

Usage:
  dbmig --connection-string <connectionstring> <action> [options]
  dbmig -c <connectionstring> <action> [options]

Actions:
  cleardb                               Clear all tables and data from database
  init [table_name]                     Initialize migration system (default table: _Migrations)
  migrate <directory> [table_name]      Run migrations from directory

Examples:
  dbmig -c ""Server=localhost;Database=MyDb;Integrated Security=true;"" cleardb
  dbmig -c ""Server=localhost;Database=MyDb;Integrated Security=true;"" init
  dbmig -c ""Server=localhost;Database=MyDb;Integrated Security=true;"" migrate ./migrations
  dbmig -c ""Server=localhost;Database=MyDb;Integrated Security=true;"" migrate ./migrations CustomMigrations

Options:
  --help, -h                           Show this help message
";
    }
}