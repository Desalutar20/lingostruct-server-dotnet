namespace src.Modules.Shared.Endpoints.Dto;

public sealed record ApiResponse<T>(T Data)
    where T : notnull;

public sealed record ApiKeysetPaginatedResponse<T>(
    IReadOnlyList<T> Data,
    string? PrevCursor,
    string? NextCursor
)
    where T : notnull;
