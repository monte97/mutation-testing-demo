# Il test che mancava

Un test per la regola 5: cliente VIP (spedizione gratuita), un articolo a prezzo 0, totale 0, l'ordine va in revisione.

```csharp
[Fact]
public void ZeroTotalOrder_ShouldRequireReview()
{
    // VIP: spedizione gratuita, quindi totale 0
    Assert.Equal(0m, result.Total);
    Assert.Equal(OrderStatus.RequiresReview, result.Status);
}
```

- `dotnet test`: 25 verdi.
- Mutation score: **97,73%**. I due mutanti della riga 42 sono uccisi.
- Resta solo l'equivalente su `DiscountService.cs:31`.

Report in `../saved-reports/stryker-agent-after/`.
