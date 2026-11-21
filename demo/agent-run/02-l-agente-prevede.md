# L'agente legge e prevede

Modello: claude-sonnet-5-5 (`claude -p`), 2026-11-21. Tool: solo lettura (Read, Glob, Grep): niente test, niente modifiche.

## Prompt

```
Leggi AGENTS.md e i test in tests/. Le regole di dominio sono tutte coperte da un test che fallisce se la regola si rompe? Rispondi in breve, regola per regola.
```

## Risposta

Quattro regole su cinque sono coperte. La regola 5 no. Ho letto `OrderServiceTests.cs` e `OrderService.cs`. Non ho eseguito `dotnet test` e non ho aperto `DiscountServiceTests` né `ShippingServiceTests`.

1. **Ordine senza prodotti:** coperta. `EmptyOrder_ShouldThrowWithMessage` controlla eccezione e messaggio, `NullItems_ShouldThrow` il caso `Items` nullo.
2. **Subtotale:** coperta. `VipOrder_ShouldCalculateCorrectTotals` e `MultipleItems_ShouldSumCorrectly` usano più righe con quantità maggiori di 1, quindi rompere somma o moltiplicazione fa fallire il test.
3. **Totale = subtotale − sconto + spedizione:** coperta. Ci sono tre combinazioni diverse (VIP 170, standard 35, express 157,50), quindi un segno invertito viene scoperto.
4. **Sopra 1.000 € revisione, fino a 1.000 € compresi conferma:** coperta. `HighValueOrder_ExactlyAt1000_ShouldBeConfirmed` e `HighValueOrder_JustAbove1000_ShouldRequireReview` testano il confine da entrambi i lati. Cambiare `>` in `>=` fa fallire il primo.
5. **Totale non positivo in revisione:** **non coperta.** Nessun test ha un totale ≤ 0. In `OrderService.cs:42` si potrebbe cambiare `total > 0` in `total >= 0` o in `true` e tutti i test resterebbero verdi.

Per chiudere il buco basta un test con un cliente VIP e un articolo a prezzo 0 (totale esattamente 0, quindi `RequiresReview`). Un secondo test con prezzo negativo coprirebbe il caso strettamente sotto zero.

Non ho verificato a mano `DiscountService`: il test sul 1000,01 assume uno sconto VIP del 20% senza arrotondamenti.
