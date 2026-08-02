namespace SchoolLibrary.Application.Common.Interfaces
{
    public interface ICurrentUserService
    {
        bool IsAuthenticated { get; }

        Guid? UserId { get; }

        bool IsInRole(string role);
    }
}
