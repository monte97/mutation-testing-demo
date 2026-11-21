# AGENTS.md

Regole per chi lavora su questo progetto, persone e agenti.

## Dominio degli ordini

1. Un ordine senza prodotti non si elabora: `InvalidOrderException`.
2. Il subtotale è la somma di prezzo per quantità di ogni riga.
3. Il totale è subtotale meno sconto più spedizione.
4. Un ordine con totale sopra 1.000 € va in revisione (`OrderStatus.RequiresReview`). Fino a 1.000 € compresi si conferma.
5. Un ordine con totale non positivo va in revisione.

## Test

- Test con xUnit in `tests/MutationTestingDemo.Tests`, un file per servizio.
- Ogni regola di dominio ha un test che fallisce se la regola si rompe.
- Prima di consegnare: `dotnet test` verde.
