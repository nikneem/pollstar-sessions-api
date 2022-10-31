using PollStar.Sessions.Abstractions.DataTransferObjects;

namespace PollStar.Sessions.Abstractions.Services;

public interface IPollStarSessionsService
{
    Task<SessionDto> GetSessionByCodeAsync(string code, Guid userId);
    Task<SessionDto> GetSessionAsync(Guid id, Guid userId);
    Task<List<SessionDto>> ListSessionsAsync(Guid userId);
    Task<SessionDto> CreateSessionAsync(CreateSessionDto dto);
    Task<SessionDto> UpdateSessionAsync(SessionDto dto);
    Task<bool> DeleteSessionAsync(Guid id);
}