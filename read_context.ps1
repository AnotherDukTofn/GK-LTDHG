$json = Get-Content -Path scene_context.json -Raw | ConvertFrom-Json
if ($json.status -eq 'success') {
    foreach ($obj in $json.results.sceneHierarchy) {
        Write-Host "Object: $($obj.path)"
        if ($obj.components.Length -gt 0) {
            Write-Host "  Components: $($obj.components -join ', ')"
        }
    }
}
else {
    Write-Host "Error status: $($json.error)"
}
