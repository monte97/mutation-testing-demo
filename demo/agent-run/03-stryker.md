# Stryker misura

2026-11-21, `demo/agent` con i test della regola 4 scritti dall'agente (`01-test-dell-agente.md`), Stryker.NET 5.0.0.

- `dotnet test`: 24 verdi.
- Mutation score: **93,18%**.
- Sopravvissuti:

| File | Riga | Codice | Mutante |
|---|---|---|---|
| `OrderService.cs` | 42 | `total > 0 ? OrderStatus.Confirmed : OrderStatus.RequiresReview` | `total >= 0` |
| `OrderService.cs` | 42 | idem | condizione sempre `true` |
| `DiscountService.cs` | 31 | `if (discountPercentage > 0.25m)` | `>= 0.25m` |

I primi due sono quelli che l'agente aveva previsto leggendo (`02-l-agente-prevede.md`): la regola 5 di `AGENTS.md` non ha un test. Il terzo l'agente non l'aveva guardato: è il tetto dello sconto, ed è un mutante equivalente (con lo sconto al 25% esatto, `>` e `>=` danno lo stesso risultato).
