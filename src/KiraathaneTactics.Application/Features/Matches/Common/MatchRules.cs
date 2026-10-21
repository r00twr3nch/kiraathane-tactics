using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Domain.Exceptions;

namespace KiraathaneTactics.Application.Features.Matches.Common;

internal static class MatchRules
{
    public static Result<int> Guard(Action action) => Guard(() =>
    {
        action();
        return 0;
    });

    public static Result<T> Guard<T>(Func<T> action)
    {
        try
        {
            return action();
        }
        catch (DomainException exception)
        {
            return Error.Conflict(exception.Code, exception.Message);
        }
    }
}
