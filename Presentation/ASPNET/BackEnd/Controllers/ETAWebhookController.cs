using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ASPNET.BackEnd.Controllers;

[ApiController]
[Route("api/eta-webhooks")]
public class ETAWebhookController : ControllerBase
{
    private readonly ILogger<ETAWebhookController> _logger;

    public ETAWebhookController(ILogger<ETAWebhookController> logger)
    {
        _logger = logger;
    }

    [HttpPost("receipt-notifications")]
    public async Task<IActionResult> ReceiveReceiptNotifications([FromBody] object notification)
    {
        try
        {
            _logger.LogInformation("Received receipt notification: {Notification}", notification);
            
            // Process receipt notification (validation, reception, cancellation)
            // Add your business logic here
            
            return Ok(new { status = "received", message = "Receipt notification processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process receipt notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("general-notifications")]
    public async Task<IActionResult> ReceiveGeneralNotifications([FromBody] object notification)
    {
        try
        {
            _logger.LogInformation("Received general notification: {Notification}", notification);
            
            // Process general notifications (credentials expiration, device status, etc.)
            // Add your business logic here
            
            return Ok(new { status = "received", message = "General notification processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process general notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }

    [HttpPost("package-ready-notifications")]
    public async Task<IActionResult> ReceivePackageReadyNotifications([FromBody] object notification)
    {
        try
        {
            _logger.LogInformation("Received package ready notification: {Notification}", notification);
            
            // Process package ready notifications
            // Add your business logic here
            
            return Ok(new { status = "received", message = "Package ready notification processed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process package ready notification");
            return StatusCode(500, new { error = "Failed to process notification" });
        }
    }
} 