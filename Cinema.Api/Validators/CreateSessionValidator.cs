using Cinema.Api.DTOs;
using FluentValidation;

namespace Cinema.Api.Validators;

public class CreateSessionValidator : AbstractValidator<CreateSessionRequest>
{
    public CreateSessionValidator()
    {
        RuleFor(x => x.MovieId)
            .GreaterThan(0).WithMessage("MovieId é obrigatório.");

        RuleFor(x => x.RoomId)
            .GreaterThan(0).WithMessage("RoomId é obrigatório.");

        RuleFor(x => x.StartTime)
            .NotEmpty().WithMessage("Horário de início é obrigatório.");

        RuleFor(x => x.EndTime)
            .NotEmpty().WithMessage("Horário de término é obrigatório.")
            .GreaterThan(x => x.StartTime).WithMessage("Horário de término deve ser após o início.");

        RuleFor(x => x.TicketPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Preço não pode ser negativo.");
    }
}
