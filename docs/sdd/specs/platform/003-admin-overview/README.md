# Platform 003: data for the admin «Огляд»

**Status:** implemented 2026-11-05. **Owner modules:** Sales, Catalog, Content (one read endpoint each). **Feature card:** [Admin «Огляд» data](../../../features/platform/admin-overview.md).

## Goal

The owner chose (2026-11-05) the «Робочий стіл» layout for the admin home page: a list of what needs attention, today's orders and drafts, and the newest orders, built from real data instead of the demo numbers. Each module reports its own part, so no module reads another's tables.

**Out of scope:** sums of money and new customers on the home page (the owner asked to leave them out), pages without SEO texts (the site has a default for each), charts, a history of who changed what.

## Rules

- R-001 `GET /api/admin/order-workspace/overview` (`CanManageOrders`) returns orders created since the current Kyiv midnight, the number of saved drafts, the number of orders with sync status `BusinessError` or `DeadLetter` (the ones a manager can retry) with the newest 5 of them and their last error, and the newest 5 orders in the order-list shape.
- R-002 `GET /api/admin/catalog/overview` (`CanManageProducts`) returns the number of products without a linked photo (the same list as the admin «Без фото» tab), the category with most of them, and the number of active categories without any site text or FAQ in Ukrainian or in Russian; deleted products and categories are not counted.
- R-003 `GET /api/admin/content/overview` (`CanManageContent`) returns the number of unpublished articles with the one edited longest ago, the number of articles whose cover is still in the database, and the number of unpublished document versions.
- R-004 The endpoints only read and report counts of problems; the admin shows a line only when a count is above zero.

## Scenarios

- SC-001 An order 1C refused appears in the attention list with its number and reason until a manager retries it.
- SC-002 Linking a photo to the last product without one removes the «без фото» line.
- SC-003 A Manager without content access still gets the orders and catalog blocks; the admin hides a block whose request is refused.

## Data and rollout

No schema change. Deploy the backend first; the admin treats a failed request as «nothing to show» for that block. Rollback: remove the three actions.

## Evidence

Integration `WorkspaceOverviewReaderTests`, `AdminCatalogOverviewApiTests`, `ContentOverviewApiTests` (counts, lists, access by role), `ApiContractTests`; unit `WorkspaceDateRangeTests` (Kyiv midnight); architecture tests (10) pass.
