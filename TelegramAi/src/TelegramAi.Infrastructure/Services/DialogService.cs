using Microsoft.EntityFrameworkCore;
using TelegramAi.Application.DTOs;
using TelegramAi.Application.Interfaces;
using TelegramAi.Application.Requests;
using TelegramAi.Domain.Entities;
using TelegramAi.Domain.Enums;
using TelegramAi.Infrastructure.Extensions;
using TelegramAi.Infrastructure.Persistence;

namespace TelegramAi.Infrastructure.Services;

public class DialogService : IDialogService
{
    private readonly AppDbContext _dbContext;
    private readonly ILanguageModelClient _languageModelClient;

    public DialogService(AppDbContext dbContext, ILanguageModelClient languageModelClient)
    {
        _dbContext = dbContext;
        _languageModelClient = languageModelClient;
    }

    public async Task<DialogDto> StartAsync(Guid userId, CreateDialogRequest request, CancellationToken cancellationToken)
    {
        var channel = await _dbContext.Channels.FirstOrDefaultAsync(
            x => x.Id == request.ChannelId && x.OwnerId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Channel not found");

        var dialog = new Dialog
        {
            ChannelId = channel.Id,
            UserId = userId,
            Title = string.IsNullOrWhiteSpace(request.Title)
                ? $"Диалог от {DateTime.UtcNow:dd.MM HH:mm}"
                : request.Title
        };

        _dbContext.Dialogs.Add(dialog);
        await _dbContext.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            _dbContext.DialogMessages.Add(new DialogMessage
            {
                DialogId = dialog.Id,
                Sender = DialogMessageSender.System,
                Content = request.SystemPrompt
            });

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var loaded = await LoadDialogAsync(dialog.Id, userId, cancellationToken);
        return loaded ?? dialog.ToDto();
    }

    public async Task<AssistantResponseDto> SendMessageAsync(Guid userId, AssistantMessageRequest request, CancellationToken cancellationToken)
    {
        var dialog = await _dbContext.Dialogs
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == request.DialogId && x.UserId == userId, cancellationToken)
            ?? throw new InvalidOperationException("Dialog not found");

        var userMessage = new DialogMessage
        {
            DialogId = dialog.Id,
            Sender = DialogMessageSender.User,
            Content = request.Message
        };

        _dbContext.DialogMessages.Add(userMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // reload messages to include the latest entry
        var messages = await _dbContext.DialogMessages
            .Where(x => x.DialogId == dialog.Id)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var history = messages.Select(m => m.ToDto()).ToList();
        var assistant = await _languageModelClient.GenerateResponseAsync(dialog.Id, history, request.Message, cancellationToken);

        var assistantMessage = new DialogMessage
        {
            DialogId = dialog.Id,
            Sender = DialogMessageSender.Assistant,
            Content = assistant.AssistantMessage,
            MetadataJson = assistant.ProposedCommand
        };

        _dbContext.DialogMessages.Add(assistantMessage);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return assistant;
    }

    public async Task<IReadOnlyCollection<DialogDto>> ListByChannelAsync(Guid userId, Guid channelId, CancellationToken cancellationToken)
    {
        var dialogs = await _dbContext.Dialogs
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.ChannelId == channelId)
            .Include(x => x.Messages)
            .ToListAsync(cancellationToken);

        return dialogs.Select(d => d.ToDto()).ToList();
    }

    private async Task<DialogDto?> LoadDialogAsync(Guid dialogId, Guid userId, CancellationToken cancellationToken)
    {
        var dialog = await _dbContext.Dialogs
            .AsNoTracking()
            .Include(x => x.Messages)
            .FirstOrDefaultAsync(x => x.Id == dialogId && x.UserId == userId, cancellationToken);

        return dialog?.ToDto();
    }
}


