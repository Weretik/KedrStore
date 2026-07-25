# SDD task templates

Choose the folder by the type of work. Do not combine templates from different folders unless the parent specification explicitly needs both.

~~~text
_templates/
├── feature/                 ordinary product/backend feature
│   ├── README.md            phase model and target feature-folder layout
│   ├── template-feature.md  parent feature specification
│   ├── contracts/
│   │   └── api-contract.md  client-visible API contract
│   └── phases/
│       ├── 01-domain.md through 06-frontend-handoff.md
│       └── 99-custom-phase.md
├── migration/               multi-phase structural/data/integration migration
│   ├── template-migration.md
│   └── phases/
└── git/                     commit planning only
    └── git-commit-batching.md
~~~

## Selection

| Need | Start with |
| --- | --- |
| New command, query, endpoint or business capability | feature/README.md, then feature/template-feature.md |
| API contract needs separate agreement | feature/contracts/api-contract.md |
| Standard backend implementation phase | feature/phases/01-domain.md through 06-frontend-handoff.md |
| Additional independent phase | feature/phases/99-custom-phase.md |
| Data, module, integration or contract transition | migration/template-migration.md |
| Intentional commit grouping | git/git-commit-batching.md |

Copy templates into the feature folder before editing them. Never edit a template as a record of an implemented feature.
