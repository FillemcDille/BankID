namespace BlazorApp4.Domain;
/// <summary>
/// Specifies the type of account for banking or financial operations.
/// </summary>
/// <remarks>Use this enumeration to indicate whether an account is a savings account, a deposit account, or
/// unspecified. The value <see cref="AccountType.None"/> represents an undefined or unknown account type.</remarks>
public enum AccountType
{
    Savings,
    Deposit
}
/// <summary>
/// Enum for currency value
/// </summary>
public enum Currency
{
    SEK,
}