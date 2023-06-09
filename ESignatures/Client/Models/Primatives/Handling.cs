// ********************************************************
// The use of this source code is licensed under the terms
// of the MIT License (https://opensource.org/licenses/MIT)
// ********************************************************

using FluentValidation;

namespace SquidEyes.ESignatures.Client;

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