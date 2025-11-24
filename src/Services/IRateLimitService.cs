using mithrandir.Models;
using mithrandir.Models.DTOs;

namespace mithrandir.Services;

public interface IRateLimitService
{
    Task<RateLimitResult> CheckAndIncrementAsync(string keyHash, Tier tier);
}