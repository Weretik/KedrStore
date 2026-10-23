# 1C counterparties and Identity account provisioning

## Purpose and ownership

Sales import maps a 1C counterparty to two related local records:

```text
1C GetCounterparties
       ↓ SalesOneCReadClient
OneCCounterpartiesSyncService
       ├── Sales.Counterparties (business customer)
       └── IImportedCounterpartyIdentityProvisioningService
                ↓
          Identity.AspNetUsers (login account, User role)
```

`Counterparty.Id` in Sales is the trimmed 1C `CounterpartyId` string. `Counterparty.IdentityUserId` links it to `AppUser.Id` (GUID). Sales owns the customer record; Identity owns the login user, password hashing, roles, and account settings.

## Source methods and fields

`SalesOneCReadClient.GetCounterpartiesAsync()` calls generated SOAP operation `GetCounterpartiesAsync()` and maps:

| 1C field | Local use |
| --- | --- |
| `CounterpartyId` | Sales counterparty ID; input value for the imported account's initial password |
| `CounterpartyName` | Counterparty name and `AppUser.FullName` |
| `Email` | Sales email, username, and Identity email |
| `Phone` | Normalized Sales phone; invalid optional phone is ignored |
| `DefaultPriceTypeId` | Customer default price |

`SalesOneCReadClient.GetCounterpartyCategoryPriceTypesAsync()` calls `GetCounterpartyCategoryPriceTypesAsync()` and returns `(CounterpartyId, CategoryId, PriceTypeId)` rules. In `sales-customers-full`, rule sync runs after counterparties so their references can be validated.

## Counterparty reconciliation

`OneCCounterpartiesSyncService`:

1. Stops without changes if 1C returns no counterparties.
2. Deduplicates non-empty IDs, keeping the last row for each.
3. Rejects rows with empty ID/name, non-positive default price type, or invalid/missing email; logs the skip reason.
4. Normalizes email and optional phone through `CounterpartyContactNormalizer`.
5. Creates or finds an Identity user, then creates, updates, or restores the Sales counterparty.
6. Treats local counterparties missing from a valid non-empty source response as stale: removes their category price rules and Sales records, then requests deletion of the Identity user.

It also prevents linking one `IdentityUserId` to two different counterparties in one local state.

## Identity user creation and update

Implementation: `ImportedCounterpartyIdentityProvisioningService`, registered in Identity Infrastructure for Application interface `IImportedCounterpartyIdentityProvisioningService`.

```text
existing Counterparty.IdentityUserId → find AppUser by ID
                                      ↓ not found
                                  find AppUser by email
                                      ↓ not found
                              create AppUser + User role
```

For a new user, the service sets username/email/full name, `EmailConfirmed`, and `LockoutEnabled` from `Identity:ImportedCounterparties`, then creates an account using the 1C `CounterpartyId` as the initial password input. For an existing user, it updates those fields, resets the password to the current `CounterpartyId`, and ensures the `User` role. ASP.NET Identity hashes passwords; the application must not log or expose the input value.

## Security and operational effects

- This is more than a read import: `counterparties` or `sales-customers-full` can create, update, restore, or delete both customer data and login accounts.
- An email match can link an existing Identity user to an imported counterparty. Treat imported 1C email as authoritative only after data quality is confirmed.
- Sales changes are saved before stale Identity users are deleted. If Identity deletion fails, the Sales deletion remains and a warning is logged; investigate the orphaned account manually.
- The flow spans Sales and Identity contexts and is not one cross-context transaction. Do not add side effects that assume atomic rollback without separate design.
- Category price-type rules are skipped when the counterparty is absent from Sales or the category/price type is missing from Catalog. If lookup data changed, sync Catalog before `sales-customers-full`.

## Verification after a safe test run

1. Use a non-production test counterparty with valid email and default price type.
2. Run `--job=counterparties`; check Sales fields and exactly one linked `IdentityUserId`.
3. Check that the Identity user has the `User` role and imported account settings. Do not inspect or print password data.
4. Run `--job=counterparty-category-price-types`; check that only rules with valid local Catalog references remain.
5. Check imported, updated, restored, deleted, and skipped counts. Investigate skipped or deleted rows before rerunning.
