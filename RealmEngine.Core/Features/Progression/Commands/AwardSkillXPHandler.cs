using RealmEngine.Shared.Models;
using RealmEngine.Core.Features.Progression.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Handles awarding skill XP and processing rank-ups.
/// </summary>
public class AwardSkillXPHandler : IRequestHandler<AwardSkillXPCommand, AwardSkillXPResult>
{
    private readonly SkillProgressionService _progressionService;
    private readonly ILogger<AwardSkillXPHandler> _logger;

    public AwardSkillXPHandler(
        SkillProgressionService progressionService,
        ILogger<AwardSkillXPHandler> logger)
    {
        _progressionService = progressionService ?? throw new ArgumentNullException(nameof(progressionService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Task<AwardSkillXPResult> Handle(AwardSkillXPCommand request, CancellationToken cancellationToken)
    {
        var result = _progressionService.AwardSkillXP(
            request.Character,
            request.SkillId,
            request.XPAmount,
            request.ActionSource);

        return Task.FromResult(new AwardSkillXPResult
        {
            SkillId = result.SkillId,
            NewRank = result.NewRank,
            RanksGained = result.RanksGained,
            Notifications = result.Notifications
        });
    }
}
