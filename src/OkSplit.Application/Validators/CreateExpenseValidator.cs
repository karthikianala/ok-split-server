using FluentValidation;
using OkSplit.Application.DTOs.Expense;

namespace OkSplit.Application.Validators;

public class CreateExpenseValidator : AbstractValidator<CreateExpenseDto>
{
    public CreateExpenseValidator()
    {
        RuleFor(x => x.GroupId)
            .NotEmpty().WithMessage("Group ID is required");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required")
            .MaximumLength(255);

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0");

        RuleFor(x => x.Category)
            .NotEmpty().WithMessage("Category is required");

        RuleFor(x => x.SplitType)
            .NotEmpty().WithMessage("Split type is required")
            .Must(s => s is "Equal" or "Exact" or "Percentage")
            .WithMessage("Split type must be Equal, Exact, or Percentage");

        RuleFor(x => x.Splits)
            .NotEmpty().WithMessage("At least one split participant is required");
    }
}
