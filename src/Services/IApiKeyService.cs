using mithrandir.Models.DTOs;

namespace mithrandir.Services;

public interface IApiKeyService
{
    Task<GenerateKeyResponse> GenerateKeyAsync(GenerateKeyRequest request);
}