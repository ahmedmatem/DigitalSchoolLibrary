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
