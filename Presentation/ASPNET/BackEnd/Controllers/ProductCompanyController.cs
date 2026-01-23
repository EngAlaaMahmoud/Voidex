using Application.Features.ProductCompanyManager.Commands;
using Application.Features.ProductCompanyManager.Queries;
using ASPNET.BackEnd.Common.Base;
using ASPNET.BackEnd.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASPNET.BackEnd.Controllers;

[Route("api/[controller]")]
public class ProductCompanyController : BaseApiController
{
    public ProductCompanyController(ISender sender) : base(sender)
    {
    }

    [Authorize]
    [HttpGet("GetProductCompanyList")]
    public async Task<ActionResult<ApiSuccessResult<GetProductCompanyListResult>>> GetProductCompanyListAsync(CancellationToken cancellationToken)
    {
        var request = new GetProductCompanyListRequest();
        var response = await _sender.Send(request, cancellationToken);
        return Ok(new ApiSuccessResult<GetProductCompanyListResult>
        {
            Code = StatusCodes.Status200OK,
            Message = "Success executing GetProductCompanyList",
            Content = response
        });
    }

    [Authorize]
    [HttpPost("CreateProductCompany")]
    public async Task<ActionResult<ApiSuccessResult<CreateProductCompanyResult>>> CreateProductCompanyAsync(CreateProductCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        return Ok(new ApiSuccessResult<CreateProductCompanyResult>
        {
            Code = StatusCodes.Status200OK,
            Message = "Success executing CreateProductCompany",
            Content = response
        });
    }

    [Authorize]
    [HttpPost("UpdateProductCompany")]
    public async Task<ActionResult<ApiSuccessResult<UpdateProductCompanyResult>>> UpdateProductCompanyAsync(UpdateProductCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        return Ok(new ApiSuccessResult<UpdateProductCompanyResult>
        {
            Code = StatusCodes.Status200OK,
            Message = "Success executing UpdateProductCompany",
            Content = response
        });
    }

    [Authorize]
    [HttpPost("DeleteProductCompany")]
    public async Task<ActionResult<ApiSuccessResult<DeleteProductCompanyResult>>> DeleteProductCompanyAsync(DeleteProductCompanyRequest request, CancellationToken cancellationToken)
    {
        var response = await _sender.Send(request, cancellationToken);
        return Ok(new ApiSuccessResult<DeleteProductCompanyResult>
        {
            Code = StatusCodes.Status200OK,
            Message = "Success executing DeleteProductCompany",
            Content = response
        });
    }
}
