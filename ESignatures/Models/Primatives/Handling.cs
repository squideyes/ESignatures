using FluentValidation;

namespace ESignatures;

public class Handling
{
    public class Validator : AbstractValidator<Handling>
    {
        public Validator()
        {
            RuleFor(x => x.Ordinal)
                .GreaterThanOrEqualTo(0);

            RuleFor(x => x.GetDocBy)
                .IsInEnum();

            RuleFor(x => x.SigReqBy)
                .IsInEnum();
        }
    }

    public required int Ordinal { get; set; } = 0;
    public required Mode GetDocBy { get; set; } = Mode.Email;
    public required Mode SigReqBy { get; set; } = Mode.Email;
    public required bool IdBySms { get; set; } = true;
    public required bool IdByEmail { get; set; } = true;
}