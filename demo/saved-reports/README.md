# Report pre-generati (Piano B per il talk)

Questi report sono committati su git come backup nel caso la demo live fallisca.

## Contenuto

| Cartella | Cosa mostra | Numero chiave |
|----------|-------------|---------------|
| `coverage-before/` | Code coverage dei 10 test before | **93% line, 80% branch** |
| `stryker-before/` | Mutation testing con test before | **65% mutation score, 12 survived** |
| `stryker-after/` | Mutation testing con test after | **92% mutation score, 3 survived** |

## Rigenerare i report

```bash
make build             # build Docker image
make coverage          # genera coverage report HTML
make stryker-before    # genera Stryker report before
make stryker-after     # genera Stryker report after
```

## Aprire i report salvati

```bash
make open-coverage-saved    # apri coverage before (93%)
make open-stryker-saved     # apri Stryker before (65%)
make open-stryker-after-saved  # apri Stryker after (92%)
```
