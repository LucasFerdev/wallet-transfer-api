using System.ComponentModel.DataAnnotations;

namespace WalletTransfer.Api.Contracts.Transfers;

public sealed record CreateTransferRequest(
    decimal Value,
    long Payer,
    long Payee)
    : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(
        ValidationContext validationContext)
    {
        if (Value <= 0)
        {
            yield return new ValidationResult(
                "O valor da transferência deve ser maior que zero.",
                [nameof(Value)]);
        }

        if (Payer <= 0)
        {
            yield return new ValidationResult(
                "O identificador do pagador deve ser maior que zero.",
                [nameof(Payer)]);
        }

        if (Payee <= 0)
        {
            yield return new ValidationResult(
                "O identificador do recebedor deve ser maior que zero.",
                [nameof(Payee)]);
        }

        if (Payer == Payee)
        {
            yield return new ValidationResult(
                "O pagador e o recebedor devem ser diferentes.",
                [nameof(Payer), nameof(Payee)]);
        }
    }
}