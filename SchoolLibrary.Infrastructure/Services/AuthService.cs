using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SchoolLibrary.Application.Common.Exceptions;
using SchoolLibrary.Application.DTOs.AuthDtos;
using SchoolLibrary.Application.Interfaces;
using SchoolLibrary.Domain.Constants;
using SchoolLibrary.Infrastructure.Data;
using SchoolLibrary.Infrastructure.Identity;

namespace SchoolLibrary.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext dbContext;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext dbContext)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.dbContext = dbContext;
        }

        public async Task<MeDto> RegisterAsync(
            RegisterDto model,
            CancellationToken cancellationToken = default)
        {
            var normalizedEmail = model.Email.Trim();

            var existingUser = await userManager.FindByEmailAsync(normalizedEmail);

            if (existingUser is not null)
            {
                throw new ValidationException("Вече съществува потребител с този имейл адрес.");
            }

            await ValidateStudentClassAsync(
                model.GradeLevelId,
                model.SchoolClassId,
                cancellationToken);

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),

                FirstName = model.FirstName.Trim(),
                FatherName = model.FatherName.Trim(),
                LastName = model.LastName.Trim(),

                Email = normalizedEmail,
                UserName = normalizedEmail,

                GradeLevelId = model.GradeLevelId,
                SchoolClassId = model.SchoolClassId
            };

            var createResult = await userManager.CreateAsync(user, model.Password);

            if (!createResult.Succeeded)
            {
                throw CreateIdentityValidationException(createResult);
            }

            var roleResult = await userManager.AddToRoleAsync(user, RoleConstants.Student);

            if (!roleResult.Succeeded)
            {
                await userManager.DeleteAsync(user);

                throw CreateIdentityValidationException(roleResult);
            }

            await signInManager.SignInAsync(user, isPersistent: false);

            return await BuildMeDtoAsync(user, cancellationToken);
        }

        public async Task<MeDto?> LoginAsync(
            LoginDto model,
            CancellationToken cancellationToken = default)
        {
            var email = model.Email.Trim();

            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                return null;
            }

            var passwordResult =
                await signInManager.CheckPasswordSignInAsync(
                    user,
                    model.Password,
                    lockoutOnFailure: true);

            if (!passwordResult.Succeeded)
            {
                return null;
            }

            await signInManager.SignInAsync(user, isPersistent: model.RememberMe);

            return await BuildMeDtoAsync(user, cancellationToken);
        }

        public Task LogoutAsync()
        {
            return signInManager.SignOutAsync();
        }

        public async Task<MeDto?> GetMeAsync(
            Guid userId,
            CancellationToken cancellationToken = default)
        {
            var user = await userManager.FindByIdAsync(userId.ToString());

            if (user is null)
            {
                return null;
            }

            return await BuildMeDtoAsync(user, cancellationToken);
        }

        private async Task<MeDto> BuildMeDtoAsync(
            ApplicationUser user,
            CancellationToken cancellationToken)
        {
            var roles = await userManager.GetRolesAsync(user);

            var schoolData = await dbContext.Users
                .AsNoTracking()
                .Where(item => item.Id == user.Id)
                .Select(item => new
                {
                    GradeNumber = item.GradeLevelId == null
                        ? (int?)null
                        : dbContext.GradeLevels
                            .Where(grade =>
                                grade.Id == item.GradeLevelId)
                            .Select(grade => (int?)grade.Number)
                            .FirstOrDefault(),

                    SchoolClassName = item.SchoolClassId == null
                        ? null
                        : dbContext.SchoolClasses
                            .Where(schoolClass =>
                                schoolClass.Id == item.SchoolClassId)
                            .Select(schoolClass =>
                                schoolClass.GradeLevel.Number +
                                schoolClass.Section)
                            .FirstOrDefault()
                })
                .FirstAsync(cancellationToken);

            return new MeDto
            {
                Id = user.Id,

                FirstName = user.FirstName,
                FatherName = user.FatherName,
                LastName = user.LastName,

                FullName =
                    $"{user.FirstName} {user.FatherName} {user.LastName}",

                Email = user.Email ?? string.Empty,

                Roles = roles.ToArray(),

                GradeLevelId = user.GradeLevelId,
                GradeNumber = schoolData.GradeNumber,

                SchoolClassId = user.SchoolClassId,
                SchoolClassName = schoolData.SchoolClassName
            };
        }

        private async Task ValidateStudentClassAsync(
            int? gradeLevelId,
            Guid? schoolClassId,
            CancellationToken cancellationToken)
        {
            if (!gradeLevelId.HasValue && !schoolClassId.HasValue)
            {
                return;
            }

            if (!gradeLevelId.HasValue || !schoolClassId.HasValue)
            {
                throw new ValidationException(
                    "Трябва да бъдат избрани както клас, така и паралелка.");
            }

            var classExists = await dbContext.SchoolClasses
                .AnyAsync(
                    schoolClass =>
                        schoolClass.Id == schoolClassId.Value &&
                        schoolClass.GradeLevelId == gradeLevelId.Value,
                    cancellationToken);

            if (!classExists)
            {
                throw new ValidationException(
                    "Избраната паралелка не принадлежи към избрания клас.");
            }
        }

        private static ValidationException CreateIdentityValidationException(IdentityResult result)
        {
            var errors = result.Errors
                .GroupBy(error => error.Code)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.Description)
                        .ToArray());

            return new ValidationException("Регистрацията не беше успешна.", errors);
        }
    }
}
