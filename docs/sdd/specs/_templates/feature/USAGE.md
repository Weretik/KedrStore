# Створення feature-специфікації

```text
Use $kedrstore-feature-spec.
Модуль: <catalog | sales | identity | platform>.
Feature: <NNN>-<feature-slug>.
Мета: <спостережуваний результат>.
У scope: <що має працювати>.
Поза scope: <що зараз не робимо>.
Джерела: <точні шляхи або немає>.
```

`У scope` — поведінка, яку потрібно описати й реалізувати. `Поза scope` — явно виключена поведінка. `Джерела` — наявні вимоги, contracts, designs або related features.

Skill готує SDD і зупиняється до implementation.
