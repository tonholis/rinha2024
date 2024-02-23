using FluentValidation;

namespace Api
{
	public class TransactionValidator : AbstractValidator<Transaction>
	{
        public TransactionValidator()
        {
            RuleFor(t => t.Amount).GreaterThanOrEqualTo(0);
            RuleFor(t => t.Description).MinimumLength(1).MaximumLength(10);
			RuleFor(t => t.Type).Matches("^[cd]$");
		}
	}
}
