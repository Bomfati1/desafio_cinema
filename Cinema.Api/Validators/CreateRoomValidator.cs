using Cinema.Api.DTOs;
using FluentValidation;

namespace Cinema.Api.Validators;

public class CreateRoomValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Nome da sala é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Rows)
            .InclusiveBetween(1, 26).WithMessage("Fileiras devem ser entre 1 e 26 (A-Z).");

        RuleFor(x => x.Columns)
            .InclusiveBetween(1, 20).WithMessage("Colunas devem ser entre 1 e 20.");
    }
}
