---
description: Control Unity Editor via REST API. Create GameObjects, manage scenes, components, materials, and more. 100+ automation tools.
---

# unity-skills

AI-powered Unity Editor automation through REST API. This workflow enables intelligent control of Unity Editor including GameObject manipulation, scene management, asset handling, and much more.

## Available modules

| Module | Description |
|--------|-------------|
| **gameobject** | Create, modify, find GameObjects |
| **component** | Add, remove, configure components |
| **scene** | Scene loading, saving, management |
| **material** | Material creation, HDR emission, keywords |
| **light** | Lighting setup and configuration |
| **animator** | Animation controller management |
| **ui** | UI Canvas and element creation |
| **validation**| Project validation and checking |
| **prefab** | Prefab creation and instantiation |
| **asset** | Asset import, organize, search |
| **editor** | Editor state, play mode, selection |
| **console** | Log capture and debugging |
| **script** | C# script creation and search |
| **shader** | Shader creation and listing |
| **workflow** | Time-machine revert, history tracking, auto-save |

## How to Use

1. **Check Unity Connection**: Ensure Unity Editor is running with the `SkillsForUnity` plugin.
2. **Invoke Skills**: Use `unity_skills.py` (located in the skill's scripts directory) to call Unity functions.

### Example Prompt
`/unity-skills create a red cube at (0, 0, 0)`

## Best Practices

- **Save Progress**: Frequently call `scene_save` during automation.
- **Undo Support**: Operations are usually undoable in Unity.
- **Domain Reload**: Be aware that creating scripts triggers a domain reload.
