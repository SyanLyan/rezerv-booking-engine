using Microsoft.AspNetCore.Mvc;
using Rezerv.Api.Contracts.Common;

namespace Rezerv.Api.Controllers;

public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult<ApiResponse<T>> OkResponse<T>(T data) =>
        Ok(ApiResponse<T>.Succeeded(data, ApiResponseMessages.Fetched));

    protected ActionResult<ApiResponse<T>> CreatedResponse<T>(T data) =>
        StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Succeeded(data, ApiResponseMessages.Created));

    protected ActionResult<ApiResponse<T>> UpdatedResponse<T>(T data) =>
        Ok(ApiResponse<T>.Succeeded(data, ApiResponseMessages.Updated));

    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(string message) =>
        BadRequest(ApiResponse<T>.Failed(ApiResponseMessages.RequestFailed, message));

    protected ActionResult<ApiResponse<T>> BadRequestResponse<T>(IReadOnlyList<string> errors) =>
        BadRequest(new ApiResponse<T>(false, ApiResponseMessages.RequestFailed, default, errors));

    protected ActionResult<ApiResponse<T>> NotFoundResponse<T>(string message) =>
        NotFound(ApiResponse<T>.Failed(ApiResponseMessages.RequestFailed, message));
}