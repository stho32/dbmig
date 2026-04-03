namespace dbmig.BL.Common;

public record Result(bool IsSuccess, string Message);

public record Result<T>(T? Value, bool IsSuccess, string Message);
