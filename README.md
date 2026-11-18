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
to `ai` and pull requests targeting `ai`. It builds the entire solution and runs
the Application, Infrastructure, and API test projects. New commits cancel older
runs for the same ref. TRX results are retained as the `backend-test-results`
artifact for 14 days, including when tests fail.

Tests use isolated in-memory databases and test email senders; no database,
storage, email credentials, or repository secrets are required. The workflow
does not deploy or run migrations. See each test project's README for coverage
and known limitations. Dependency vulnerability warnings remain visible in CI.
