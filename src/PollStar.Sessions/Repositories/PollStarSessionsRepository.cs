using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Options;
using PollStar.Core;
using PollStar.Core.Configuration;
using PollStar.Sessions.Abstractions.DomainModels;
using PollStar.Sessions.Abstractions.Repositories;
using PollStar.Sessions.DomainModels;
using PollStar.Sessions.Repositories.Entities;

namespace PollStar.Sessions.Repositories;

public class PollStarSessionsRepository : IPollStarSessionsRepository
{

    private TableClient _tableClient;
    private const string TableName = "sessions";
    private const string PartitionKey = "session";

    public async Task<ISession> GetAsync(Guid sessionId)
    {
        var entity = await _tableClient.GetEntityAsync<SessionTableEntity>(PartitionKey, sessionId.ToString());
        return new Session(
            Guid.Parse(entity.Value.RowKey),
            entity.Value.SessionCode,
            Guid.Parse(entity.Value.UserId),
            entity.Value.Name,
            entity.Value.Description);
    }

    public async Task<ISession> GetByCodeAsync(string code)
    {
        var sessions = new List<ISession>();
        var sessionsQuery = _tableClient.QueryAsync<SessionTableEntity>($"{nameof(SessionTableEntity.PartitionKey)} eq '{PartitionKey}' and {nameof(SessionTableEntity.SessionCode)} eq '{code}'");
        await foreach (var entitiesPage in sessionsQuery.AsPages())
        {
            sessions.AddRange(entitiesPage.Values.Select(entity =>
            new Session(
                Guid.Parse(entity.RowKey),
                entity.SessionCode,
                Guid.Parse(entity.UserId),
                entity.Name,
                entity.Description)
            ));
        }
        return sessions.First();
    }

    public async Task<bool> CreateAsync(ISession domainModel)
    {
        var entity = new SessionTableEntity
        {
            RowKey = domainModel.Id.ToString(),
            SessionCode = domainModel.SessionCode,
            UserId = domainModel.UserId.ToString(),
            Name = domainModel.Name,
            Description = domainModel.Description,
            PartitionKey = PartitionKey,
            Timestamp = DateTimeOffset.UtcNow,
            ETag = ETag.All
        };
        var response = await _tableClient.AddEntityAsync(entity);
        return !response.IsError;
    }

    public Task<bool> UpdateAsync(ISession domainModel)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public PollStarSessionsRepository(IOptions<AzureConfiguration> options)
    {
        var accountName = options.Value.StorageAccount;
        var accountKey = options.Value.StorageKey;
        var storageUri = new Uri($"https://{accountName}.table.core.windows.net");
        _tableClient = new TableClient(
            storageUri,
            TableName,
        new TableSharedKeyCredential(accountName, accountKey));
    }
}