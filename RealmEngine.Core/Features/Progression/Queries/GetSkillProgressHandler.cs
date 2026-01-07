using RealmEngine.Core.Features.Progression.Services;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Queries;

/// <summary>
/// Handles getting skill progress information.
/// </summary>
public class GetSkillProgressHandler : IRequestHandler<GetSkillProgressQuery, GetSkillProgressResult>
{
    private readonly SkillProgressionService _progressionService;

    public GetSkillProgressHandler(SkillProgressionService progressionService)
    {
        _progressionService = progressionService ?? throw new ArgumentNullException(nameof(progressionService));
    }

    public Task<GetSkillProgressResult> Handle(GetSkillProgressQuery request, CancellationToken cancellationToken)
    {
        var progress = _progressionService.GetSkillProgress(request.Character, request.SkillId);

        return Task.FromResult(new GetSkillProgressResult
        {
            SkillId = progress.SkillId,
            Name = progress.Name,
            Category = progress.Category,
            CurrentRank = progress.CurrentRank,
            CurrentXP = progress.CurrentXP,
            XPToNextRank = progress.XPToNextRank,
            ProgressPercent = progress.ProgressPercent,
            CurrentEffect = progress.CurrentEffect,
            NextRankEffect = progress.NextRankEffect
        });
    }
}
