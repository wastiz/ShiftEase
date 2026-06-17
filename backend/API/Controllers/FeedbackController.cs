using BLL.Contracts;
using DTOs.FeedbackDtos;
using DTOs.SupportDtos;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/feedback")]
[ApiController]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ISupportService _supportService;

    public FeedbackController(IFeedbackService feedbackService, ISupportService supportService)
    {
        _feedbackService = feedbackService;
        _supportService = supportService;
    }

    // POST: api/feedback/submit
    [HttpPost("submit")]
    public async Task<IActionResult> Submit([FromBody] BllFeedbackResponse dto)
    {
        if (dto == null)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Feedback data is missing."));

        if (dto.Rating < 1 || dto.Rating > 10)
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Rating must be between 1 and 10."));

        if (string.IsNullOrWhiteSpace(dto.SchedulingMethodBefore) ||
            string.IsNullOrWhiteSpace(dto.TodaysPurpose) ||
            string.IsNullOrWhiteSpace(dto.BiggestChallenge) ||
            string.IsNullOrWhiteSpace(dto.RatingReason))
        {
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Required fields are missing."));
        }

        await _feedbackService.SubmitFeedbackAsync(dto);
        return Ok(new { message = "Feedback submitted successfully. Thank you!" });
    }

    // POST: api/feedback/support/send-message
    [HttpPost("support/send-message")]
    public async Task<IActionResult> SendMessage([FromBody] BllSupportMessage message)
    {
        if (message == null || string.IsNullOrEmpty(message.SenderEmail) || string.IsNullOrEmpty(message.Message))
            return BadRequest(new ErrorResponse("VALIDATION_ERROR", "Message details are missing."));

        await _supportService.SendMessageAsync(message);
        return Ok(new { message = "Message successfully sent!" });
    }
}
