# Database rules

**Applies to:** `KedrStore.sln`: every module `DbContext` (Catalog, Sales, Identity, Content, Media, Work, Scheduling), `Host.Api`, `Host.Jobs`, the deploy workflow
**Checked by:** architecture tests in `tests/ArchitectureTests`, integration tests on PostgreSQL (`tests/IntegrationTests`, Testcontainers), migration review, and the database step of `$backend-code-audit`

How code reads and writes the database: queries, indexes and search, transactions, concurrency, and how migrations reach production. KedrStore and BreviERP use this text verbatim, followed by their own specifics. The rules assume EF Core with PostgreSQL (Npgsql) and use only PostgreSQL built-ins and the .NET platform: no third-party search engines or locking libraries.

Each rule says **what** to do, **where** in the code it applies, and **how to check it**. An audit reports a violation as a finding with the file and line; a new feature follows the rule from its first commit.

## 1. Queries

### 1.1 Reads do not track

- **What.** A query whose result is only returned or mapped uses `AsNoTracking()` or a `Select(...)` projection to a DTO. Tracking is only for entities the same use case changes and saves.
- **Where.** Query handlers (`Features/<Feature>/Queries/`), readers in Infrastructure, `IRead<Module>DbContext` users, read repositories. A "read repository" type must not track: give it `AsNoTracking` or read through the read context.
- **Check.** Every `ToListAsync`/`FirstOrDefaultAsync`/`ToDictionaryAsync` in a query path is preceded by `AsNoTracking()` or ends in a `Select` to a non-entity type.

### 1.2 Project, do not load whole entities

- **What.** A list or page selects only the columns it returns (`Select(x => new RowDto(...))`). Loading entities and mapping in memory is for commands that change them.
- **Where.** Every list, search, export, and projection rebuild.
- **Check.** No query handler maps an entity list with `.Select` after `ToListAsync`.

### 1.3 No N+1

- **What.** Never run a query or `SaveChanges` inside a loop over rows. Load what the loop needs in one query before it (`Where(x => ids.Contains(x.Id))` → `ToDictionaryAsync`), change objects in memory, and save once after it. For related rows of a list use a projection with the child fields, or one `Include` of a single collection; two or more collection `Include`s in one query use `AsSplitQuery()` to avoid a cartesian explosion.
- **Where.** Import and sync jobs (1C, CSV, CDN), batch commands, scheduler ticks, mappers that call a repository.
- **Check.** A `foreach`/`for` body that contains `await ...Async(` on a `DbContext`, repository, or `SaveChanges`. A loop over at most ten fixed items (for example scheduler task definitions) is allowed and needs no fix.

### 1.4 Filter, sort, and page in SQL

- **What.** `Where`, `OrderBy`, `Skip`/`Take`, `Count` run in the database. Do not load a whole table and filter, search, or page in C#. Every list endpoint has paging; an unbounded endpoint (export, offline sync) is documented as such and uses a projection.
- **Where.** Admin lists, public catalog pages, "state" readers that build a model of the whole catalog.
- **Check.** `ToListAsync()` / `ToDictionaryAsync()` with no `Where` on a table that grows with the business (products, orders, customers, files), followed by LINQ-to-objects filtering.
- **When a whole-table model is really needed** (for example a public catalog tree), cache it with the platform `HybridCache` (or `IMemoryCache`) for a short time and invalidate it from the commands that change it. Never rebuild it on every HTTP request.

### 1.5 Other EF traps

- `Contains` over an id list is fine (Npgsql sends one array parameter); do not pass a list of all ids when the filter keeps every row.
- Change-tracker cost grows with tracked entities: a long import that tracks tens of thousands of rows saves in batches with `ChangeTracker.Clear()` between batches, or uses `ExecuteUpdate`/`ExecuteDelete` for set-based changes.
- Use `ExecuteUpdateAsync`/`ExecuteDeleteAsync` for a bulk change that needs no domain rule. When a domain rule must run, load and change the aggregates.
- `EnableRetryOnFailure` (transient connection retries) is optional; when enabled, every explicit transaction runs inside `Database.CreateExecutionStrategy().ExecuteAsync(...)`.

