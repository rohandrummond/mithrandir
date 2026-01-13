namespace mithrandir.Services;

public interface IZodiacService
{
    string GenerateResponse(DateOnly dateOfBirth);
}
