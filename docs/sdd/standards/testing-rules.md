# Testing rules

Choose tests by risk:

- domain/application rules — unit tests;
- persistence, Result mapping, and HTTP contracts — integration tests;
- layer or dependency changes — architecture tests.

Before completing a feature, run where applicable:

```powershell
dotnet restore KedrStore.sln
dotnet build KedrStore.sln --no-restore
dotnet test KedrStore.sln --no-build
```

If verification is blocked or fails, state the exact command, failure point, and whether the failure is pre-existing or caused by the change. Do not hide unrelated failures.
