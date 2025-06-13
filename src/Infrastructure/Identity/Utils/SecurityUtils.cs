namespace Infrastructure.Identity.Utils;

public static class SecurityUtils
{
    public static string GenerateSecurePassword(int length = 16)
    {
        const string uppercase = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lowercase = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string symbols = "!@#$%^&*()";
        var all = uppercase + lowercase + digits + symbols;

        var random = new Random();
        var password = new char[length];

        password[0] = uppercase[random.Next(uppercase.Length)];
        password[1] = lowercase[random.Next(lowercase.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = symbols[random.Next(symbols.Length)];

        for (int i = 4; i < length; i++)
            password[i] = all[random.Next(all.Length)];

        return new string(password.OrderBy(_ => random.Next()).ToArray());
    }
}

