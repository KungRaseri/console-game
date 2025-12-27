# Script to fix notes formatting in all JSON files:
# 1. Remove root-level "notes" fields
# 2. Convert object-type "notes" to string arrays

$jsonPath = "Game.Data\Data\Json"
$filesProcessed = 0
$filesModified = 0

Get-ChildItem -Path $jsonPath -Filter "*.json" -Recurse | ForEach-Object {
    $file = $_
    $filesProcessed++
    $modified = $false
    
    try {
        $content = Get-Content $file.FullName -Raw
        $json = $content | ConvertFrom-Json -AsHashtable
        
        # Check if there's a root-level "notes" field
        if ($json.ContainsKey("notes")) {
            $rootNotes = $json["notes"]
            
            # If notes is a string (not array, not object)
            if ($rootNotes -is [string]) {
                Write-Host "Removing root-level string notes from: $($file.Name)" -ForegroundColor Yellow
                $json.Remove("notes")
                $modified = $true
            }
            # If notes is a hashtable/object
            elseif ($rootNotes -is [hashtable] -or $rootNotes -is [System.Management.Automation.PSCustomObject]) {
                Write-Host "Removing root-level object notes from: $($file.Name)" -ForegroundColor Yellow
                $json.Remove("notes")
                $modified = $true
            }
        }
        
        # Recursively fix nested notes objects
        function Fix-NestedNotes {
            param($obj)
            
            if ($obj -is [hashtable]) {
                $keysToFix = @()
                foreach ($key in $obj.Keys) {
                    if ($key -eq "notes" -and ($obj[$key] -is [hashtable] -or $obj[$key] -is [System.Management.Automation.PSCustomObject])) {
                        $keysToFix += $key
                    }
                    elseif ($obj[$key] -is [hashtable] -or $obj[$key] -is [array]) {
                        Fix-NestedNotes -obj $obj[$key]
                    }
                }
                
                foreach ($key in $keysToFix) {
                    $noteObj = $obj[$key]
                    # Convert object to array of strings
                    $noteArray = @()
                    foreach ($propKey in $noteObj.Keys) {
                        $noteArray += $noteObj[$propKey]
                    }
                    $obj[$key] = $noteArray
                    Write-Host "  Converted nested notes object to array in: $($file.Name)" -ForegroundColor Cyan
                    $script:modified = $true
                }
            }
            elseif ($obj -is [array]) {
                foreach ($item in $obj) {
                    if ($item -is [hashtable] -or $item -is [array]) {
                        Fix-NestedNotes -obj $item
                    }
                }
            }
        }
        
        Fix-NestedNotes -obj $json
        
        if ($modified) {
            $filesModified++
            # Convert back to JSON with proper formatting
            $newContent = $json | ConvertTo-Json -Depth 100
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
            Write-Host "  ✓ Modified: $($file.FullName)" -ForegroundColor Green
        }
    }
    catch {
        Write-Host "ERROR processing $($file.Name): $_" -ForegroundColor Red
    }
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Files processed: $filesProcessed" -ForegroundColor White
Write-Host "Files modified: $filesModified" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
