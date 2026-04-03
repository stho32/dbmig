namespace dbmig.BL.Database.Models;

public record Migration(
    long Id,
    string MigrationName,
    DateTime DiscoveredAt,
    string? ErrorMessage,
    DateTime? AppliedAt,
    string? Hash
);

public record NewMigration(
    string MigrationName,
    DateTime DiscoveredAt,
    string? ErrorMessage = null,
    DateTime? AppliedAt = null,
    string? Hash = null
);
