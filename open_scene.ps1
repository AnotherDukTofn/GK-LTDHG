$body = "{`"path`":`"Assets/Scenes/TestPlayground.unity`"}"
try {
    $res = Invoke-RestMethod -Uri http://localhost:8090/skill/scene_open -Method Post -ContentType "application/json" -Body $body
    Write-Host "Opened Scene : $($res | ConvertTo-Json -Compress)"
} catch {
    Write-Host "Error : $_"
}
