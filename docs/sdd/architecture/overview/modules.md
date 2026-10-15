# Карта модулів

## Catalog

Шари: Catalog.Api, Catalog.Application, Catalog.Contracts, Catalog.Domain, Catalog.Infrastructure.

Відповідає за товари каталогу, категорії, типи цін, переклади, проєкцію списку товарів, інтеграцію створення замовлення, синхронізацію з 1C і публічне/адміністративне читання каталогу.

## Sales

Шари: Sales.Api, Sales.Application, Sales.Domain, Sales.Infrastructure.

Відповідає за клієнтів і контрагентів продажів, цінову політику та інтеграцію продажів із каталогом. Споживає Catalog через контракти й read-абстракції, а не через типи Catalog.Infrastructure.

## Identity

Шари: Identity.Api, Identity.Application, Identity.Domain, Identity.Infrastructure.

Відповідає за збереження ідентичностей, ролі, правила авторизації, сервіси сесії й поточного користувача та початкові дані. Бізнес-модулі використовують абстракції дозволів/поточного користувача, а не EF-типи Identity.

## BuildingBlocks

BuildingBlocks.Domain містить доменні примітиви. BuildingBlocks.Application містить Mediator behaviors, підтримку результатів/валідації та абстракції доменних подій. BuildingBlocks.Infrastructure містить спільні реалізації адаптерів. Це технічна основа, а не четвертий бізнес-модуль.

Перед додаванням або дублюванням спільних примітивів прочитай [довідник BuildingBlocks](../platform/building-blocks/README.md).
