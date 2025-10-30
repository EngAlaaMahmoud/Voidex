using Application.Common.DTOs.ETAReceiptSubmission;
using Application.Common.Services.ETAReceiptManager;
using FluentValidation;
using MediatR;

namespace Application.Features.ETAReceiptManager.Commands;

public class SubmitReceiptsResult
{
    public SubmitReceiptResponseDto? Data { get; set; }
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SubmitReceiptsRequest : IRequest<SubmitReceiptsResult>
{
    public string AccessToken { get; init; } = string.Empty;
    public List<DocumentDto> Receipts { get; init; } = new();
    public List<DocumentSignatureDto> Signatures { get; init; } = new();
}

public class SubmitReceiptsValidator : AbstractValidator<SubmitReceiptsRequest>
{
    public SubmitReceiptsValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required");

        RuleFor(x => x.Receipts)
            .NotEmpty()
            .WithMessage("At least one receipt is required");

        RuleForEach(x => x.Receipts)
            .SetValidator(new DocumentValidator());
    }
}

public class DocumentValidator : AbstractValidator<DocumentDto>
{
    public DocumentValidator()
    {
        RuleFor(x => x.ReceiptNumber)
            .NotEmpty()
            .WithMessage("Receipt number is required");
    }
}

public class SubmitReceiptsHandler : IRequestHandler<SubmitReceiptsRequest, SubmitReceiptsResult>
{
    private readonly IDirectETAIntegration _directETAIntegration;

    public SubmitReceiptsHandler(IDirectETAIntegration directETAIntegration)
    {
        _directETAIntegration = directETAIntegration;
    }

    public async Task<SubmitReceiptsResult> Handle(SubmitReceiptsRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var submitRequest = new SubmitReceiptRequestDto
            {
                Receipts = request.Receipts,
                Signatures = request.Signatures
            };

            var response = await _directETAIntegration.SubmitReceiptAsync(submitRequest, request.AccessToken);

            return new SubmitReceiptsResult
            {
                Data = response,
                IsSuccess = !string.IsNullOrEmpty(response.SubmissionUUID),
                ErrorMessage = string.IsNullOrEmpty(response.SubmissionUUID) ? "Receipt submission failed" : null
            };
        }
        catch (Exception ex)
        {
            return new SubmitReceiptsResult
            {
                IsSuccess = false,
                ErrorMessage = $"Receipt submission failed: {ex.Message}"
            };
        }
    }
} 