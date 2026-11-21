# Tentativo 2: test della regola 4 con un prompt corto

Modello: claude-haiku-5-5 (`claude -p`), 2026-11-21. Tool: lettura, modifica, `dotnet test`, `dotnet build`.

## Prompt

```
Aggiungi un test per la regola 4 di AGENTS.md e dimmi se la regola è coperta.
```

## Risposta finale

Ho aggiunto due test sulla regola 4 e la regola è coperta sul confine, ma non lo era prima.

**Stato prima:** nessun test raggiungeva la soglia. Tutti gli ordini dei test finivano `Confirmed`, quindi un `>` cambiato in `>=` o la soglia spostata non facevano fallire nulla.

**Test aggiunti** in `tests/MutationTestingDemo.Tests/OrderServiceTests.cs`, con cliente VIP (sconto 20%, spedizione gratuita) per avere totali esatti:
- `HighValueOrder_ExactlyAt1000_ShouldBeConfirmed`: subtotale 1250 → totale 1000,00 → `Confirmed`.
- `HighValueOrder_JustAbove1000_ShouldRequireReview`: subtotale 1250,0125 → totale 1000,01 → `RequiresReview`.

**Verifica:**
- `dotnet test`: 24/24 verdi.
- Ho mutato `total > HighValueThreshold` in `>=` in `OrderService.cs`: il test `ExactlyAt1000` fallisce (atteso `Confirmed`, ottenuto `RequiresReview`). Ho ripristinato il sorgente e la suite è tornata verde. Il file `OrderService.cs` è identico a prima.

**Non coperto:** la regola 5 (totale non positivo → revisione). Il ramo `total > 0 ? Confirmed : RequiresReview` non è mai eseguito dai test. Non l'ho toccata perché non l'hai chiesta, ma se vuoi la chiudo con un test simile.

Non ho committato.

## Diff dei test

```diff
diff --git a/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs b/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs
index 566bdbe..ff61c1f 100755
--- a/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs
+++ b/demo/agent/tests/MutationTestingDemo.Tests/OrderServiceTests.cs
@@ -125,4 +125,45 @@ public class OrderServiceTests
         Assert.Equal(157.50m, result.Total);
         Assert.Equal(OrderStatus.Confirmed, result.Status);
     }
+
+    // Regola 4 (AGENTS.md): totale sopra 1.000 € va in revisione; fino a 1.000 € compresi si conferma.
+    // Cliente VIP: sconto 15% + 5% (subtotale > 200) = 20%, spedizione gratuita, quindi totale = subtotale * 0.8.
+
+    [Fact]
+    public void HighValueOrder_ExactlyAt1000_ShouldBeConfirmed()
+    {
+        var order = new Order
+        {
+            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 0 },
+            Items = new List<OrderItem>
+            {
+                new() { ProductName = "Notebook", UnitPrice = 1250m, Quantity = 1 }
+            }
+        };
+
+        var result = _sut.ProcessOrder(order);
+
+        // Subtotale 1250 - sconto 250 + spedizione 0 = 1000 (il confine e' incluso)
+        Assert.Equal(1000m, result.Total);
+        Assert.Equal(OrderStatus.Confirmed, result.Status);
+    }
+
+    [Fact]
+    public void HighValueOrder_JustAbove1000_ShouldRequireReview()
+    {
+        var order = new Order
+        {
+            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 0 },
+            Items = new List<OrderItem>
+            {
+                new() { ProductName = "Notebook", UnitPrice = 1250.0125m, Quantity = 1 }
+            }
+        };
+
+        var result = _sut.ProcessOrder(order);
+
+        // Subtotale 1250.0125 - sconto 250.0025 + spedizione 0 = 1000.01 (oltre la soglia)
+        Assert.Equal(1000.01m, result.Total);
+        Assert.Equal(OrderStatus.RequiresReview, result.Status);
+    }
 }
```
