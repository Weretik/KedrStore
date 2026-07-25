# OneC counterparties and Identity provisioning

## Purpose and ownership

The Sales import turns a 1C counterparty into two linked local records:

```text
1C GetCounterparties
       ↓ SalesOneCReadClient
OneCCounterpartiesSyncService
       ├── Sales.Counterparties (business customer)
       └── IImportedCounterpartyIdentityProvisioningService
                ↓
          Identity.AspNetUsers (login account, role User)
```

The Sales `Counterparty.Id` is the trimmed string `CounterpartyId` from 1C. `Counterparty.IdentityUserId` is the link to `AppUser.Id` (GUID). Sales owns the customer record; Identity owns the login user, password hashing, roles and account options.

## Source methods and fields

`SalesOneCReadClient.GetCounterpartiesAsync()` calls the generated SOAP operation `GetCounterpartiesAsync()` and maps:

| 1C field | Local use |
| --- | --- |
| `CounterpartyId` | Sales counterparty ID; initial password input for an imported account |
| `CounterpartyName` | counterparty name and `AppUser.FullName` |
| `Email` | Sales email, Identity username and email |
| `Phone` | normalized Sales phone; invalid optional phone is ignored |
| `DefaultPriceTypeId` | default customer price type |

`SalesOneCReadClient.GetCounterpartyCategoryPriceTypesAsync()` calls `GetCounterpartyCategoryPriceTypesAsync()` and returns `(CounterpartyId, CategoryId, PriceTypeId)` rules. The rule sync runs after counterparties in `sales-customers-full` so it can validate the counterparty reference.

## Counterparty reconciliation

`OneCCounterpartiesSyncService`:

1. Stops with no changes if 1C returns no counterparties.
2. De-duplicates non-empty IDs, keeping the last row for each ID.
3. Rejects a row with empty ID/name, non-positive default price type or invalid/missing email. It logs a skip reason.
4. Normalizes email and optional phone through `CounterpartyContactNormalizer`.
5. Provisions an Identity user, then creates, updates or restores the Sales counterparty.
6. Treats locally present counterparties absent from a non-empty valid source result as stale: it deletes their category price rules and Sales counterparties, then requests Identity-user deletion.

It also refuses to attach one `IdentityUserId` to two different counterparties in the same local state.

## Identity provisioning behaviour

The implementation is `ImportedCounterpartyIdentityProvisioningService`, registered from Identity Infrastructure against the Application interface `IImportedCounterpartyIdentityProvisioningService`.

```text
existing Counterparty.IdentityUserId → find AppUser by ID
                                      ↓ not found
                                  find AppUser by email
                                      ↓ not found
                              create AppUser + User role
```

For a created user it sets username/email/full name, `EmailConfirmed` and `LockoutEnabled` from `Identity:ImportedCounterparties`, then creates the account with the 1C `CounterpartyId` as its initial password input. For an existing matched user it updates the same fields, resets the password to the current `CounterpartyId`, and ensures the `User` role exists. Passwords are handled by ASP.NET Identity hashing; the application must never log or expose the input value.

## Safety and operational consequences

- This is not merely a read import: a `counterparties` or `sales-customers-full` run can create, update, restore or delete both customer data and login accounts.
- An email match can associate an existing Identity user with an imported counterparty. Treat imported email as authoritative only after validating 1C data quality.
- Sales changes are saved before stale Identity users are deleted. If Identity deletion fails, Sales deletion remains and a warning is logged; investigate the orphaned account manually.
- The current flow spans Sales and Identity contexts, so it is not one cross-database transaction. Do not add side effects that assume atomic rollback without an explicit design.
- Counterparty category price rules are skipped unless their counterparty exists in Sales and their category and price type exist in Catalog. Run catalog reference-data synchronization before `sales-customers-full` when references have changed.

## Verification after a safe test run

1. Use a non-production test counterparty with a valid email and default price type.
2. Run `--job=counterparties`; verify Sales counterparty fields and exactly one linked `IdentityUserId`.
3. Verify the Identity user has the `User` role and imported-account options applied. Do not inspect or print password data.
4. Run `--job=counterparty-category-price-types`; verify only rules with valid local Catalog references are present.
5. Review imported, updated, restored, deleted and skipped counts. Investigate skipped or deleted records before rerunning.
