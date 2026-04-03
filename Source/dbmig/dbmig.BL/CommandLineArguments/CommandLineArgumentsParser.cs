using CommandLine;
using dbmig.BL.Common;

namespace dbmig.BL.CommandLineArguments;

public static class CommandLineArgumentsParser
{
    public static Result<ParsedOptions> Parse(string[] args)
    {
        var parserResult = Parser.Default.ParseArguments<ClearDbOptions, InitOptions, MigrateOptions>(args);

        ParsedOptions? parsed = null;
        string? errorMessage = null;

        parserResult
            .WithParsed<ClearDbOptions>(opts => parsed = new ParsedOptions(
                Action: "cleardb",
                ConnectionString: opts.ConnectionString,
                Directory: null,
                MigrationTableName: null,
                DryRun: false))
            .WithParsed<InitOptions>(opts => parsed = new ParsedOptions(
                Action: "init",
                ConnectionString: opts.ConnectionString,
                Directory: null,
                MigrationTableName: opts.MigrationTableName,
                DryRun: false))
            .WithParsed<MigrateOptions>(opts => parsed = new ParsedOptions(
                Action: "migrate",
                ConnectionString: opts.ConnectionString,
                Directory: opts.Directory,
                MigrationTableName: opts.MigrationTableName,
                DryRun: opts.DryRun))
            .WithNotParsed(errors =>
            {
                if (errors.Any(e => e is HelpRequestedError or VersionRequestedError))
                    errorMessage = null; // Help/version handled by CommandLineParser
                else
                    errorMessage = "Invalid arguments. Use --help for usage information.";
            });

        if (parsed != null)
            return new Result<ParsedOptions>(parsed, true, "Arguments parsed successfully.");

        if (errorMessage != null)
            return new Result<ParsedOptions>(null, false, errorMessage);

        // Help/version was requested — CommandLineParser already printed output
        return new Result<ParsedOptions>(null, true, string.Empty);
    }
}

public record ParsedOptions(
    string Action,
    string ConnectionString,
    string? Directory,
    string? MigrationTableName,
    bool DryRun
);
