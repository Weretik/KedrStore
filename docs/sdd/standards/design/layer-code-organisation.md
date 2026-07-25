# Layer and code organisation

Use feature-by-folder inside Application. Keep command/query, handler, validator and private DTOs together by use case. A handler owns one use case.

Place business invariants in Domain; orchestration in Application; adapters in Infrastructure; HTTP binding and Result mapping in API. Never inject Infrastructure implementations into Application.

Use explicit names: CreateOrderCommand, GetPublicProductListQuery, GetAdminProductListQueryHandler. Avoid generic folders and names such as Common, Misc, Helper, Utils or Manager.

Split code by independent responsibility, not by file size alone. Keep cohesive small code together; do not create ceremony-only abstractions.
