# ADR 0002: Use PostgreSQL as Primary Database

## Status
Accepted

## Date
2025-06-04

## Context
KedrStore is an e-commerce platform with high requirements for data integrity, performance, and extensibility. A relational database with strong consistency guarantees is necessary. 

Options considered:
- PostgreSQL
- Microsoft SQL Server
- MySQL/MariaDB
- SQLite (for prototyping only)

PostgreSQL was chosen due to its:

- Full support for relational and JSONB data
- Mature ecosystem and extensions (e.g. `pg_stat_statements`, `pgvector`)
- Open-source license and cost efficiency
- Rich tooling (pgAdmin, psql, backups, migration tools)
- Advanced indexing, full-text search, and performance features

## Decision
We will use **PostgreSQL** as the primary relational database for all environments (dev/stage/prod).  
EF Core will be used as the ORM, with Fluent API mappings and migrations.

## Consequences

### Positive
- Strong support for transactions, ACID compliance
- Advanced features like JSONB, UUID, constraints, functions
- Seamless integration with EF Core and Npgsql
- Cost-effective for cloud and on-premise

### Negative
- Developers must be familiar with PostgreSQL-specific syntax
- Slightly more complex setup than SQLite for local testing
