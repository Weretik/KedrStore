# Перевірка фото товарів Catalog

`check-product-photos` один раз проходить усі активні товари Catalog і завершується. Він ігнорує `ExportToSite`, залишок, категорію та прапорці розпродажу й новинки. `Host.Jobs` не має планувальника; повторний запуск Cloud Run Job налаштовують окремо.

## Локальний запуск

```powershell
# Виконувати з кореня репозиторію
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project src/Bootstrapper/Host.Jobs/Host.Jobs/Host.Jobs.csproj -- --job=check-product-photos
```

Надай `ConnectionStrings__Default` через User Secrets `Host.Jobs` або змінну середовища. Команда не потребує секретів 1C чи Telegram і аргументу `--rootId`.

## Конфігурація

`ProductPhotoCheck` підтримує:

| Параметр | Типове значення | Обмеження |
| --- | ---: | --- |
| `BatchSize` | 500 | 1–5000 |
| `MaxConcurrency` | 16 | 1–100 |
| `RequestTimeoutSeconds` | 5 | 1–120 |
| `MaxRedirects` | 3 | 0–10 |
| `AllowedHosts` | `images-kedr.cdn.express` | Принаймні один непорожній host |

Обмежуй allow-list потрібним CDN. Redirect за межі HTTPS або дозволеного host відхиляється без наступного запиту.

## Тлумачення результату

- `Available`: фінальна відповідь `2xx` із `Content-Type: image/*`.
- `Missing`: `404` або `410`.
- `InvalidContentType`: фінальна відповідь `2xx` без типу медіа image.
- `CheckFailed`: timeout, збій мережі/DNS, некоректний URL, `429`, `5xx` або інший неуспішний результат.

Анонімний API недоступних фото включає останні три статуси. Окремі збої CDN не завершують завдання помилкою. Збій БД, некоректна конфігурація, скасування або непередбачений збій координації дають ненульовий код виходу.

Фінальний структурований лог містить лічильники examined, available, missing, invalid-content, check-failed, stale-result і тривалість. Очікувані віддалені збої не записують тіла відповіді або credentials.

## Cloud Run

Процес розгортання створює або оновлює `catalog-product-photo-check` з одним завданням, паралелізмом 1, нульовою кількістю повторних запусків платформи й обмеженням часу 30 хвилин. Процес розгортання не виконує його й не створює Cloud Scheduler. Розклад у Google Cloud налаштовують або зупиняють окремо.

До першого успішного запуску наявні backfilled-записи залишаються `Unknown` і не з'являються у списку недоступних. Якщо виконання скасовано, вже committed batches залишаються чинними, а наступний запланований запуск знову перевіряє весь активний каталог.
