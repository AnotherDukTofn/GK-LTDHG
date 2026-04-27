$j = Get-Content -Path "hierarchy_result.txt" -Raw | ConvertFrom-Json
foreach ($h in $j.result.hierarchy) {
    Write-Host $h.name
    if ($h.children) {
        foreach ($c in $h.children) {
            Write-Host "  - $($c.name)"
        }
    }
}
