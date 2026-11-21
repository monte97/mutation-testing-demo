# Tentativo 1: l'agente copre il confine

Modello: claude-sonnet-5-5 (`claude -p`), 2026-11-21. Tool: lettura, modifica, `dotnet test`, `dotnet build`.

## Prompt

```
Leggi AGENTS.md. Scrivi i test per la regola 4 (ordini sopra 1.000 € in revisione) in tests/MutationTestingDemo.Tests/OrderServiceTests.cs. Quando hai finito, dimmi se la regola è coperta.
```

## Risposta finale

Ho scritto otto test per la regola 4 e `dotnet test` è verde (30 su 30). Però non posso dirti che la regola è coperta: il controllo che l'avrebbe dimostrato non è partito.

**Cosa ho aggiunto** in `tests/MutationTestingDemo.Tests/OrderServiceTests.cs`. Sconto e spedizione sono fissati con stub, così il totale è esattamente quello voluto:
- totale 1000 esatto → `Confirmed`
- totale 1000,01 → `RequiresReview`
- totale 999,99 → `Confirmed`
- totale 5000 → `RequiresReview`
- subtotale 1100 con sconto 100 → totale 1000, `Confirmed`. Così la soglia si confronta col totale, non col subtotale.
- subtotale 996 con spedizione 5 → totale 1001, `RequiresReview`
- `HighValueThreshold == 1000`
- un caso con i servizi reali: cliente VIP, totale 1040 → `RequiresReview`

**Cosa non ho verificato:** volevo rompere `OrderService.cs` a mano con cinque modifiche e controllare che i test fallissero. Le modifiche erano `>` → `>=`, `>` → `<`, soglia a 1001, segno dello sconto invertito e stato del ramo alto scambiato. Il comando è stato bloccato: prima un errore di parsing dello shell, poi la richiesta di approvazione, che non è arrivata. Non ho riprovato in altro modo. `OrderService.cs` non è stato toccato, ma non ho rieseguito `git status`.

Dai valori scelti mi aspetto che i cinque mutanti falliscano, perché i test su 1000 e 1000,01 sono pensati per `>=` e per la soglia. Resta un'attesa, non una misura. Per averla puoi lanciare Stryker, oppure approvarmi `bash /tmp/mut.sh` (lo script è già pronto) e ti riporto l'esito.

## Diff dei test

```diff
diff --git a/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs b/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs
index 566bdbe..1bb205e 100755
--- a/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs
+++ b/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs
@@ -125,4 +125,111 @@ public class OrderServiceTests
         Assert.Equal(157.50m, result.Total);
         Assert.Equal(OrderStatus.Confirmed, result.Status);
     }
+
+    // --- Regola 4: totale > 1.000 € -> RequiresReview; fino a 1.000 € compresi -> Confirmed ---
+    // Sconto e spedizione sono fissati con stub, così il totale è esattamente quello voluto.
+
+    private sealed class FixedDiscount(decimal amount) : Interfaces.IDiscountService
+    {
+        public decimal CalculateDiscount(Customer customer, decimal subtotal) => amount;
+    }
+
+    private sealed class FixedShipping(decimal cost) : Interfaces.IShippingService
+    {
+        public (decimal Cost, ShippingType Type) CalculateShipping(Customer customer, decimal subtotal)
+            => (cost, ShippingType.Standard);
+    }
+
+    private static OrderResult Process(decimal subtotal, decimal discount = 0m, decimal shipping = 0m)
+    {
+        var sut = new OrderService(new FixedDiscount(discount), new FixedShipping(shipping));
+        var order = new Order
+        {
+            Customer = new Customer { Name = "Test" },
+            Items = new List<OrderItem>
+            {
+                new() { ProductName = "Prodotto", UnitPrice = subtotal, Quantity = 1 }
+            }
+        };
+        return sut.ProcessOrder(order);
+    }
+
+    [Fact]
+    public void TotalExactlyAtThreshold_ShouldBeConfirmed()
+    {
+        var result = Process(subtotal: 1000m);
+
+        Assert.Equal(1000m, result.Total);
+        Assert.Equal(OrderStatus.Confirmed, result.Status);
+    }
+
+    [Fact]
+    public void TotalJustAboveThreshold_ShouldRequireReview()
+    {
+        var result = Process(subtotal: 1000.01m);
+
+        Assert.Equal(1000.01m, result.Total);
+        Assert.Equal(OrderStatus.RequiresReview, result.Status);
+    }
+
+    [Fact]
+    public void TotalJustBelowThreshold_ShouldBeConfirmed()
+    {
+        var result = Process(subtotal: 999.99m);
+
+        Assert.Equal(OrderStatus.Confirmed, result.Status);
+    }
+
+    [Fact]
+    public void TotalWellAboveThreshold_ShouldRequireReview()
+    {
+        var result = Process(subtotal: 5000m);
+
+        Assert.Equal(OrderStatus.RequiresReview, result.Status);
+    }
+
+    [Fact]
+    public void Threshold_ShouldBeComparedWithTotalNotSubtotal_WhenDiscountBringsItDown()
+    {
+        // Subtotale 1100 sopra soglia, ma totale 1100 - 100 = 1000 -> confermato
+        var result = Process(subtotal: 1100m, discount: 100m);
+
+        Assert.Equal(1000m, result.Total);
+        Assert.Equal(OrderStatus.Confirmed, result.Status);
+    }
+
+    [Fact]
+    public void Threshold_ShouldBeComparedWithTotalNotSubtotal_WhenShippingPushesItUp()
+    {
+        // Subtotale 996 sotto soglia, ma totale 996 + 5 = 1001 -> revisione
+        var result = Process(subtotal: 996m, shipping: 5m);
+
+        Assert.Equal(1001m, result.Total);
+        Assert.Equal(OrderStatus.RequiresReview, result.Status);
+    }
+
+    [Fact]
+    public void HighValueThreshold_ShouldBe1000()
+    {
+        Assert.Equal(1000m, OrderService.HighValueThreshold);
+    }
+
+    [Fact]
+    public void RealServices_VipOrderAboveThreshold_ShouldRequireReview()
+    {
+        // VIP, subtotale 1300: sconto 20% = 260, spedizione gratuita, totale 1040
+        var order = new Order
+        {
+            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 0 },
+            Items = new List<OrderItem>
+            {
+                new() { ProductName = "Monitor", UnitPrice = 650m, Quantity = 2 }
+            }
+        };
+
+        var result = _sut.ProcessOrder(order);
+
+        Assert.Equal(1040m, result.Total);
+        Assert.Equal(OrderStatus.RequiresReview, result.Status);
+    }
 }
```
