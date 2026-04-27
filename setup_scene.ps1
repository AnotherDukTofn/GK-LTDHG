function RunBatch {
    param ($url, $jsonFile)
    $body = Get-Content -Raw $jsonFile
    try {
        $res = Invoke-RestMethod -Uri $url -Method Post -ContentType "application/json" -Body $body
        Write-Host "Success for $jsonFile"
    } catch {
        Write-Host "Error for $jsonFile: $_"
    }
}

RunBatch "http://localhost:8090/skill/gameobject_create_batch" "create_gameobjects.json"
RunBatch "http://localhost:8090/skill/gameobject_create_batch" "camera.json"
RunBatch "http://localhost:8090/skill/component_add_batch" "add_player_components.json"
RunBatch "http://localhost:8090/skill/component_add_batch" "add_manager_components.json"
RunBatch "http://localhost:8090/skill/navmesh_bake" "empty.json"
