using KiraathaneTactics.Application.Common.Interfaces;
using KiraathaneTactics.Application.Common.Messaging;
using KiraathaneTactics.Application.Common.Results;
using KiraathaneTactics.Domain.Entities;

namespace KiraathaneTactics.Application.Features.Matches.Create;

public sealed class CreateMatchCommandHandler(
    IMatchRepository matchRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateMatchCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateMatchCommand request, CancellationToken cancellationToken)
    {
        var match = new Match();
        await matchRepository.AddAsync(match, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return match.Id;
    }
}
