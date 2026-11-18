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

Known existing defect discovered during this step: GlobalExceptionHandler stores
ValidationProblemDetails in a ProblemDetails variable and passes that static
type to WriteAsJsonAsync. Its application validation responses contain status,
title, detail, and instance but omit the derived errors property. Automatic MVC
validation responses do include errors. Tests check current application error
details without asserting that errors must be absent. Restoring structured
application errors requires a separate production fix and regression assertions;
no production code is changed in this step.
