// ApexFood.Application/Features/Insumos/CriarInsumoCommandValidator.cs
using FluentValidation;
namespace ApexFood.Application.Features.Insumos;

public class CriarInsumoCommandValidator : AbstractValidator<CriarInsumoCommand>
{
    public CriarInsumoCommandValidator()
    {
        RuleFor(x => x.Nome).NotEmpty().MaximumLength(100);
        RuleFor(x => x.UnidadeMedidaBase).NotEmpty().MaximumLength(10);
    }
}