using System.Text.RegularExpressions;

namespace dbmig.BL.Common;

public static partial class ValidationHelper
{
    private static readonly Regex _validTableNameRegex = CreateTableNameRegex();

    /// <summary>
    /// Validates a SQL Server table name to prevent SQL injection.
    /// Allows alphanumeric characters and underscores, starting with a letter or underscore.
    /// Maximum length: 128 characters (SQL Server identifier limit).
    /// </summary>
    public static bool IsValidTableName(string? tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName))
            return false;

        return _validTableNameRegex.IsMatch(tableName);
    }

    [GeneratedRegex(@"^[a-zA-Z_][a-zA-Z0-9_]{0,127}$")]
    private static partial Regex CreateTableNameRegex();
}
