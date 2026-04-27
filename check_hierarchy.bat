@echo off
curl.exe -s -X POST http://localhost:8090/skill/scene_get_hierarchy -H "Content-Type: application/json" -d "{}"
