using BLL.Interfaces;
using BLL.ServiceInterfaces;
using DAL.Repositories;
using Domain;
using DTOs.SupportDtos;

namespace BLL.Services;

public class SupportService : ISupportService
{
    private readonly ISupportRepository _repository;

    public SupportService(ISupportRepository repository)
    {
        _repository = repository;
    }

    public async Task SendMessageAsync(DalSupportMessage dto)
    {
        await _repository.AddMessageAsync(dto);
    }

    public async Task<List<SupportMessage>> GetAllMessagesAsync()
    {
        return await _repository.GetAllMessagesAsync();
    }

    public async Task<int> GetUnreadCountAsync()
    {
        return await _repository.GetUnreadCountAsync();
    }

    public async Task<List<SupportMessage>> GetRecentMessagesAsync(int count)
    {
        return await _repository.GetRecentMessagesAsync(count);
    }

    public async Task<List<SupportMessage>> GetOrganizationsWithIssuesAsync()
    {
        return await _repository.GetOrganizationsWithIssuesAsync();
    }

    public async Task<SupportMessage?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task MarkAsReadAsync(int messageId)
    {
        await _repository.MarkAsReadAsync(messageId);
    }

    public async Task<bool> MarkAsResolvedAsync(int messageId)
    {
        return await _repository.MarkAsResolvedAsync(messageId);
    }

    public async Task<bool> ReplyToMessageAsync(DalSupportReply dto)
    {
        return await _repository.ReplyToMessageAsync(dto);
    }
}