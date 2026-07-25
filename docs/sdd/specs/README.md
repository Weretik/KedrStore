# Feature specifications

Every non-trivial change starts here before implementation.

Reusable task templates are in [_templates](_templates/). Read its README first and choose the folder by work type.

## Layout

~~~text
docs/specs/
  <module>/
    <feature-slug>/
      README.md                    parent SDD
      contracts/
        api-contract.md            when a client consumes the API
      phases/
        01-domain.md through 06-frontend-handoff.md
  _templates/
    feature/                ordinary feature and API contract templates
    migration/              multi-phase migration templates
    git/                    commit planning template
~~~

Use a module folder such as catalog, sales, identity, platform or cross-module. Use lowercase kebab-case feature and phase names.

For an ordinary backend feature, start with feature/README.md: it contains the standard six-phase implementation flow. Remove only inapplicable phases and record the reason in the parent SDD. Link to architecture and standards rather than copying them.

## Lifecycle

Draft → accepted → in-progress → verified → completed.

An implementation must not silently diverge from an accepted specification. Add a dated change note with the reason, scope impact and new verification when it does.
