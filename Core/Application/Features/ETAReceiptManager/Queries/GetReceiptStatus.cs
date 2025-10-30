using Application.Common.DTOs.ETAReceiptDetails;
using Application.Common.Services.ETAReceiptManager;
using FluentValidation;
using MediatR;

namespace Application.Features.ETAReceiptManager.Queries;

public class GetReceiptStatusResult
{
    public string? Uuid { get; set; }
    public string? Status { get; set; }
    public string? StatusReason { get; set; }
    public DateTime? DateTimeReceived { get; set; }
    public DateTime? DateTimeIssued { get; set; }
    public string? ReceiptNumber { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class GetReceiptStatusRequest : IRequest<GetReceiptStatusResult>
{
    public string Uuid { get; init; } = string.Empty;
    public string AccessToken { get; init; } = string.Empty;
}

public class GetReceiptStatusValidator : AbstractValidator<GetReceiptStatusRequest>
{
    public GetReceiptStatusValidator()
    {
        RuleFor(x => x.Uuid)
            .NotEmpty()
            .WithMessage("Receipt UUID is required");
            
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");
    }
}

public class GetReceiptStatusHandler : IRequestHandler<GetReceiptStatusRequest, GetReceiptStatusResult>
{
    private readonly IDirectETAIntegration _directETAIntegration;

    public GetReceiptStatusHandler(IDirectETAIntegration directETAIntegration)
    {
        _directETAIntegration = directETAIntegration;
    }

    public async Task<GetReceiptStatusResult> Handle(GetReceiptStatusRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var getReceiptDetailsRequest = new GetReceiptDetailsRequestDto
            {
                Uuid = request.Uuid
            };

            var response = await _directETAIntegration.GetReceiptDetailsAsync(getReceiptDetailsRequest, request.AccessToken);

            if (response?.Receipt != null)
            {
                return new GetReceiptStatusResult
                {
                    Uuid = response.Receipt.Uuid,
                    Status = response.Receipt.Status,
                    StatusReason = response.Receipt.StatusReason,
                    DateTimeReceived = response.DateTimeReceived,
                    DateTimeIssued = response.DateTimeIssued,
                    ReceiptNumber = response.Receipt.ReceiptNumber,
                    IsSuccess = true
                };
            }

            return new GetReceiptStatusResult
            {
                IsSuccess = false,
                ErrorMessage = "Receipt not found or not accessible"
            };
        }
        catch (Exception ex)
        {
            return new GetReceiptStatusResult
            {
                IsSuccess = false,
                ErrorMessage = $"Error getting receipt status: {ex.Message}"
            };
        }
    }
} 