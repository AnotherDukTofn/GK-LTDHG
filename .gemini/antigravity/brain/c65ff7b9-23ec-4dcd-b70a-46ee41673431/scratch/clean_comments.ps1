$rootDir = "c:\Users\ducto\Code\GK-LTDHG\Assets\_Project\Scripts"
$files = Get-ChildItem -Path $rootDir -Filter "*.cs" -Recurse

$importantKeywords = @("TODO", "FIXME", "BUG", "HACK", "NOTE", "CAUTION", "IMPORTANT", "WARNING", "REFERENCE")
$count = 0

foreach ($file in $files) {
    $lines = Get-Content $file.FullName
    $newLines = New-Object System.Collections.Generic.List[string]
    $changed = $false

    foreach ($line in $lines) {
        $stripped = $line.Trim()
        
        # Keep XML documentation
        if ($stripped.StartsWith("///")) {
            $newLines.Add($line)
            continue
        }
        
        # Keep regions
        if ($stripped.StartsWith("#region") -or $stripped.StartsWith("#endregion")) {
            $newLines.Add($line)
            continue
        }
        
        # Check for standalone comments
        if ($stripped.StartsWith("//")) {
            $content = $stripped.Substring(2).Trim()
            $isImportant = $false
            foreach ($kw in $importantKeywords) {
                if ($content.ToUpper().Contains($kw)) {
                    $isImportant = $true
                    break
                }
            }
            
            if ($isImportant -or $content.Length -gt 60 -or $content.StartsWith("---") -or $content.StartsWith("===")) {
                $newLines.Add($line)
            } else {
                $changed = $true
            }
            continue
        }
        
        # Check for inline comments
        if ($line.Contains("//")) {
            $index = $line.IndexOf("//")
            $codePart = $line.Substring(0, $index)
            $commentPart = $line.Substring($index + 2).Trim()
            
            $isImportant = $false
            foreach ($kw in $importantKeywords) {
                if ($commentPart.ToUpper().Contains($kw)) {
                    $isImportant = $true
                    break
                }
            }
            
            if ($isImportant -or $commentPart.Length -gt 60) {
                $newLines.Add($line)
            } else {
                if (![string]::IsNullOrWhiteSpace($codePart)) {
                    $newLines.Add($codePart.TrimEnd())
                    $changed = $true
                } else {
                    $changed = $true
                }
            }
            continue
        }

        # Keep non-comment lines
        $newLines.Add($line)
    }

    if ($changed) {
        $newLines | Out-File -FilePath $file.FullName -Encoding utf8
        Write-Host "Cleaned: $($file.FullName)"
        $count++
    }
}

Write-Host "Finished. Total files modified: $count"
