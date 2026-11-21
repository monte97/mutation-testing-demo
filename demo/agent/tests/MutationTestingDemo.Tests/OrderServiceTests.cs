using MutationTestingDemo.Exceptions;
using MutationTestingDemo.Models;
using MutationTestingDemo.Services;

namespace MutationTestingDemo.Tests;

public class OrderServiceTests
{
    private readonly OrderService _sut = new(new DiscountService(), new ShippingService());

    [Fact]
    public void VipOrder_ShouldCalculateCorrectTotals()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 1 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Tastiera meccanica", UnitPrice = 80m, Quantity = 2 },
                new() { ProductName = "Mouse wireless", UnitPrice = 40m, Quantity = 1 }
            }
        };

        var result = _sut.ProcessOrder(order);

        // Subtotale: 80*2 + 40*1 = 200 (multi-item killa Sum->Max)
        Assert.Equal(200m, result.Subtotal);
        // VIP 15% di 200 = 30
        Assert.Equal(30m, result.DiscountAmount);
        // VIP -> spedizione gratuita
        Assert.Equal(0m, result.ShippingCost);
        Assert.Equal(ShippingType.Free, result.ShippingType);
        // Totale: 200 - 30 + 0 = 170
        Assert.Equal(170m, result.Total);
        // Status confermato
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public void MultipleItems_ShouldSumCorrectly()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Luigi Verdi", IsVip = false, LoyaltyYears = 0 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Cavo USB-C", UnitPrice = 10m, Quantity = 3 },
                new() { ProductName = "Mouse wireless", UnitPrice = 40m, Quantity = 2 }
            }
        };

        var result = _sut.ProcessOrder(order);

        // Subtotale: 10*3 + 40*2 = 110 (Quantity > 1 killa * -> /)
        Assert.Equal(110m, result.Subtotal);
    }

    [Fact]
    public void NonVipSmallOrder_ShouldHaveStandardShipping()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Luigi Verdi", IsVip = false, LoyaltyYears = 0 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Cavo USB-C", UnitPrice = 10m, Quantity = 3 }
            }
        };

        var result = _sut.ProcessOrder(order);

        // Subtotale 30, nessuno sconto, Standard 5.00
        Assert.Equal(30m, result.Subtotal);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(5.00m, result.ShippingCost);
        Assert.Equal(ShippingType.Standard, result.ShippingType);
        // Totale: 30 - 0 + 5 = 35 (killa +/- sulla spedizione)
        Assert.Equal(35m, result.Total);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public void EmptyOrder_ShouldThrowWithMessage()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Giulia Neri" },
            Items = new List<OrderItem>()
        };

        var ex = Assert.Throws<InvalidOrderException>(() => _sut.ProcessOrder(order));
        Assert.Contains("almeno un prodotto", ex.Message);
    }

    [Fact]
    public void NullItems_ShouldThrow()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Giulia Neri" },
            Items = null!
        };

        Assert.Throws<InvalidOrderException>(() => _sut.ProcessOrder(order));
    }

    [Fact]
    public void MediumOrder_ShouldGetExpressShipping()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Paolo Viola", IsVip = false, LoyaltyYears = 0 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Tastiera meccanica", UnitPrice = 75m, Quantity = 2 }
            }
        };

        var result = _sut.ProcessOrder(order);

        Assert.Equal(150m, result.Subtotal);
        Assert.Equal(0m, result.DiscountAmount);
        Assert.Equal(7.50m, result.ShippingCost);
        Assert.Equal(ShippingType.Express, result.ShippingType);
        Assert.Equal(157.50m, result.Total);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    // Regola 4 (AGENTS.md): totale sopra 1.000 € va in revisione; fino a 1.000 € compresi si conferma.
    // Cliente VIP: sconto 15% + 5% (subtotale > 200) = 20%, spedizione gratuita, quindi totale = subtotale * 0.8.

    [Fact]
    public void HighValueOrder_ExactlyAt1000_ShouldBeConfirmed()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 0 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Notebook", UnitPrice = 1250m, Quantity = 1 }
            }
        };

        var result = _sut.ProcessOrder(order);

        // Subtotale 1250 - sconto 250 + spedizione 0 = 1000 (il confine e' incluso)
        Assert.Equal(1000m, result.Total);
        Assert.Equal(OrderStatus.Confirmed, result.Status);
    }

    [Fact]
    public void HighValueOrder_JustAbove1000_ShouldRequireReview()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 0 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Notebook", UnitPrice = 1250.0125m, Quantity = 1 }
            }
        };

        var result = _sut.ProcessOrder(order);

        // Subtotale 1250.0125 - sconto 250.0025 + spedizione 0 = 1000.01 (oltre la soglia)
        Assert.Equal(1000.01m, result.Total);
        Assert.Equal(OrderStatus.RequiresReview, result.Status);
    }

    // Regola 5 (AGENTS.md): un ordine con totale non positivo va in revisione.
    [Fact]
    public void ZeroTotalOrder_ShouldRequireReview()
    {
        var order = new Order
        {
            Customer = new Customer { Name = "Mario Rossi", IsVip = true, LoyaltyYears = 0 },
            Items = new List<OrderItem>
            {
                new() { ProductName = "Omaggio", UnitPrice = 0m, Quantity = 1 }
            }
        };

        var result = _sut.ProcessOrder(order);

        // VIP: spedizione gratuita, quindi totale 0
        Assert.Equal(0m, result.Total);
        Assert.Equal(OrderStatus.RequiresReview, result.Status);
    }
}
