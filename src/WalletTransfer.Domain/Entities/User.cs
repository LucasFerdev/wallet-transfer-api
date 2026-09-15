using System.Net.Mail;

using WalletTransfer.Domain.Enums;
using WalletTransfer.Domain.Exceptions;

namespace WalletTransfer.Domain.Entities;

public sealed class User
{
    public long Id { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public string Document { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserType Type { get; private set; }
    public Wallet Wallet { get; private set; } = null!;

    private User()
    {
    }

    public User(
        string fullName,
        string document,
        string email,
        string passwordHash,
        UserType type,
        decimal initialBalance = 0)
    {
        FullName = ValidateFullName(fullName);
        Document = ValidateAndNormalizeDocument(document, type);
        Email = ValidateAndNormalizeEmail(email);
        PasswordHash = ValidatePasswordHash(passwordHash);
        Type = ValidateUserType(type);
        Wallet = new Wallet(initialBalance);
    }

    public bool CanTransfer => Type == UserType.Common;

    public void EnsureCanTransfer()
    {
        if (!CanTransfer)
        {
            throw new DomainException(
                "Lojistas não podem realizar transferências.");
        }
    }

    private static string ValidateFullName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new DomainException(
                "O nome completo é obrigatório.");
        }

        var normalizedName = fullName.Trim();

        if (normalizedName.Length < 3)
        {
            throw new DomainException(
                "O nome completo deve possuir pelo menos 3 caracteres.");
        }

        return normalizedName;
    }

    private static string ValidateAndNormalizeDocument(
        string document,
        UserType type)
    {
        if (string.IsNullOrWhiteSpace(document))
        {
            throw new DomainException(
                "O CPF ou CNPJ é obrigatório.");
        }

        var normalizedDocument = new string(
            document.Where(char.IsDigit).ToArray());

        var expectedLength = type switch
        {
            UserType.Common => 11,
            UserType.Merchant => 14,
            _ => throw new DomainException(
                "O tipo de usuário é inválido.")
        };

        if (normalizedDocument.Length != expectedLength)
        {
            var documentName = type == UserType.Common
                ? "CPF"
                : "CNPJ";

            throw new DomainException(
                $"O {documentName} deve possuir {expectedLength} dígitos.");
        }

        return normalizedDocument;
    }

    private static string ValidateAndNormalizeEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException(
                "O e-mail é obrigatório.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();

        if (!MailAddress.TryCreate(normalizedEmail, out _))
        {
            throw new DomainException(
                "O endereço de e-mail é inválido.");
        }

        return normalizedEmail;
    }

    private static string ValidatePasswordHash(string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
        {
            throw new DomainException(
                "A senha protegida é obrigatória.");
        }

        return passwordHash;
    }

    private static UserType ValidateUserType(UserType type)
    {
        if (!Enum.IsDefined(type))
        {
            throw new DomainException(
                "O tipo de usuário é inválido.");
        }

        return type;
    }
}