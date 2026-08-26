namespace Ct.Core;
public sealed record ApiResponse<T>(int StatusCode, string Message, T? Data, int? TotalRecords = null)
{
    public static ApiResponse<T> Ok(T data, string message = "Success", int? totalRecords = null) => new(200, message, data, totalRecords);
}
