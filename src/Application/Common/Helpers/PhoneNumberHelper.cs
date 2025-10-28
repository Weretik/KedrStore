namespace Application.Common.Helpers;

public static class PhoneNumberHelper
{
    private static readonly PhoneNumberUtil _phone = PhoneNumberUtil.GetInstance();

    public static bool TryParse(string raw, out string? normalized, string baseRegion = "UA")
    {
        normalized = null;
        if (string.IsNullOrWhiteSpace(raw)) return false;

        try
        {
            var parsed = _phone.Parse(raw, baseRegion);
            if (!_phone.IsValidNumber(parsed)) return false;

            normalized = _phone.Format(parsed, PhoneNumberFormat.E164);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
