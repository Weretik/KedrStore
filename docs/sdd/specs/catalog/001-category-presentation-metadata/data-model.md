# Метадані представлення категорій — модель даних

## Контекст

Наявна таблиця `ProductCategories` зберігає належність до кореня 1C, оригінальну назву категорії, slug, ID батьківської категорії та шлях ltree. Їй потрібні дані для відображення, відокремлені від вихідної назви 1C, які можна споживати без відображення, підтримуваного storefront.

## Модель

```text
ProductCategory / ProductCategories
├── Id: ProductCategoryId (existing stable numeric ID)
├── ProductTypeIdOneC: string (existing root ownership)
├── Name: string (existing original 1C full name)
├── Slug: string (existing stable URL slug)
├── ParentId: ProductCategoryId? (existing hierarchy relation)
├── Path: CategoryPath / ltree (existing hierarchy path)
├── ShortNameUk: string (new configured Ukrainian display label)
├── ShortNameRu: string (new configured Russian display label)
├── SortOrder: int (new deterministic sibling/root order)
└── Level: int (new depth derived from final hierarchy)
```

## Інваріанти та цілісність

- `Name` є обов'язковим, зберігає наявну максимальну довжину 100 і залишається оригінальною назвою 1C.
- `ShortNameUk` і `ShortNameRu` є обов'язковими після заповнення міграцією, обрізаними від пробілів і непорожніми, з максимальною довжиною 100.
- `SortOrder` є детермінованим, невід'ємним і унікальним серед дочірніх категорій одного `ParentId` (зокрема серед кореневих). Для неналаштованої категорії використовується `Int32.MaxValue`; числовий ID категорії розв'язує можливу рівність.
- `Level >= 0`; коренева категорія має рівень `0`, а значення відповідає ієрархії, виведеній із `Path`/`ParentId`.
- Додавайте індекс лише тоді, коли погоджений запит дерева категорій потребує впорядкування в базі даних на додачу до поточного формування дерева в пам'яті; форма індексу має відповідати погодженому шаблону читання.
