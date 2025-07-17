using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DTOs.SupportDtos;
using BLL.Interfaces;
using BLL.ServiceInterfaces;
using Domain;

namespace ShiftEaseAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportController : ControllerBase
    {
        private readonly ISupportService _supportService;

        public SupportController(ISupportService supportService)
        {
            _supportService = supportService;
        }

        // POST: api/support/send-message
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] DalSupportMessage message)
        {
            if (message == null || string.IsNullOrEmpty(message.SenderEmail) || string.IsNullOrEmpty(message.Message))
            {
                return BadRequest("Message details are missing.");
            }

            try
            {
                await _supportService.SendMessageAsync(message);
                return Ok(new { message = "Message successfully sent!" });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while sending the message.", error = ex.Message });
            }
        }

        // GET: api/support/messages
        [HttpGet("messages")]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _supportService.GetAllMessagesAsync();
            return Ok(messages);
        }

        // GET: api/support/unread-count
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _supportService.GetUnreadCountAsync();
            return Ok(new { unreadCount = count });
        }

        // GET: api/support/recent?count=5
        [HttpGet("recent")]
        public async Task<IActionResult> GetRecentMessages([FromQuery] int count = 5)
        {
            var recentMessages = await _supportService.GetRecentMessagesAsync(count);
            return Ok(recentMessages);
        }

        // GET: api/support/unresolved
        [HttpGet("unresolved")]
        public async Task<IActionResult> GetUnresolvedMessages()
        {
            var unresolved = await _supportService.GetOrganizationsWithIssuesAsync();
            return Ok(unresolved);
        }

        // GET: api/support/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessageById(int id)
        {
            var message = await _supportService.GetByIdAsync(id);
            if (message == null)
                return NotFound();
            return Ok(message);
        }

        // POST: api/support/mark-as-read/5
        [HttpPost("mark-as-read/{id}")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _supportService.MarkAsReadAsync(id);
            return Ok(new { message = "Message marked as read." });
        }

        // POST: api/support/mark-as-resolved/5
        [HttpPost("mark-as-resolved/{id}")]
        public async Task<IActionResult> MarkAsResolved(int id)
        {
            var result = await _supportService.MarkAsResolvedAsync(id);
            if (!result)
                return NotFound(new { message = "Message not found." });
            return Ok(new { message = "Message marked as resolved." });
        }

        // POST: api/support/reply
        [HttpPost("reply")]
        public async Task<IActionResult> ReplyToMessage([FromBody] DalSupportReply reply)
        {
            var result = await _supportService.ReplyToMessageAsync(reply);
            if (!result)
                return NotFound(new { message = "Message not found." });

            return Ok(new { message = "Reply sent and message marked as resolved." });
        }
    }
}