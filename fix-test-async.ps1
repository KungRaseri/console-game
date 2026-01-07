# Fix CombatServiceTests to properly await Task<CombatResult> and Task<CombatOutcome>
$file = "RealmEngine.Core.Tests\Features\Combat\CombatServiceTests.cs"
$content = Get-Content $file -Raw

# Fix result.Property accesses after await
$content = $content -replace '(\s+)(var \w+ = await service\.Execute\w+Attack[^;]+;)\r?\n(\s+)(result\.)', '$1$2$3var result = $4'
$content = $content -replace '(\s+)(var \w+ = await service\.Execute\w+\([^;]+\);)\r?\n(\s+)(outcome\.)', '$1$2$3var outcome = $4'

# Fix accessing properties on Task without await - pattern: result.Property where result is from ExecuteEnemyAttack
$content = $content -replace '(\s+var \w+ = await service\.ExecuteEnemyAttack[^;]+;)', '$1'
$content = $content -replace '(\s+var \w+ = await service\.ExecuteCombat[^;]+;)', '$1'

Set-Content $file $content

Write-Host "Fixed $file"

# Fix AttackEnemyHandlerTests - change Returns to ReturnsAsync that actually work
$file2 = "RealmEngine.Core.Tests\Features\Combat\Commands\AttackEnemyHandlerTests.cs"
$content2 = Get-Content $file2 -Raw

# Replace .Returns(combatResult) with .Returns(Task.FromResult(combatResult))
$content2 = $content2 -replace '\.ReturnsAsync\((combatResult)\)', '.Returns(Task.FromResult($1))'

Set-Content $file2 $content2

Write-Host "Fixed $file2"
Write-Host "Done!"
