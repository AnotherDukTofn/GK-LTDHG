import requests

def create_so(type_name, name, folder):
    url = "http://localhost:8090/skill/scriptableobject_create"
    data = {
        "typeName": type_name,
        "name": name,
        "folder": folder
    }
    r = requests.post(url, json=data)
    print(f"Created SO {name}: {r.status_code} - {r.text}")

def create_scene(name, folder):
    url = "http://localhost:8090/skill/scene_create"
    data = {
        "name": name,
        "folder": folder
    }
    r = requests.post(url, json=data)
    print(f"Created Scene {name}: {r.status_code} - {r.text}")

create_so("SpellDataSO", "TestFireball", "Assets/_Project/Data/Spells")
create_so("EnemyDataSO", "TestMeleeData", "Assets/_Project/Data/Enemies")
create_so("GameConfigSO", "TestGameConfig", "Assets/_Project/Data")
create_scene("TestPlayground", "Assets/Scenes")
