using Cinema.Api.DTOs;
using FluentValidation;

namespace Cinema.Api.Validators;

public class CreateMovieValidator : AbstractValidator<CreateMovieRequest>
{
    public CreateMovieValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Título é obrigatório.")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Genre)
            .MaximumLength(100).WithMessage("Gênero deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Descrição deve ter no máximo 2000 caracteres.");

        RuleFor(x => x.DurationMinutes)
            .GreaterThan(0).WithMessage("Duração deve ser maior que zero.");

        RuleFor(x => x.PosterUrl)
            .MaximumLength(500).WithMessage("URL do poster deve ter no máximo 500 caracteres.");
    }
}
