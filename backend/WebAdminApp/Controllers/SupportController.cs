using DAL.Repositories;
using DTOs.SupportDtos;
using Microsoft.AspNetCore.Mvc;

namespace WebAdminApp.Controllers;

public class SupportController : Controller
{
    private readonly SupportRepository _supportRepository;

    public SupportController(SupportRepository supportRepository)
    {
        _supportRepository = supportRepository;
    }

    public async Task<IActionResult> Index()
    {
        var messages = await _supportRepository.GetAllMessagesAsync();
        return View(messages);
    }

    public async Task<IActionResult> View(int id)
    {
        var message = await _supportRepository.GetByIdAsync(id);
        if (message == null) return NotFound();

        await _supportRepository.MarkAsReadAsync(id);
        return View(message);
    }

    [HttpPost]
    public async Task<IActionResult> Reply(DalSupportReply dto)
    {
        var success = await _supportRepository.ReplyToMessageAsync(dto);
        if (!success) return NotFound();
        
        return RedirectToAction("Index");
    }
}