using Microsoft.EntityFrameworkCore;
using MyPersonalSpace.Data;
using MyPersonalSpace.Models;

namespace MyPersonalSpace.Services;

public class OperationLogService
{
    private readonly ApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OperationLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 异步记录操作日志
    /// </summary>
    public async Task LogAsync(string userId, string action, string description, string? targetId = null)
    {
        var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

        var log = new OperationLog
        {
            UserId = userId,
            Action = action,
            Description = description,
            TargetId = targetId,
            IpAddress = ipAddress,
            CreatedAt = DateTime.Now
        };

        _context.OperationLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// 获取所有操作日志（按时间倒序）
    /// </summary>
    public async Task<List<OperationLog>> GetLogsAsync(int page = 1, int pageSize = 20)
    {
        return await _context.OperationLogs
            .OrderByDescending(l => l.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    /// <summary>
    /// 获取指定用户的操作日志
    /// </summary>
    public async Task<List<OperationLog>> GetLogsByUserAsync(string userId)
    {
        return await _context.OperationLogs
            .Where(l => l.UserId == userId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }
}