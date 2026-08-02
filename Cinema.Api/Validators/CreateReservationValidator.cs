using Cinema.Api.DTOs;
using FluentValidation;

namespace Cinema.Api.Validators;

public class CreateReservationValidator : AbstractValidator<CreateReservationRequest>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.SessionId)
            .GreaterThan(0).WithMessage("SessionId é obrigatório.");

        RuleFor(x => x.CustomerName)
            .NotEmpty().WithMessage("Nome do cliente é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.SeatIds)
            .NotEmpty().WithMessage("Selecione pelo menos um assento.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Há assentos duplicados na seleção.");
    }
}
