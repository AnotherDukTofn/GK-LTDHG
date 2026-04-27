@echo off
echo === Creating Environment ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_create -H "Content-Type: application/json" -d "{\"name\":\"Environment\"}"
echo.
echo === Creating Ground ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_create -H "Content-Type: application/json" -d "{\"name\":\"Ground\",\"primitiveType\":\"Plane\",\"parentName\":\"Environment\"}"
echo.
echo === Creating Player ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_create -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"primitiveType\":\"Capsule\",\"y\":1}"
echo.
echo === Creating Managers ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_create -H "Content-Type: application/json" -d "{\"name\":\"Managers\"}"
echo.
echo === Creating UI ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_create -H "Content-Type: application/json" -d "{\"name\":\"UI\"}"
echo.
echo === Creating Camera ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_create -H "Content-Type: application/json" -d "{\"name\":\"PlayerCamera\",\"x\":0,\"y\":10,\"z\":-10}"
echo.
echo === Setting Ground Scale ===
curl.exe -s -X POST http://localhost:8090/skill/gameobject_set_transform -H "Content-Type: application/json" -d "{\"name\":\"Ground\",\"scaleX\":10,\"scaleY\":1,\"scaleZ\":10}"
echo.
echo === Adding Player Components ===
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"CharacterController\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Player.PlayerInputHandler\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Combat.HealthComponent\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Combat.StaminaComponent\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Player.DashController\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Player.StatusEffectController\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Player.PlayerController\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Player\",\"componentType\":\"SpellStrike.Player.PlayerSpellController\"}"
echo.
echo === Adding Manager Components ===
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Managers\",\"componentType\":\"SpellStrike.Core.GameManager\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Managers\",\"componentType\":\"SpellStrike.Combat.LootDropperService\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Managers\",\"componentType\":\"SpellStrike.Combat.Spells.SpellPoolService\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Managers\",\"componentType\":\"SpellStrike.Level.LevelManager\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"Managers\",\"componentType\":\"SpellStrike.Core.SaveManager\"}"
echo.
echo === Adding Camera Component ===
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"PlayerCamera\",\"componentType\":\"Camera\"}"
echo.
curl.exe -s -X POST http://localhost:8090/skill/component_add -H "Content-Type: application/json" -d "{\"name\":\"PlayerCamera\",\"componentType\":\"AudioListener\"}"
echo.
echo === Baking NavMesh ===
curl.exe -s -X POST http://localhost:8090/skill/navmesh_bake -H "Content-Type: application/json" -d "{}"
echo.
echo === Saving Scene ===
curl.exe -s -X POST http://localhost:8090/skill/scene_save -H "Content-Type: application/json" -d "{}"
echo.
echo === DONE ===
