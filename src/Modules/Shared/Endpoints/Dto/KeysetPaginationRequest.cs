using Microsoft.AspNetCore.Mvc;

namespace src.Modules.Shared.Endpoints.Dto;

public abstract record KeysetPaginationRequest(int? Limit, string? PrevCursor, string? NextCursor);
