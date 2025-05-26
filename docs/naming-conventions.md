# Naming Conventions in KedrStore Project

## General Principles

1. **Clarity and self-documentation** – names should clearly describe their purpose
2. **Consistency** – similar concepts should follow the same naming patterns
3. **Meaningfulness** – avoid abbreviations except for well-known ones (ID, HTTP, URL, etc.)
4. **Length** – names should be descriptive but not unnecessarily long

---

## Naming Style

| Code Element       | Style                   | Example                        |
|--------------------|--------------------------|--------------------------------|
| Class              | `PascalCase`             | `ProductService`               |
| Interface          | `PascalCase` + `I`       | `IProductRepository`           |
| Method             | `PascalCase`             | `GetProductById`               |
| Property           | `PascalCase`             | `ProductName`                  |
| Parameter          | `camelCase`              | `productId`                    |
| Local variable     | `camelCase`              | `currentUser`                  |
| Private field      | `_camelCase` (underscore)| `_orderItems`                  |
| Constant           | `UPPER_CASE` or Pascal   | `MAX_RETRY_COUNT`, `MaxRetries`|
| Enum               | `PascalCase`             | `OrderStatus`                  |
| Enum value         | `PascalCase`             | `OrderStatus.Pending`          |

---

## File Naming

1. **Classes** – file name must match class name: `ProductService.cs`
2. **Interfaces** – file name must match interface name: `IProductRepository.cs`
3. **Blazor Components** – `PascalCase` with descriptive suffix: `ProductDetail.razor`
4. **Tests** – class name ends with `Tests`: `ProductServiceTests.cs`

---

## Naming by Layer

### Domain Layer

- **Entities**: Singular noun, no suffix (`Product`, not `ProductEntity`)
- **Repository Interfaces**: `I{Entity}Repository` (e.g. `IOrderRepository`)
- **Domain Services**: `{Functionality}Service` (e.g. `DiscountCalculationService`)
- **Enums**: Clear noun (e.g. `OrderStatus`, `PaymentMethod`)

### Application Layer

- **DTOs**: `{Entity}Dto`, or `{Entity}{Action}Dto` (e.g. `ProductDto`, `ProductCreateDto`)
- **Commands**: `{Action}{Entity}Command` (e.g. `CreateOrderCommand`)
- **Queries**: `{Action}{Entity}Query` (e.g. `GetProductByIdQuery`)
- **Handlers**: `{CommandOrQuery}Handler` (e.g. `CreateOrderCommandHandler`)
- **Validators**: `{DtoOrCommand}Validator` (e.g. `ProductCreateDtoValidator`)

### Infrastructure Layer

- **Repositories**: `{EntityName}Repository` (e.g. `ProductRepository`)
- **DbContext**: Ends with `DbContext` (e.g. `KedrStoreDbContext`)
- **EF Configs**: `{Entity}Configuration` (e.g. `OrderConfiguration`)
- **Infrastructure Services**: `{Feature}Service` (e.g. `EmailService`)

### Presentation Layer

- **Controllers**: Ends with `Controller` (e.g. `ProductsController`)
- **View Models**: `ProductViewModel` or `ProductVM`
- **Blazor Components**: `{Feature}{ComponentType}` (e.g. `ProductDetail`, `OrderList`)
- **Blazor Pages**: Named by purpose (e.g. `OrderManagement.razor`)

---

## Abbreviations and Acronyms

- Use standard ones only: `ID`, `HTTP`, `URL`, `DTO`, `API`
- For 2-letter acronyms: all uppercase (`ID`), for longer – first letter only (`Http`)
- Avoid arbitrary short forms (`mgr` → use `manager`, `utils` → use `utilities`)

---

## Prefixes and Suffixes

- **Avoid** Hungarian notation (`strName`, `intCount`)
- **Avoid** meaningless prefixes like `cls`, `obj`
- **Allowed suffixes**:
    - `Service`, `Repository`, `Factory`, `Handler`, `Controller`, `Dto`, `Validator`, `Provider`

---
~~~~
## Specific Conventions

- **Async methods**: add suffix `Async` (e.g. `GetProductByIdAsync`)
- **Extension methods**: natural naming (e.g. `string.IsNullOrEmpty()`)
- **Boolean properties**: use `Is`, `Has`, `Can` (e.g. `IsActive`, `HasAccess`, `CanDelete`)

---

## Exception Handling

- **Exception class names**: end with `Exception` (e.g. `ProductNotFoundException`)
- **Exception variable names**: use `ex`, `exception`, or descriptive (`validationException`)

---

## Test Naming

- **Test class**: ends with `Tests` (e.g. `OrderServiceTests`)
- **Test method**: `{MethodName}_{Scenario}_{ExpectedOutcome}`  
  Example: `GetProductById_WithValidId_ReturnsProduct`

---

## Namespaces

- Structure: `KedrStore.{Layer}.{Module}.{Submodule}`  
  Examples:
    - `KedrStore.Domain.Orders`
    - `KedrStore.Application.Products.Commands`
    - `KedrStore.Infrastructure.Persistence.Repositories`
    - `KedrStore.Presentation.Components.Orders`

---

## Additional Recommendations

1. Avoid names that conflict with C# keywords
2. Use plural names for collections: `IEnumerable<Product> products`
3. Avoid negations in boolean names (`!isNotValid` → use `isValid`)
4. Keep method names and parameter names consistent

---

## Exceptions to the Rules

- Deviations allowed if they improve clarity
- Must be reviewed and agreed by the team

---

## Tools for Enforcement

- **IDE-level**: ReSharper, Rider
- **Code analyzers**: StyleCop, SonarQube
- **.editorconfig**: formatting and naming rules