## 2. Indexes and search

### 2.1 Search: every word, part of a word, on an index

- **What.** A text search box behaves the same everywhere:
  1. Trim the input and split it on whitespace into words.
  2. For each word add one condition `EF.Functions.ILike(column, "%" + Escape(word) + "%", "\\")`; the conditions are joined with AND (each word must match), and one word may match any of the searched columns (OR across columns inside the word).
  3. Escape `%`, `_` and `\` in the word with the shared `SearchWords` helper (`SearchWords.Patterns` splits and escapes, `SearchWords.Escape` escapes one value).
  4. Every searched column has a trigram index: `HasIndex(x => x.Column).HasMethod("gin").HasOperators("gin_trgm_ops")`, with `modelBuilder.HasPostgresExtension("pg_trgm")` in the owning `DbContext`.
  5. Normalize text the same way when you store it and when you search it if the language needs it (apostrophes `'` and `’`, `ё`/`е`). Keep the normalization in one domain function.
- **Never.** `ToLower().Contains(...)`, `ToUpper().Contains(...)`, or `string.Contains` in SQL: the database cannot use an index for them and reads every row. Never search in C# after loading the table.
- **Short words.** A word of one or two characters yields no trigram and makes the index scan everything. Apply it only together with a longer word, or match it as a prefix or exact value (for example an id).
- **Several columns.** When a search spans several text columns of one row, either index each column or store one combined search column (`SearchText = id + " " + name + ...`) maintained by the domain or projection and index that one column.
- **Ranking** ("most similar first") is a feature, not a default: use `pg_trgm` `word_similarity` or full-text search (`tsvector`) only when a specification asks for it.
- **Where.** Every handler or reader with a `Search`, `SearchTerm`, `Query`, or `q` parameter.
- **Check.** Search code follows steps 1 to 4; the searched columns have a `gin_trgm_ops` index in the model snapshot.

### 2.2 Which indexes to add

- **What.** Add an index for each query that filters, joins, or sorts a table that grows: a `WHERE` or `JOIN` column, the `ORDER BY` of a paged list, and every foreign key (EF adds FK indexes itself; hand-written SQL must add them).
- **Column order in a composite index:** first the columns compared with `=`, then one column compared with a range (`>=`, `<`, `BETWEEN`) or used for `ORDER BY`, then a tie-breaker (`Id`) so paging is stable. Example: list of a customer's orders by date → `(CounterpartyId, CreatedAt DESC, Id)`.
- **Partial index** (`HasFilter`) when queries always filter on the same condition (`WHERE "IsDeleted" = false`, `WHERE "ExportToSite"`), and for "at most one active row" uniqueness.
- **Unique index** for every business uniqueness rule (slug, code, idempotency key, number). It is the real guard against duplicates under concurrency (section 4); a code check before insert is only a friendly message.
- **Where.** `Persistence/<Aggregate>/<Entity>Configuration.cs`; the SDD data model lists the indexes of new tables.

### 2.3 Which indexes to remove

