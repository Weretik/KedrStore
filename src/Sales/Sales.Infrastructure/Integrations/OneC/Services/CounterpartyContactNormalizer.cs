using System.Net.Mail;
using PhoneNumbers;

namespace Sales.Infrastructure.Integrations.OneC.Services;

public sealed class CounterpartyContactNormalizer
{
    private const string DefaultPhoneRegion = "UA";
    private static readonly PhoneNumberUtil PhoneNumberUtil = PhoneNumberUtil.GetInstance();

    public bool TryNormalizeEmail(string? rawEmail, out string? normalizedEmail)
    {
        normalizedEmail = null;
        if (string.IsNullOrWhiteSpace(rawEmail))
            return false;

        try
        {
            var mailAddress = new MailAddress(rawEmail.Trim());
            var normalized = mailAddress.Address.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalized))
                return false;

            normalizedEmail = normalized;
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    public bool TryNormalizePhone(string? rawPhone, out string? normalizedPhone)
    {
        normalizedPhone = null;
        if (string.IsNullOrWhiteSpace(rawPhone))
            return false;

        try
        {
            var parsed = PhoneNumberUtil.Parse(rawPhone.Trim(), DefaultPhoneRegion);
            if (!PhoneNumberUtil.IsValidNumber(parsed))
                return false;

            normalizedPhone = PhoneNumberUtil.Format(parsed, PhoneNumberFormat.E164);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
