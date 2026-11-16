# Application unit tests

Run from the repository root with the .NET 10 SDK:

```sh
dotnet restore
dotnet build
dotnet test
```

The authentication requests use DataAnnotations on DTOs, rather than command
handlers or separate validators. These tests validate the actual DTOs through
`Validator.TryValidateObject` with all properties enabled. Invalid cases assert
the affected member, and valid cases assert that no validation errors remain.
Password cases cover the DTO's required/minimum-length/confirmation rules;
Identity password policy is enforced separately in Infrastructure.

No database, email service, FluentAssertions, or mocking library is required.

`MustChangePassword` transitions live in Infrastructure's `AuthService` and depend
on `UserManager`/`SignInManager`. `GetMeAsync` also queries `ApplicationDbContext`.
`CurrentUserService` exposes identity/roles, not this flag. Testing those behaviors
requires Infrastructure-focused coverage and is outside this Application-only
unit project; testing assignments on `MeDto` alone would not verify that behavior.
