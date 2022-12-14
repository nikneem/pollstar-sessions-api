using PollStar.Sessions.Abstractions.DataTransferObjects;
using PollStar.Sessions.Abstractions.DomainModels;

namespace PollStar.Sessions.Abstractions.Repositories;

public interface IPollStarSessionsRepository
{
    Task<ISession> GetByCodeAsync(string code);
    Task<ISession> GetAsync(Guid sessionId);
    Task<List<SessionDto>> ListAsync(Guid  userId);
    Task<bool> CreateAsync(ISession domainModel);
    Task<bool> UpdateAsync(ISession domainModel);
    Task<bool> DeleteAsync(Guid id);
}