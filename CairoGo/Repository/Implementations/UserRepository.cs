using CairoGo.Models.Entity;
using CairoGo.Repository.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CairoGo.Repository.Implementations
{
    public class UserRepository:IUserRepository
    {
        private readonly UserManager<UserApplication> _userManager;

        public UserRepository(UserManager<UserApplication> userManager)
        {
            _userManager = userManager;
        }

        public Task<bool> CheckPasswordAsync(UserApplication user, string password)=>
            _userManager.CheckPasswordAsync(user, password);
        

        public Task<IdentityResult> CreateAsync(UserApplication user, string password)=>
            _userManager.CreateAsync(user,password);
      

        public Task<UserApplication> GetByEmailAsync(string email)=>
            _userManager.FindByEmailAsync(email);
        public Task<UserApplication> GetByIdAsync(Guid id) =>
       _userManager.FindByIdAsync(id.ToString());


        public async Task<UserApplication?> GetByRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
                return null;

            return await _userManager.Users
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);
        }

        public Task<IdentityResult> UpdateAsync(UserApplication user) =>
            _userManager.UpdateAsync(user);

    }
}
