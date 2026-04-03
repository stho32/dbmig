namespace dbmig.BL.Common;

public record CommandInfo(
    string Action,
    string ConnectionString,
    Dictionary<string, object> Parameters
);

public record ParsedCommand(
    CommandInfo? CommandInfo,
    bool IsSuccess,
    string Message
);