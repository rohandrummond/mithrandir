using Microsoft.Extensions.Options;
using mithrandir.Models;
using mithrandir.Models.DTOs;
using mithrandir.Options;
using StackExchange.Redis;

namespace mithrandir.Services;

public class RateLimitService : IRateLimitService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RateLimitService> _logger;
    private readonly RateLimitOptions _options;
    private readonly TimeProvider _timeProvider;
    

    public RateLimitService(
        IConnectionMultiplexer redis, 
        ILogger<RateLimitService> logger,
        IOptions<RateLimitOptions> options,
        TimeProvider timeProvider
        )
    {
        _redis = redis;
        _logger = logger;
        _options = options.Value;
        _timeProvider = timeProvider;
    }
    
    public async Task<RateLimitResult> CheckAndIncrementAsync(string keyHash, Tier tier)
    {
        _logger.LogInformation("Checking rate limit in Redis");
        
        // Get IDatabase object from StackExchange
        var db = _redis.GetDatabase();
        
        // Calculate current time block with rounded minutes
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var minutes = (now.Minute / _options.WindowMinutes) * _options.WindowMinutes;
        var timeBlock = new DateTimeOffset(
            now.Year, 
            now.Month, 
            now.Day, 
            now.Hour, 
            minutes, 
            0, 
            TimeSpan.Zero
        );
        
        // Build Redis key 
        var redisKey = $"rateLimit:{keyHash}:{timeBlock:yyyy-MM-dd-HH:mm}";
        
        // Call StringIncrementAsync function and store new count
        var requestCount = await db.StringIncrementAsync(redisKey);
        
        // If first request, set key to expire in 10 minutes
        if (requestCount == 1)
        {
            _logger.LogDebug("First request in window, setting expiry: Key = {RedisKey}", redisKey);
            await db.KeyExpireAsync(redisKey, TimeSpan.FromMinutes(_options.WindowMinutes));
        }
        else
        {
            _logger.LogDebug("Request count incremented: Count = {RequestCount}", requestCount);
        }
        
        // Set limit based on Tier
        var limit = tier == Tier.Free ? _options.FreeTierLimit : _options.ProTierLimit;
        
        // Check if request count is within limit
        var withinLimit = requestCount <= limit;
        
        // Compute how many minutes until reset
        var resetTime = timeBlock.AddMinutes(_options.WindowMinutes);
        var remaining = resetTime - now;
        var retryAfterSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
        
        if (!withinLimit)
        {
            _logger.LogWarning("Rate limit exceeded: Tier = {Tier}, Count = {RequestCount}, Limit = {Limit}, RetryAfter = {RetryAfter}s", 
                tier, requestCount, limit, retryAfterSeconds);
        }
        else
        {
            _logger.LogDebug("Rate limit check passed: Tier = {Tier}, Count = {RequestCount}, Limit = {Limit}, Remaining = {Remaining}",
                tier, requestCount, limit, Math.Max(0, limit - (int)requestCount));
        }

        // Return result
        return new RateLimitResult
        {
            Allowed = withinLimit,
            Remaining = Math.Max(0, limit - (int)requestCount),
            RetryAfterSeconds = retryAfterSeconds
        };
    }
}