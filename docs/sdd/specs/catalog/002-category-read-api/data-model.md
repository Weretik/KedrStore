# API читання категорій — модель даних

## Контекст

Фіча доступна лише для читання. Вона проєктує наявний агрегат `ProductCategory`; таблиці, стовпці, міграції чи збережені зв’язки не додаються.

## Модель

```text
ProductCategory (existing aggregate)
├── Id: positive integer public identity
├── Slug: public route key
├── ParentId: nullable immediate-parent identity
├── ProductTypeIdOneC: admin-visible imported root/source key
├── Name: admin-visible original source name
├── ShortNameUk / ShortNameRu: locale-selected public name
├── SortOrder: sibling display order
├── Level: root-relative depth
└── Path: internal hierarchy source; not exposed
```

## Інваріанти та цілісність

- Проєкція API не розкриває сутність EF або технічний `Path`; вона використовує DTO, специфічні для завдань.
- `ParentId`, `Level` і `Children` мають описувати одне узгоджене дерево категорій без циклів. Цілісність і далі забезпечують наявні правила імпорту категорій і домену.
- Публічна відповідь зберігає наявне впорядкування сусідів спершу за `SortOrder`, потім за `Id`. Сортування, задане клієнтом, не приймається.
- `name` вибирається лише зі збережених локалізованих коротких назв; воно ніколи не змінює стабільні поля `id`, `slug`, `parentId`, `sortOrder` або `level`.
- Контракт адміністратора може надавати початкове `Name`, обидві локалізовані короткі назви та `ProductTypeIdOneC`; технічний `Path` і далі не розкривається, як і сутність зберігання.