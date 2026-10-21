namespace KiraathaneTactics.Application.Features.Matches.Common;

public static class RoomCodeFactory
{
    private const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

    public static string Create()
    {
        Span<char> code = stackalloc char[6];
        for (var i = 0; i < code.Length; i++)
            code[i] = Alphabet[Random.Shared.Next(Alphabet.Length)];

        return new string(code);
    }

    public static string SeatToken() => Guid.NewGuid().ToString("N");
}
