# SchoolLibrary

## Backend checks

The solution requires the .NET 10 SDK. Run the same checks as CI from the
repository root:

```sh
dotnet restore SchoolLibrary.slnx
dotnet build SchoolLibrary.slnx --configuration Release --no-restore
dotnet test SchoolLibrary.slnx --configuration Release --no-build --no-restore --logger trx --results-directory artifacts/test-results
```

The Backend tests workflow (`.github/workflows/backend-tests.yml`) runs on pushes
to `dev`/`master` and pull requests targeting `master`. Pushes to `ai` do not
trigger this workflow. A PR from `ai` to `master` still runs the pre-merge check.
The workflow file must exist in a branch for its push checks to run. PR checks
run before merge; push checks run after the pushed commit or merge.
It builds the entire solution and runs
the Application, Infrastructure, and API test projects. New commits cancel older
runs for the same ref. TRX results are retained as the `backend-test-results`
artifact for 14 days, including when tests fail.

Tests use isolated in-memory databases and test email senders; no database,
storage, email credentials, or repository secrets are required. The workflow
does not deploy or run migrations. See each test project's README for coverage
and known limitations.

Infrastructure explicitly references `System.Security.Cryptography.Pkcs` 9.0.23
because EF Core SqlServer / Microsoft.Data.SqlClient otherwise resolves 9.0.4.
This patch update addresses GHSA-2fmv-rv2c-59x7 and GHSA-846f-f764-76mx without
upgrading the SQL driver. Keep this reference until the transitive dependency
resolves a patched version. Check all projects, including transitive packages:

```sh
dotnet list SchoolLibrary.slnx package --vulnerable --include-transitive
```

NuGet vulnerability warnings remain visible in CI.

## Environment configuration

The base appsettings.json has no machine-specific SQL server, credentials, local
frontend URL, or test email recipient. Required settings are checked before
startup role seeding connects to the database. The example
`SchoolLibrary.Api/appsettings.Production.example.json` is a template only: it is
not loaded automatically. Supply production values through your secret store or
environment variables; do not commit a populated settings file.

| Setting / environment variable | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | SQL Server connection string; production should encrypt and validate the server certificate |
| `R2Storage__AccountId` | R2 account identifier |
| `R2Storage__BucketName` | R2 bucket |
| `R2Storage__AccessKeyId` | R2 access key, secret store only |
| `R2Storage__SecretAccessKey` | R2 secret key, secret store only |
| `Email__ApiToken` | Resend token, secret store only |
| `Email__FromAddress` | Verified sending-domain email in non-development environments |
| `Email__FrontendBaseUrl` | Public frontend base URL used in password-reset links |
| `Email__UseTestRecipient` | `false` outside Development; test mode is rejected there |
| `Email__TestRecipient` | Required only when test delivery is enabled |
| `Cors__AllowedOrigins__0` (and subsequent indexes) | Explicit frontend origins, without paths/trailing slashes |
| `AllowedHosts` | Deployment host allowlist, e.g. `library.example` |
| `ASPNETCORE_ENVIRONMENT` | `Development` locally, `Production` in deployment |

Legacy `RESEND_APITOKEN` remains supported and overrides `Email:ApiToken` when
non-empty. Prefer one token source to avoid ambiguity. All non-development
frontend URLs/origins must use HTTPS and cannot point to localhost; reset base
URLs cannot contain user credentials, query strings or fragments. CORS continues
to allow cookies only from configured origins; it does not replace authorization.

For local development, use .NET 10 and configure user secrets for the API project:

```sh
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<your local SQL connection string>" --project SchoolLibrary.Api
dotnet user-secrets set "R2Storage:AccountId" "<account id>" --project SchoolLibrary.Api
dotnet user-secrets set "R2Storage:BucketName" "<bucket name>" --project SchoolLibrary.Api
dotnet user-secrets set "R2Storage:AccessKeyId" "<access key>" --project SchoolLibrary.Api
dotnet user-secrets set "R2Storage:SecretAccessKey" "<secret key>" --project SchoolLibrary.Api
dotnet user-secrets set "Email:ApiToken" "<Resend token>" --project SchoolLibrary.Api
dotnet run --project SchoolLibrary.Api --launch-profile https
```

Replace the example values locally. User secrets are development-only and are
not encrypted; use a managed secret store for deployment. Development defaults
use Windows integrated SQL authentication on localhost (override for named
instances, Linux, or SQL authentication), frontend `http://localhost:4200`, and
Resend's sandbox sender/test recipient. No migrations are applied automatically;
prepare your development database separately using the existing migrations.
Trust the development HTTPS certificate with `dotnet dev-certs https --trust`
where supported. Use the HTTPS launch profile: authentication cookies remain
HttpOnly, Secure=Always and SameSite=None. The HTTP-only profile cannot provide
a working secure-cookie login.

Production ignores launchSettings.json when launched with `--no-launch-profile`.
Use real frontend origins and reset URL, a verified sending domain, configured
SQL/R2/Resend credentials and HTTPS hosting. Match the frontend's API environment
with your existing deployment (same-origin /api routing or a separate HTTPS API).
Changing hosting/proxy infrastructure and persistent data-protection key storage
is outside this configuration change.

An OptionsValidationException identifies the missing setting without printing
its value. Verify the environment, exact key names and secret source; do not
paste connection strings, tokens or cookies into logs or issue reports.
