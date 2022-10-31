using Azure;
using PollStar.Core.Factories;
using PollStar.Sessions.Abstractions.DataTransferObjects;
using PollStar.Sessions.Abstractions.DomainModels;
using PollStar.Sessions.Abstractions.Repositories;
using PollStar.Sessions.DomainModels;
using PollStar.Sessions.Repositories.Entities;

namespace PollStar.Sessions.Repositories;

public class PollStarSessionsRepository : IPollStarSessionsRepository
{
    private readonly IStorageTableClientFactory _tableStorageClientFactory;

    private const string TableName = "sessions";
    private const string PartitionKey = "session";

    public async Task<ISession> GetAsync(Guid sessionId)
    {
        var tableClient = _tableStorageClientFactory.CreateClient(TableName);
        var entity = await tableClient.GetEntityAsync<SessionTableEntity>(PartitionKey, sessionId.ToString());
        return new Session(
            Guid.Parse(entity.Value.RowKey),
            entity.Value.SessionCode,
            Guid.Parse(entity.Value.UserId),
            entity.Value.Name,
            entity.Value.Description);
    }

    public async Task<List<SessionDto>> ListAsync(Guid userId)
    {
        var sessions = new List<SessionDto>();
        var tableClient = _tableStorageClientFactory.CreateClient(TableName);
        var sessionsQuery = tableClient.QueryAsync<SessionTableEntity>($"{nameof(SessionTableEntity.PartitionKey)} eq '{PartitionKey}' and {nameof(SessionTableEntity.UserId)} eq '{userId}'");
        await foreach (var entitiesPage in sessionsQuery.AsPages())
        {
            sessions.AddRange(entitiesPage.Values.Select(entity =>
                new SessionDto
                {
                    Id = Guid.Parse(entity.RowKey),
                    Code = entity.SessionCode,
                    Name = entity.Name,
                    Description = entity.Description,
                    IsOwner = true
                }
            ));
        }

        return sessions;
    }

    public async Task<ISession> GetByCodeAsync(string code)
    {
        var sessions = new List<ISession>();
        var tableClient = _tableStorageClientFactory.CreateClient(TableName);
        var sessionsQuery = tableClient.QueryAsync<SessionTableEntity>($"{nameof(SessionTableEntity.PartitionKey)} eq '{PartitionKey}' and {nameof(SessionTableEntity.SessionCode)} eq '{code}'");
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
        var tableClient = _tableStorageClientFactory.CreateClient(TableName);
        var response = await tableClient.AddEntityAsync(entity);
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

    public PollStarSessionsRepository(IStorageTableClientFactory tableStorageClientFactory)
    {
        _tableStorageClientFactory = tableStorageClientFactory;
    }
}