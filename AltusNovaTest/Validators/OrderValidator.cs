using AltusNovaTest.Data;
using AltusNovaTest.Models;
using FluentValidation;

namespace AltusNovaTest.Validators;

public class OrderValidator : AbstractValidator<Order>
{
    private readonly NovaContext _dbContext;

    public OrderValidator(NovaContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must contain at least one item");

        RuleForEach(x => x.Items).SetValidator(new OrderItemValidator(_dbContext));
    }
}

public class OrderItemValidator : AbstractValidator<OrderItem>
{
    private readonly NovaContext _dbContext;

    public OrderItemValidator(NovaContext dbContext)
    {
        _dbContext = dbContext;

        RuleFor(x => x.ProductId)
            .MustAsync(async (productId, cancellation) =>
            {
                var product = await _dbContext.Inventory.FindAsync(productId);
                return product != null;
            })
            .WithMessage("Product does not exist");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x)
            .MustAsync(async (item, cancellation) =>
            {
                var product = await _dbContext.Inventory.FindAsync(item.ProductId);
                return product != null && product.Quantity >= item.Quantity;
            })
            .WithMessage("Insufficient stock available");
    }
} 