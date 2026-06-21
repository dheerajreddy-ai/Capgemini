using EduVoice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EduVoice.API.Controllers;

[ApiController]
[Route("api/payment")]
[Authorize]
[EnableRateLimiting("api")]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("send-link/{studentId:guid}")]
    public async Task<IActionResult> SendLink(Guid studentId)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _paymentService.SendPaymentLinkAsync(schoolId.Value, studentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("confirm/{studentId:guid}")]
    public async Task<IActionResult> Confirm(Guid studentId, [FromBody] ConfirmPaymentRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _paymentService.ConfirmPaymentAsync(schoolId.Value, studentId, request.AmountPaid);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("dispute/{studentId:guid}")]
    public async Task<IActionResult> RaiseDispute(Guid studentId, [FromBody] DisputeRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _paymentService.RaiseDisputeAsync(schoolId.Value, studentId, request.Note);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("resolve-dispute/{studentId:guid}")]
    public async Task<IActionResult> ResolveDispute(Guid studentId)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _paymentService.ResolveDisputeAsync(schoolId.Value, studentId);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPost("grant-extension/{studentId:guid}")]
    public async Task<IActionResult> GrantExtension(Guid studentId, [FromBody] ExtensionRequest request)
    {
        var schoolId = GetSchoolId();
        if (schoolId == null) return Unauthorized();
        var result = await _paymentService.GrantExtensionAsync(schoolId.Value, studentId, request.ExtraDays);
        return result.Success ? Ok(result) : BadRequest(result);
    }

    private Guid? GetSchoolId()
    {
        var claim = HttpContext.Items["SchoolId"]?.ToString() ?? User.FindFirst("schoolId")?.Value;
        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record ConfirmPaymentRequest(decimal AmountPaid);
public record DisputeRequest(string Note);
public record ExtensionRequest(int ExtraDays = 14);
