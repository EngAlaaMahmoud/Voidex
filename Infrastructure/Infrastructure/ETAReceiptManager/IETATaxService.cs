using Application.Common.DTOs.ETAReceiptDetails;
using Infrastructure.ETAReceiptManager;
using System.Threading.Tasks;

namespace Application.Common.Services.ETAReceiptManager
{
    public interface IETATaxService
    {
        /// <summary>
        /// Get customer tax info by national/tax id
        /// </summary>
        Task<CustomerTaxInfoDto?> GetCustomerTaxInfoAsync(GetCustomerRequestDto request, string accessToken);
    }
}