- **What.** An index costs on every insert and update. Remove an index that a composite index already covers by its leftmost columns (`(A)` when `(A, B, C)` exists), unless it is the FK index EF requires and you replace it. Remove an index that production never uses.
- **How to find unused ones:** on the production database (read-only, with the owner's permission) run

  ```sql
  SELECT schemaname, relname, indexrelname, idx_scan,
         pg_size_pretty(pg_relation_size(indexrelid)) AS size
  FROM pg_stat_user_indexes
  ORDER BY idx_scan, pg_relation_size(indexrelid) DESC;
  ```

  `idx_scan = 0` over weeks of real traffic marks a candidate. Never remove a primary key or a unique index: they guard data even without reads.

### 2.4 Bulk writes and indexes

- **What.** A job that rewrites a table (projection rebuild, catalog sync) updates only changed rows: `INSERT ... ON CONFLICT (...) DO UPDATE ... WHERE <something changed>` plus `DELETE` of rows that disappeared. Never delete all rows and insert them again on a schedule: every row then rewrites every index (GIN indexes are the most expensive), the table bloats with dead rows, and readers see an empty table between the two steps.
- **Large one-off imports** may drop and recreate a secondary index around the load only inside a planned migration with downtime; normal jobs do not.

### 2.5 Hand-written SQL

Raw SQL in a migration (`migrationBuilder.Sql`) or in code (`ExecuteSqlRaw`, outbox queries) follows the same rules: an index for every FK and filter, parameters instead of string concatenation, and the schema-qualified table name of the owning module.

## 3. Transactions

### 3.1 One command, one SaveChanges

- **What.** A command handler changes its aggregates and calls `SaveChanges` exactly once at the end. EF wraps that call in one transaction: either everything is saved or nothing is.
- **Repositories never save on their own.** A generic repository base (for example Ardalis `RepositoryBase`) whose `AddAsync`, `AddRangeAsync`, `UpdateAsync`, `DeleteAsync`, `DeleteRangeAsync` call `SaveChanges` inside them breaks this rule: a handler with two such calls commits twice and can stop half-way. The module's repository overrides these methods to only mark changes (`Set<T>().Add/Update/Remove...`) and the handler calls `SaveChangesAsync` once. An architecture test forbids the auto-saving methods in handlers, or the override makes them safe.
- **Where.** Every `Commands/` handler and every job step.
- **Check.** A handler or job method with more than one `SaveChangesAsync`, or a repository `Add*/Update*/Delete*` call followed by more writes, or `ExecuteDelete`/`ExecuteUpdate` mixed with `SaveChanges` without an explicit transaction.

### 3.2 When an explicit transaction is needed

Use `await using var tx = await db.Database.BeginTransactionAsync(ct); ... await tx.CommitAsync(ct);` when one operation needs more than one database round trip that must succeed together:

- `ExecuteDelete`/`ExecuteUpdate`/raw SQL together with `SaveChanges` (for example "replace all rows of a projection");
- an id from the first `SaveChanges` is needed to build the second write (better: let EF fix up the relation in one `SaveChanges`);
- a lock must be held across reads and writes (section 4.3).

In Catalog a command handler does this through `ICatalogTransaction.RunAsync`: it commits only when the handler returns a success, so a refusal or an exception after the first save leaves nothing behind (`CatalogTransactionPostgresTests`).

Keep a transaction short: no HTTP calls, file uploads, or waiting on users inside it.

### 3.3 Long jobs

A sync of thousands of rows is not one transaction for hours: it holds locks and blocks the site. Split it into steps (one root, one batch); each step is atomic and the whole job is safe to run again (idempotent). A step that replaces a set of rows (delete missing + upsert present) is one transaction.

### 3.4 Side effects outside the database

- A call to another system (1C, Telegram, e-mail, CDN, site revalidation) never sits inside a transaction and is never "save, then call, then save again". Write an outbox row in the same `SaveChanges` and send it after the commit ([module rules](module-rules.md#transactional-outbox)).
- A file uploaded to storage before the database write is deleted again if the write fails (compensation); the database row is written last.

## 4. Concurrency

Two requests or jobs can change the same data at the same time. Pick a protection per case:

| Method | How it works | Use for |
| --- | --- | --- |
| Atomic conditional update | One statement changes the row only if it is still in the expected state: `ExecuteUpdateAsync` on `.Where(x => x.Id == id && x.Status == Pending)`; 0 affected rows means someone else was first | Status changes, claiming a job or message, counters, stock decrement (`SET Stock = Stock - @q WHERE Stock >= @q`) |
| Optimistic locking | The row has a version: PostgreSQL `xmin` (`builder.Property<uint>("xmin").IsRowVersion()`) or a `Version` column with `IsConcurrencyToken()`. EF adds `WHERE xmin = @old` to the update; on `DbUpdateConcurrencyException` return 409 "data changed, reload" or retry the whole use case | Edit forms in the admin, aggregates edited rarely by several people, scheduler rows |
| Pessimistic locking | `SELECT ... FOR UPDATE` inside a transaction makes others wait; `FOR UPDATE SKIP LOCKED` lets workers skip taken rows; `pg_advisory_xact_lock(<key>)` locks a named operation, not a row | Queues and outbox dispatch, jobs that must not run twice at once (projection rebuild, imports), hot rows with frequent conflicts |

Rules:

- **Duplicates:** a unique index guards every "must exist once" rule (idempotency key, slug, number). Code that inserts after a "does it exist?" check catches the unique violation (`PostgresException` with `SqlState == PostgresErrorCodes.UniqueViolation`) and answers as if the first request won: for an idempotent create, return the existing result, not 500.
- **Double submit:** every create endpoint the client may repeat (orders, payments, messages) takes an idempotency key stored with a unique index; where it is required and how the handler answers a repeat is in the [idempotency rules](idempotency-rules.md).
- **Jobs that rewrite shared data** take `pg_advisory_xact_lock` with a constant key per operation at the start of their transaction, so a scheduled run and a manual run never overlap. A scheduler that launches tasks separately also groups tasks that touch the same data so only one of them runs at a time.
- **Admin forms:** the entity has `public uint Version { get; private set; }` mapped with `IsRowVersion()` (PostgreSQL `xmin`); the read DTO carries `version` (0 while the row does not exist); the update request sends it back as an optional `version`, and the handler returns `RowVersions.Changed<T>()` (409, Ukrainian message) when `RowVersions.IsStale` says it differs. A request without `version` is not checked, so an older client keeps working. A `DbUpdateConcurrencyException` that no handler catches becomes the same 409 in `ExceptionBehavior`.
- **Numbers and positions:** never `MAX(x) + 1` for a business number that must be unique; use a database sequence or the identity id. For display order (`Position`) `MAX + 1` is acceptable when a duplicate only changes order.
- **Check.** A read-check-write without a version, unique index, conditional update, or lock on data that two actors can change; a `catch` that is missing for the unique violation behind an idempotency key; a job that can run twice at once.

## 5. Migrations

### 5.1 How migrations reach production

- **What.** Migrations run as a separate step of the deploy pipeline before the new code starts: a job (a `Host.Jobs --job=migrate` command or `dotnet ef migrations bundle`) that runs once, with one instance and no automatic retry, and the pipeline waits for it and stops the deploy on failure.
- **Never** `Database.Migrate()`/`MigrateAsync()` at API startup: several instances would migrate at once, and a failed migration would stop the API from starting.
- **`dotnet ef migrations script --idempotent`** produces the SQL for review: generate it in CI for a pull request that adds a migration and read it for `DROP`, `ALTER COLUMN TYPE`, table rewrites, and missing indexes.
- **Where.** The deploy workflow, `Host.Jobs`, operations documentation.
- **Check.** No `Migrate`/`MigrateAsync` call outside the migration job; the workflow runs the job before the service deploy and waits for it.

### 5.2 Backward-compatible migrations (expand, then contract)

Between the migration step and the new code there are minutes when the **old** code runs on the **new** schema. Every migration must keep the previous release working:

1. Expand: add new tables, nullable columns or columns with defaults, new indexes; copy data.
2. Deploy code that uses the new shape.
3. Contract in a later release: drop the old column or table.

Renaming or dropping a column, or making it `NOT NULL` without a default, in the same release as the code change is not allowed.

### 5.3 Locks during migrations

- Start the migration session with `SET lock_timeout = '10s'` (and a `statement_timeout` for data steps): a migration that waits for a lock fails fast instead of queuing every site query behind it. The deploy can be re-run.
- Create an index on a large table with `CREATE INDEX CONCURRENTLY` in its own migration that does not run in a transaction (`migrationBuilder.Sql(..., suppressTransaction: true)`); small tables may use a normal index.
- Data back-fills of many rows run in batches, not one `UPDATE` of the whole table.

### 5.4 Other migration rules

- Never edit a migration that production has applied; add a new one.
- Each module has its own schema and migration history table ([module rules](module-rules.md#database-schema-per-module)).
- A migration has a tested `Down` or a written recovery plan in its SDD.
- Seed data is idempotent and runs in the migration job after the migrations.

## KedrStore specifics

- EF Core model mapping lives in Infrastructure (`Persistence/<Aggregate>/`); Domain entities hold no persistence logic. Lookup and seed data change only through the seeders the migration job runs, not through hidden runtime writes.
- Migrations: `Host.Jobs --job=migrate` (`src/Bootstrapper/Host.Jobs/DatabaseMigrationJob.cs`) runs every module's migrations, then the seeders. The deploy workflow (`.github/workflows/deploy-cloudrun.yml`, step «Run database migrations») runs it as the Cloud Run Job `database-migrate` with one task and no retries, waits, and only then deploys `Host.Api`. `Host.Api` never migrates. Details and data jobs after a migration: [operations](../operations/data/migrations-and-seeding.md). Check a new migration against an empty test database and describe rollout and rollback in its SDD.
- Search helper: `SearchWords` (`src/BuildingBlocks/BuildingBlocks.Application/Text/SearchWords.cs`), used by every module. `pg_trgm` is declared by the Catalog, Sales, Media and Identity contexts. Trigram indexes: `ProductListProjections.SearchTextUk/SearchTextRu`, `Products.Name`, `CatalogProductGroups.Name`, `Counterparties` `Id`/`Name`/`Phone`, `Orders.OrderNumber`, `OrderDrafts.Title`, `MediaFiles.FileName`, `AspNetUsers` `Email`/`FullName`. When a column has two indexes, name both in `HasIndex(..., "Name")`, or EF merges them.
- Concurrency already in place: `xmin` on `AppRefreshSessions`, `Version` tokens on `ScheduledTasks` and `OrderDrafts`, `FOR UPDATE SKIP LOCKED` in the shared outbox, unique idempotency keys for orders and drafts with `UniqueViolation.Is` (`BuildingBlocks.Infrastructure/Persistence`) catching the race, `pg_advisory_xact_lock` in the product list rebuild, and the scheduler's `catalog` group that keeps catalog tasks from running at once. Repositories derive from `UnitOfWorkRepositoryBase` and never save.
- The product list projection (`ProductListProjections`) is rebuilt by 1C tasks and by «Перебудувати список товарів»; rules 2.4, 3.2 and 4 (advisory lock) apply to it.

## Known deviations

Found by the database audit of 2026-11-15; each is closed by its own change and removed from this table. Closed on 2026-11-15: the product list rebuild (one locked transaction, diff), the 1C price sync (load once, save once), auto-saving repositories (`UnitOfWorkRepositoryBase` + `RepositorySaveTests`), per-word trigram search, order list indexes, the double-submitted order, the public model catalogue cache, and `lock_timeout` in the migrator. Closed by kedr-web #111: the six Admin edit forms send `version` back and reload on 409. Rule 2.3: production statistics showed both suspected `CategoryId` indexes in use (13,518 and 1,549 scans), so no index was dropped; the leftover Hangfire tables were dropped by the Scheduling migration `DropLegacyHangfireTables` at the owner's request.

| Rule | Where | Status | Plan |
| --- | --- | --- | --- |
| 3.1 | New rows whose database id the next step needs are saved before that step in jobs: `ProductGroupBuilder` run by the import (new groups), `SyncCdnMediaJob` (new folders), the attribute import steps. The Catalog commands that did it (`CreateProductGroup`, `CreateSiteModel`, `CreateCatalogAttribute`, `SetProductGroup`, `AcceptSiteModels`, `FillSiteModelFromNames`, `SetProductAttribute`, `RestoreProductAttribute`) run in one transaction through `ICatalogTransaction` since 2026-11-17 | open | Jobs: section 3.3 (each step re-runs safely); ids made before saving if a job step must become atomic |
| 1.4 | `SiteModelStateReader` loads the whole catalog for each admin model page | open | Acceptable for a few staff users; revisit if the admin pages slow down |
