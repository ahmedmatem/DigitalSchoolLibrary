# API integration tests

Run from the repository root using the .NET 10 SDK:

```sh
dotnet restore
dotnet build
dotnet test
```

WebApplicationFactory starts the real Program, controllers, model validation,
exception handler, authorization, cookie authentication, Identity password/token
providers, and password-recovery rate limiter. No auth service or authentication
handler is mocked. Each test case has a separate host and uniquely named EF
InMemory database, so users and rate-limit counters are isolated. HTTPS clients
retain cookies and disable redirects to expose authentication status codes.

The test host replaces SQL Server options before startup role seeding. It uses
in-memory data-protection keys, inert storage settings, and a no-op email sender.
There are no SQL Server, email, R2, frontend, or migration calls. InMemory does
not verify SQL translation, relational constraints, or transaction behavior.

Coverage includes anonymous 401s without redirects; automatic validation 400s
with field errors; generic forgot-password success; invalid/valid Identity reset
tokens; rejected and successful password changes; MeDto flags and refreshed
cookies; logout; and the sixth password-recovery request returning 429.

Application validation regression checks verify that GlobalExceptionHandler
preserves the errors dictionary when serializing ValidationProblemDetails.
Invalid reset tokens expose the general error entry; an incorrect current
password exposes the Identity PasswordMismatch entry. Both responses retain
their status, detail, and instance. Automatic MVC field validation remains
covered separately.
