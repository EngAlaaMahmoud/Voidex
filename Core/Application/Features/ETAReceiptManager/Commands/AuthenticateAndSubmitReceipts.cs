using Application.Common.DTOs.ETAAuthentication;
using Application.Common.DTOs.ETAReceiptSubmission;
using Application.Common.Services.ETAReceiptManager;
using FluentValidation;
using MediatR;

namespace Application.Features.ETAReceiptManager.Commands;

public class AuthenticateAndSubmitReceiptsResult
{
    public AuthenticateResponseDto? AuthenticationData { get; set; }
    public SubmitReceiptResponseDto? SubmissionData { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public string? AccessToken { get; set; }
}

public class AuthenticateAndSubmitReceiptsRequest : IRequest<AuthenticateAndSubmitReceiptsResult>
{
    // Authentication properties
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string PosSerial { get; init; } = string.Empty;
    public string? PosOsVersion { get; init; } = "WinPOS";
    public string? PosModelFramework { get; init; } = "DOTNET";
    public string PresharedKey { get; init; } = string.Empty;
    
    // Submission properties
    public List<DocumentDto> Receipts { get; init; } = new();
    public List<DocumentSignatureDto> Signatures { get; init; } = new();
}

public class AuthenticateAndSubmitReceiptsValidator : AbstractValidator<AuthenticateAndSubmitReceiptsRequest>
{
    public AuthenticateAndSubmitReceiptsValidator()
    {
        // Authentication validation
        RuleFor(x => x.ClientId).NotEmpty().WithMessage("Client ID is required");
        RuleFor(x => x.ClientSecret).NotEmpty().WithMessage("Client Secret is required");
        RuleFor(x => x.PosSerial).NotEmpty().WithMessage("POS Serial is required");
        RuleFor(x => x.PresharedKey).NotEmpty().WithMessage("Pre-shared Key is required");
        
        // Submission validation
        RuleFor(x => x.Receipts).NotEmpty().WithMessage("At least one receipt is required");
    }
}

public class AuthenticateAndSubmitReceiptsHandler : IRequestHandler<AuthenticateAndSubmitReceiptsRequest, AuthenticateAndSubmitReceiptsResult>
{
    private readonly IDirectETAIntegration _directETAIntegration;

    public AuthenticateAndSubmitReceiptsHandler(IDirectETAIntegration directETAIntegration)
    {
        _directETAIntegration = directETAIntegration;
    }

    public async Task<AuthenticateAndSubmitReceiptsResult> Handle(AuthenticateAndSubmitReceiptsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            // Step 1: Authenticate
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

            var authResponse = await _directETAIntegration.AuthenticateAsync(authenticateRequest, authenticateHeaders);

            if (string.IsNullOrEmpty(authResponse.AccessToken))
            {
                return new AuthenticateAndSubmitReceiptsResult
                {
                    AuthenticationData = authResponse,
                    IsSuccess = false,
                    ErrorMessage = "Authentication failed - no access token received"
                };
            }

            // Step 2: Submit Receipts
            var submitRequest = new SubmitReceiptRequestDto
            {
                Receipts = request.Receipts,
                Signatures = request.Signatures
            };

            var submitResponse = await _directETAIntegration.SubmitReceiptAsync(submitRequest, authResponse.AccessToken);

            return new AuthenticateAndSubmitReceiptsResult
            {
                AuthenticationData = authResponse,
                SubmissionData = submitResponse,
                AccessToken = authResponse.AccessToken,
                IsSuccess = !string.IsNullOrEmpty(submitResponse.SubmissionUUID),
                ErrorMessage = string.IsNullOrEmpty(submitResponse.SubmissionUUID) ? "Receipt submission failed" : null
            };
        }
        catch (Exception ex)
        {
            return new AuthenticateAndSubmitReceiptsResult
            {
                IsSuccess = false,
                ErrorMessage = $"Operation failed: {ex.Message}"
            };
        }
    }
} 