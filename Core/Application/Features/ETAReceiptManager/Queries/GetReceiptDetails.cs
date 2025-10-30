using Application.Common.DTOs.ETAReceiptDetails;
using Application.Common.Services.ETAReceiptManager;
using FluentValidation;
using MediatR;

namespace Application.Features.ETAReceiptManager.Queries;

public class GetReceiptDetailsResult
{
    public GetReceiptDetailsResponseDto? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class GetReceiptDetailsRequest : IRequest<GetReceiptDetailsResult>
{
    public string Uuid { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
    public DateTime? DateTimeIssued { get; init; }
}

public class GetReceiptDetailsValidator : AbstractValidator<GetReceiptDetailsRequest>
{
    public GetReceiptDetailsValidator()
    {
        RuleFor(x => x.Uuid)
            .NotEmpty()
            .WithMessage("Receipt UUID is required");

        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");
    }
}

public class GetReceiptDetailsHandler : IRequestHandler<GetReceiptDetailsRequest, GetReceiptDetailsResult>
{
    private readonly IDirectETAIntegration _directETAIntegration;

    public GetReceiptDetailsHandler(IDirectETAIntegration directETAIntegration)
    {
        _directETAIntegration = directETAIntegration;
    }

    public async Task<GetReceiptDetailsResult> Handle(GetReceiptDetailsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var getReceiptDetailsRequest = new GetReceiptDetailsRequestDto
            {
                Uuid = request.Uuid,
                DateTimeIssued = request.DateTimeIssued
            };

            var response = await _directETAIntegration.GetReceiptDetailsAsync(getReceiptDetailsRequest, request.AccessToken);

            return new GetReceiptDetailsResult
            {
                Data = response,
                IsSuccess = response != null && response.Receipt != null,
                ErrorMessage = response?.Receipt == null ? "Receipt not found" : null
            };
        }
        catch (Exception ex)
        {
            return new GetReceiptDetailsResult
            {
                IsSuccess = false,
                ErrorMessage = $"Failed to get receipt details: {ex.Message}"
            };
        }
    }
} 