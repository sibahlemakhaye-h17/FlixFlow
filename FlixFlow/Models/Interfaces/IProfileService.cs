using FlixFlow.ViewModels;

namespace FlixFlow.Interfaces;

public interface IProfileService
{
    Task<ProfileViewModel?> GetProfileAsync(string userId);
    Task<bool> UpdateProfileAsync(string userId, ProfileViewModel model);
}
