using Rezerv.Application.Commands.Businesses;
using Rezerv.Application.DTOs.Businesses;

namespace Rezerv.Application.Services.Businesses;

public interface IBusinessService
{
    Task<IReadOnlyList<BusinessDto>> ListAsync(CancellationToken cancellationToken = default);

    Task<BusinessDto> CreateAsync(
        CreateBusinessCommand command,
        CancellationToken cancellationToken = default);
}