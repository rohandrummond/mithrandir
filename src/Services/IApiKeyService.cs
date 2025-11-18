using mithrandir.Models.DTOs;

namespace mithrandir.Services;

public interface IApiKeyService
{
    Task<GenerateKeyResponse> GenerateKeyAsync(GenerateKeyRequest request);
    Task<ValidateKeyResponse> ValidateKeyAsync(ValidateKeyRequest request);
    Task<RevokeKeyResponse> RevokeKeyAsync(RevokeKeyRequest request);
    Task<DeleteKeyResponse> DeleteKeyAsync(DeleteKeyRequest request);
}