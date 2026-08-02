using Microsoft.AspNetCore.Http;
using SchoolLibrary.Application.Common.Interfaces;
using System.Security.Claims;

namespace SchoolLibrary.Infrastructure.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

        public Guid? UserId
        {
            get
            {
                var userIdValue = User?.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                return Guid.TryParse(userIdValue, out var userId)
                    ? userId
                    : null;
            }
        }

        public bool IsInRole(string role)
        {
            return User?.IsInRole(role) == true;
        }
    }
}
