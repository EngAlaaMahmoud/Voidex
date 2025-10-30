using Application.Common.DTOs.ETAAuthentication;
using Application.Common.Services.ETAReceiptManager;
using FluentValidation;
using MediatR;

namespace Application.Features.ETAReceiptManager.Commands;

public class AuthenticateETAResult
{
    public AuthenticateResponseDto? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class AuthenticateETARequest : IRequest<AuthenticateETAResult>
{
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string PosSerial { get; init; } = string.Empty;
    public string? PosOsVersion { get; init; } = "WinPOS";
    public string? PosModelFramework { get; init; } = "DOTNET";
    public string PresharedKey { get; init; } = string.Empty;
}

public class AuthenticateETAValidator : AbstractValidator<AuthenticateETARequest>
{
    public AuthenticateETAValidator()
    {
        RuleFor(x => x.ClientId)
            .NotEmpty()
            .WithMessage("Client ID is required");

        RuleFor(x => x.ClientSecret)
            .NotEmpty()
            .WithMessage("Client Secret is required");

        RuleFor(x => x.PosSerial)
            .NotEmpty()
            .WithMessage("POS Serial is required");

        RuleFor(x => x.PresharedKey)
            .NotEmpty()
            .WithMessage("Pre-shared Key is required");
    }
}

public class AuthenticateETAHandler : IRequestHandler<AuthenticateETARequest, AuthenticateETAResult>
{
    private readonly IDirectETAIntegration _directETAIntegration;

    public AuthenticateETAHandler(IDirectETAIntegration directETAIntegration)
    {
        _directETAIntegration = directETAIntegration;
    }

    public async Task<AuthenticateETAResult> Handle(AuthenticateETARequest request, CancellationToken cancellationToken)
    {
        try
        {
            var authenticateRequest = new AuthenticateRequestDto
            {
                ClientId = request.ClientId,
                ClientSecret = request.ClientSecret,
                GrantType = "client_credentials"
            };

            var authenticateHeaders = new AuthenticateHeadersDto
            {
                PosSerial = request.PosSerial,
                PosOsVersion = request.PosOsVersion ?? "WinPOS",
                PosModelFramework = request.PosModelFramework ?? "DOTNET",
                PresharedKey = request.PresharedKey
            };

            var response = await _directETAIntegration.AuthenticateAsync(authenticateRequest, authenticateHeaders);

            return new AuthenticateETAResult
            {
                Data = response,
                IsSuccess = !string.IsNullOrEmpty(response.AccessToken),
                ErrorMessage = string.IsNullOrEmpty(response.AccessToken) ? "Authentication failed - no access token received" : null
            };
        }
        catch (Exception ex)
        {
            return new AuthenticateETAResult
            {
                IsSuccess = false,
                ErrorMessage = $"Authentication failed: {ex.Message}"
            };
        }
    }
} 