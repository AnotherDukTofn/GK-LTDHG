@echo off
curl -s -X POST http://localhost:8090/skill/gameobject_create_batch -H "Content-Type: application/json" -d @create_gameobjects.json
echo.
curl -s -X POST http://localhost:8090/skill/gameobject_create_batch -H "Content-Type: application/json" -d @camera.json
echo.
curl -s -X POST http://localhost:8090/skill/component_add_batch -H "Content-Type: application/json" -d @add_player_components.json
echo.
curl -s -X POST http://localhost:8090/skill/component_add_batch -H "Content-Type: application/json" -d @add_manager_components.json
echo.
curl -s -X POST http://localhost:8090/skill/component_add_batch -H "Content-Type: application/json" -d @add_camera.json
echo.
curl -s -X POST http://localhost:8090/skill/navmesh_bake -H "Content-Type: application/json" -d @empty.json
echo.
