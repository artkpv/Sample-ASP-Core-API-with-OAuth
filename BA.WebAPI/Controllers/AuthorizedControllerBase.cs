using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BA.WebAPI.Controllers
{
    public class AuthorizedControllerBase : ControllerBase
    {
        private string _userId = null;

        private string _role = null;

        private void EnsureClaims()
        {
            if (_userId == null)
                _userId = User.Claims.FirstOrDefault(c => c.Type == "name")?.Value;
            if (_role == null)
                _role = User.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        }

        protected string GetUserId()
        {
            EnsureClaims();
            return _userId;
        }
        protected bool IsEntryMine(string userId)
        {
            EnsureClaims();
            return _userId == userId;
        }

        protected bool IsAdminRole()
        {
            EnsureClaims();
            return _role == Startup.Roles.Administrator;
        }
    }
}
