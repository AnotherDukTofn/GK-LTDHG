function CreateSO {
    param ($typeName, $name, $folder)
    $body = "{`"typeName`":`"$typeName`", `"name`":`"$name`", `"folder`":`"$folder`"}"
    try {
        $res = Invoke-RestMethod -Uri http://localhost:8090/skill/scriptableobject_create -Method Post -ContentType "application/json" -Body $body
        Write-Host "Created SO $name : $($res | ConvertTo-Json -Compress)"
    } catch {
        Write-Host "Error for $name : $_"
    }
}

function CreateScene {
    param ($name, $folder)
    $body = "{`"name`":`"$name`", `"folder`":`"$folder`"}"
    try {
        $res = Invoke-RestMethod -Uri http://localhost:8090/skill/scene_create -Method Post -ContentType "application/json" -Body $body
        Write-Host "Created Scene $name : $($res | ConvertTo-Json -Compress)"
    } catch {
        Write-Host "Error for Scene $name : $_"
    }
}

CreateSO -typeName "SpellDataSO" -name "TestFireball" -folder "Assets/_Project/Data/Spells"
CreateSO -typeName "EnemyDataSO" -name "TestMeleeData" -folder "Assets/_Project/Data/Enemies"
CreateSO -typeName "GameConfigSO" -name "TestGameConfig" -folder "Assets/_Project/Data"
CreateScene -name "TestPlayground" -folder "Assets/Scenes"
