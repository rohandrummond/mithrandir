using mithrandir.Models;
using mithrandir.Models.DTOs;
using StackExchange.Redis;

namespace mithrandir.Services;

public class RateLimitService(IConnectionMultiplexer redis) : IRateLimitService
{
    private readonly IConnectionMultiplexer _redis = redis;
    
    public async Task<RateLimitResult> CheckAndIncrementAsync(string keyHash, Tier tier)
    {
        
        // Get IDatabase object from StackExchange
        var db = _redis.GetDatabase();
        
        // Calculate current time block with rounded minutes
        var now = DateTime.UtcNow;
        var minutes = (now.Minute / 10) * 10;
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
            await db.KeyExpireAsync(redisKey, TimeSpan.FromMinutes(10));
        }
        
        // Set limit based on Tier
        var limit = tier == Tier.Free ? 10 : 50;
        
        // Check if request count is within limit
        var withinLimit = requestCount <= limit;
        
        // Compute how many minutes until reset
        var resetTime = timeBlock.AddMinutes(10);
        var remaining = resetTime - now;
        var retryAfterSeconds = (int)Math.Ceiling(remaining.TotalSeconds);
        
        // Return result
        return new RateLimitResult
        {
            Allowed = withinLimit,
            Remaining = limit - (int)requestCount, 
            RetryAfterSeconds = retryAfterSeconds
        };
    }
}