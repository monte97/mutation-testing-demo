# Come il mutation testing mi ha fatto capire che la suite stava mentendo

Slide e demo del talk su **mutation testing** — Working Software Conference 2026.
Speaker: Francesco Montelli · [montelli.dev](https://montelli.dev)

## Cosa trovi qui

- **`slides.pdf`** — le slide del talk.
- **`demo/`** — il progetto .NET della demo, in due stati:
  - `demo/before/` — suite di test debole: **10 test, coverage ~93%, mutation score 65%**.
  - `demo/after/` — suite migliorata coi due fix guidati dal report: **22 test, mutation score 92%**.
  - `demo/saved-reports/` — i report HTML pre-generati (coverage + Stryker before/after), apribili **senza far girare nulla**.

## La tesi in una riga

La coverage ti dice **quanto codice esegui**. Il mutation score ti dice **quanto bene lo verifichi**.

## Provare la demo

Serve Docker. Da `demo/before/` (o `demo/after/`):

```bash
make build      # build immagine Docker
make test       # i test (verdi)
make stryker    # mutation testing con Stryker.NET -> mutation score
```

Oppure apri direttamente i report già generati in `demo/saved-reports/` nel browser.

## Provare Stryker sul tuo codice

```bash
dotnet tool install -g dotnet-stryker
dotnet stryker
```

## Contatti

[montelli.dev](https://montelli.dev) · [github.com/monte97](https://github.com/monte97)
