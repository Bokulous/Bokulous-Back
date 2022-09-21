using Bokulous_Back.Services;

namespace Bokulous_Back.Helpers
{
    public class UserHelpers
    {
        private static BokulousDbService _bokulousDbService;
        public UserHelpers(BokulousDbService bokulousDbService)
        {
            _bokulousDbService = bokulousDbService;
        }

        public async Task<bool> CheckIsAdmin(string adminId, string adminPassword)
        {
            var admin = await _bokulousDbService.GetUserAsync(adminId);

            return admin is not null && admin.Password == adminPassword && admin.IsAdmin;
        }

        public bool CheckIsPasswordValid(string password)
        {
            const int PASS_MIN_LENGTH = 6;
                
            if (password is null)
                return false;

            if (password.Length < PASS_MIN_LENGTH)
                return false;

            return true;
        }

        public bool CheckIsUsernameValid(string username)
        {
            const int USERNAME_MIN_LENGTH = 3;

            if (username is null)
                return false;

            if (username.Length < USERNAME_MIN_LENGTH)
                return false;

            return true;
        }
    }
}
