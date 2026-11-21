# Demo dell'Atto 5: l'agente prevede, la mutazione misura

Ricostruzione su `demo/agent` (copia di `demo/after` con la regola della soglia e un `AGENTS.md`), registrata il 2026-11-21 con `claude -p`. Nel talk si mostra come slide statiche, mai dal vivo.

| Passo | File | Cosa succede |
|---|---|---|
| 1 | `01-test-dell-agente.md` | L'agente scrive i test della regola 4 (soglia 1.000 €), prova da solo `>` → `>=` e segnala che la regola 5 non ha test |
| 2 | `02-l-agente-prevede.md` | Un agente in sola lettura risponde regola per regola: quattro coperte, la 5 no, e indica i mutanti che sopravviverebbero |
| 3 | `03-stryker.md` | Stryker conferma i due mutanti previsti e ne trova un terzo, equivalente, che l'agente non aveva guardato |
| 4 | `04-il-test-che-mancava.md` | Un test per la regola 5: score da 93,18% a 97,73% |

`tentativi/` raccoglie le prove non usate nel talk, tenute per traccia.

Per rifarla: `cd demo/agent && make build && make test && make stryker`. Le risposte di un agente cambiano a ogni esecuzione; quelle registrate sono qui.
