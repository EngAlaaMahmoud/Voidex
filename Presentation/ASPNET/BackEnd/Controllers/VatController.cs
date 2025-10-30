using Application.Features.TaxManager.Commands;
using Application.Features.TaxManager.Queries;
using Application.Features.VatManger.Commands;
using Application.Features.VatManger.Queries;
using ASPNET.BackEnd.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading;

namespace ASPNET.BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VatController : ControllerBase
    {
        private readonly ISender _sender;

        public VatController(ISender sender)
        {
            _sender = sender;
        }

        [Authorize]
        [HttpPost("CreateVat")]
        public async Task<ActionResult<ApiSuccessResult<CreateVatResult>>> CreateVatAsync(CreateVatRequest request, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(request, cancellationToken);

            return Ok(new ApiSuccessResult<CreateVatResult>
            {
                Code = StatusCodes.Status200OK,
                Message = $"Success executing {nameof(CreateVatAsync)}",
                Content = response
            });
        }

        [Authorize]
        [HttpPost("UpdateVat")]
        public async Task<ActionResult<ApiSuccessResult<UpdateVatResult>>> UpdateVatAsync(UpdateVatRequest request, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(request, cancellationToken);

            return Ok(new ApiSuccessResult<UpdateVatResult>
            {
                Code = StatusCodes.Status200OK,
                Message = $"Success executing {nameof(UpdateVatAsync)}",
                Content = response
            });
        }

        [Authorize]
        [HttpPost("DeleteVat")]
        public async Task<ActionResult<ApiSuccessResult<DeleteVatResult>>> DeleteVatAsync(DeleteVatRequest request, CancellationToken cancellationToken)
        {
            var response = await _sender.Send(request, cancellationToken);

            return Ok(new ApiSuccessResult<DeleteVatResult>
            {
                Code = StatusCodes.Status200OK,
                Message = $"Success executing {nameof(DeleteVatAsync)}",
                Content = response
            });
        }

        [Authorize]
        [HttpGet("GetVatList")]
        public async Task<ActionResult<ApiSuccessResult<GetVatListResult>>> GetVatListAsync(
            CancellationToken cancellationToken,
            [FromQuery] bool isDeleted = false
            )
        {
            var request = new GetVatListRequest { IsDeleted = isDeleted };
            var response = await _sender.Send(request, cancellationToken);

            return Ok(new ApiSuccessResult<GetVatListResult>
            {
                Code = StatusCodes.Status200OK,
                Message = $"Success executing {nameof(GetVatListAsync)}",
                Content = response
            });
        }
    }
}
