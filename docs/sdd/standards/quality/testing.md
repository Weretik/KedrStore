# Testing

Choose tests by risk:

- domain/application rules: unit tests;
- persistence, result mapping and HTTP contracts: integration tests;
- layer/dependency changes: architecture tests.

Run, where applicable:

~~~powershell
dotnet restore KedrStore.sln
dotnet build KedrStore.sln --no-restore
dotnet test KedrStore.sln --no-build
~~~

If a check is blocked or fails, report the exact command, failure point and whether it is pre-existing or caused by the change. Do not hide unrelated failures.
