# SDD templates

For a new backend feature or migration, create `docs/sdd/specs/<module>/<NNN>-<feature-slug>/` and copy `feature/` into it. Do not create empty documents.

```text
<NNN>-<feature-slug>/
├── README.md                 feature orchestration
├── requirements/             what is needed and why
├── design/                   how the backend implements the feature
├── data-model.md             entities, relations, and invariants
├── contracts/                API and integration contracts
├── tasks/                    phases and atomic AI tasks
└── checklist/                quality gates
```

`git/git-commit-batching.md` is a separate template for commit planning only. For a migration, use this same template: document the baseline and rollback in `design/`, and rollout and verification in `tasks/`.
