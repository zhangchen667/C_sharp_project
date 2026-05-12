using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;

namespace MyPersonalSpace.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 记录详细的异常信息到日志
        _logger.LogError(exception, 
            "全局异常捕获 | 请求路径: {Path} | 请求方法: {Method} | 异常类型: {ExceptionType} | 异常信息: {Message}",
            context.Request.Path,
            context.Request.Method,
            exception.GetType().Name,
            exception.Message);

        // 判断异常类型进行针对性处理
        if (exception is DbUpdateException || exception is MySqlConnector.MySqlException)
        {
            _logger.LogCritical("数据库操作异常: {Error}", exception.Message);
        }

        if (exception is IOException)
        {
            _logger.LogCritical("文件IO异常: {Error}", exception.Message);
        }

        context.Response.ContentType = "application/json; charset=utf-8";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            success = false,
            message = "服务器内部错误，请稍后重试",
            detail = _environment.IsDevelopment() ? exception.Message : null,
            stackTrace = _environment.IsDevelopment() ? exception.StackTrace : null
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}