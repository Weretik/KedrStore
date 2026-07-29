# ADR 0031: Use EF Core Migrations

## Date
2025-06-17

## Status
Accepted

## Context
Managing database schema changes is a critical aspect of application development. EF Core Migrations provide a way to define and apply schema changes in a controlled and repeatable manner. In the Kedr E-Commerce Platform, EF Core Migrations are used to ensure consistency and maintainability of the database schema across environments.

## Decision
We decided to use EF Core Migrations in the project to:

1. Automate the process of applying schema changes to the database.
2. Ensure consistency in database schema across development, staging, and production environments.
3. Simplify the management of schema changes by leveraging EF Core's built-in tools.
4. Align with best practices for modern .NET applications.

## Consequences
### Positive
1. Automates the process of applying schema changes, reducing manual effort.
2. Ensures consistency in database schema across environments.
3. Simplifies the management of schema changes.
4. Promotes maintainability by using a version-controlled approach to schema changes.

### Negative
1. Requires developers to understand and correctly implement migrations.
2. Adds complexity to the development workflow.
3. May lead to issues if migrations are not properly tested before deployment.

## Example
EF Core Migrations are implemented as follows:

**Adding a Migration**:
```bash
dotnet ef migrations add InitialCreate
```

**Applying Migrations**:
```bash
dotnet ef database update
```

**Example Migration File**:
```csharp
public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Products",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Price = table.Column<decimal>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Products", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Products");
    }
}
```
