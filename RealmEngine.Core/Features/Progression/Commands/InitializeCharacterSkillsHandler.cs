using RealmEngine.Core.Features.Progression.Services;
using MediatR;

namespace RealmEngine.Core.Features.Progression.Commands;

/// <summary>
/// Handles initializing all skills for a new character.
/// </summary>
public class InitializeCharacterSkillsHandler : IRequestHandler<InitializeCharacterSkillsCommand, InitializeCharacterSkillsResult>
{
    private readonly SkillProgressionService _progressionService;

    public InitializeCharacterSkillsHandler(SkillProgressionService progressionService)
    {
        _progressionService = progressionService ?? throw new ArgumentNullException(nameof(progressionService));
    }

    public Task<InitializeCharacterSkillsResult> Handle(InitializeCharacterSkillsCommand request, CancellationToken cancellationToken)
    {
        _progressionService.InitializeAllSkills(request.Character);

        return Task.FromResult(new InitializeCharacterSkillsResult
        {
            SkillsInitialized = request.Character.Skills.Count,
            SkillIds = request.Character.Skills.Keys.ToList()
        });
    }
}
