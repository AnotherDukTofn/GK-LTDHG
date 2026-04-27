# ISSUES — Rà soát TDD (02 - TDD.md)

> **Ngày rà soát:** 2026-04-25
> **Tham chiếu:** [02 - TDD.md](./02%20-%20TDD.md) ↔ [01 - GDD.md](./01%20-%20GDD.md)

---

## Mức độ: 🔴 Nghiêm trọng — Mâu thuẫn nội bộ / Sai logic

### ISSUE-001: Ràng buộc "Không dùng Update() cho timer" mâu thuẫn với SpellBase

- **Vị trí:** TDD §1.3 vs §11.1
- **Mô tả:** §1.3 quy định *"Không dùng `Update()` cho timer/cooldown — dùng Coroutine hoặc custom timer"*, nhưng §11.1 `SpellBase` ghi rõ *"Update cooldown trong Update() — giảm m_CurrentCooldown theo deltaTime"*.
- **Hậu quả:** Developer không biết phải tuân theo quy tắc nào. Nếu tuân theo ràng buộc → phải đổi sang Coroutine/Timer cho cooldown; nếu tuân theo SpellBase → vi phạm ràng buộc.
- **Đề xuất:** Chọn một trong hai: (A) Bỏ ràng buộc, cho phép Update() cho cooldown đơn giản. (B) Đổi SpellBase sang dùng custom `Timer` utility.

---

### ISSUE-002: OnPlayerHPChanged là IntEventChannelSO nhưng cần 2 giá trị (current, max)

- **Vị trí:** TDD §2.3 bảng Event Channel vs §17.1 HealthComponent
- **Mô tả:** Event channel `OnPlayerHPChanged` được khai báo kiểu `IntEventChannelSO` (1 tham số int). Nhưng `HealthComponent.OnHPChanged` là `System.Action<int, int>` truyền cả `(current, max)`. HP Bar UI cần cả 2 giá trị để hiển thị "80/100".
- **Hậu quả:** UI không nhận đủ dữ liệu, hoặc phải hardcode maxHP riêng.
- **Đề xuất:** Tạo `HPChangedEventChannelSO` kiểu custom với struct `{int current, int max}`, hoặc dùng 2 event riêng.

---

### ISSUE-003: HealthComponent dùng System.Action nhưng kiến trúc yêu cầu SO Event Channel

- **Vị trí:** TDD §2.2 vs §17.1
- **Mô tả:** Kiến trúc quy định giao tiếp cross-system qua **SO Event Channel** (§2.2). Tuy nhiên `HealthComponent` (§17.1) sử dụng `System.Action OnDeath` và `System.Action<int, int> OnHPChanged` — đây là C# delegate trực tiếp, không phải SO Event Channel.
- **Hậu quả:** Tight coupling giữa HealthComponent và subscriber. Không đúng pattern đã đề ra.
- **Đề xuất:** HealthComponent nên nhận `[SerializeField]` các SO Event Channel và raise event qua đó, hoặc chấp nhận hybrid approach nhưng phải ghi rõ exception.

---

### ISSUE-004: CharacterController + Rigidbody trên cùng GameObject

- **Vị trí:** TDD §10.1
- **Mô tả:** Player GameObject liệt kê cả `CharacterController` và `Rigidbody (Kinematic)`. Unity khuyến cáo **không nên** dùng cả hai trên cùng một GameObject — CharacterController có capsule collision riêng, Rigidbody sẽ xung đột.
- **Hậu quả:** Physics behavior không dự đoán được, trigger detection có thể không hoạt động đúng.
- **Đề xuất:** Chọn một: (A) Dùng CharacterController cho movement + child GameObject chứa Collider+Rigidbody cho trigger. (B) Dùng Rigidbody kinematic cho cả movement và trigger.

---

### ISSUE-005: Blizzard "disable physics collider" nhưng CharacterController vẫn có collision

- **Vị trí:** TDD §11.2 (BlizzardSpell)
- **Mô tả:** BlizzardSpell ghi *"Disable physics collider (xuyên enemy)"*, nhưng `CharacterController` có **built-in capsule collider** không thể disable riêng. Disable `CapsuleCollider` component sẽ không ảnh hưởng đến CharacterController collision.
- **Hậu quả:** Player không thể xuyên qua enemy như GDD yêu cầu.
- **Đề xuất:** Dùng Physics Layer để ignore collision giữa Player và Enemy layer trong khi Blizzard active, hoặc chuyển sang Rigidbody-based movement.

---

## Mức độ: 🟠 Quan trọng — Thiếu thiết kế / SO chưa định nghĩa

### ISSUE-006: SpawnerDataSO được dùng nhưng chưa bao giờ định nghĩa

- **Vị trí:** TDD §13 dùng `SpawnerDataSO` nhưng §6 không có, §25 không liệt kê
- **Mô tả:** `SpawnerController` reference `[SerializeField] private SpawnerDataSO m_Data` nhưng class `SpawnerDataSO` chưa được thiết kế. Các field cần: hp, spawnInterval, spawnCountMin, spawnCountMax, enemyPool, dropTable, spawnRadius.
- **Đề xuất:** Bổ sung §6 với `SpawnerDataSO` ScriptableObject và thêm vào phụ lục §25.

---

### ISSUE-007: PowerUpDataSO xuất hiện trong kiến trúc nhưng không được định nghĩa

- **Vị trí:** TDD §2.1 diagram (dòng 100) liệt kê `PowerUpDataSO`, §3 có thư mục `Data/PowerUps/`
- **Mô tả:** Sơ đồ kiến trúc và cấu trúc thư mục đều đề cập `PowerUpDataSO`, nhưng chưa bao giờ thiết kế class này. Power-up hiện chỉ là enum + switch trong `PowerUpDropItem`.
- **Đề xuất:** Tạo `PowerUpDataSO` với fields: type, icon, displayName, description, effectValue, effectDuration. Hoặc xóa tham chiếu nếu quyết định dùng enum đơn giản.

---

### ISSUE-008: DamageDealer.cs liệt kê trong thư mục nhưng không thiết kế

- **Vị trí:** TDD §3 (dòng 198)
- **Mô tả:** File `DamageDealer.cs` có trong folder structure nhưng không có section nào mô tả class này. Damage flow (§17.2) chỉ dùng `GetComponent<HealthComponent>().TakeDamage()` trực tiếp.
- **Đề xuất:** Thiết kế `DamageDealer` component (gắn lên projectile/spell/enemy) chứa damage amount, scaling, source tag; hoặc xóa khỏi folder structure.

---

### ISSUE-009: PowerUpEffect.cs liệt kê trong thư mục nhưng không thiết kế

- **Vị trí:** TDD §3 (dòng 201)
- **Mô tả:** `PowerUpEffect.cs` có trong `Scripts/PowerUp/` nhưng không có mô tả. Logic power-up nằm trong `PowerUpDropItem` (§16).
- **Đề xuất:** Quyết định rõ: power-up logic nằm ở `PowerUpDropItem` hay tách ra `PowerUpEffect`. Cập nhật folder structure cho khớp.

---

### ISSUE-010: LootBoxDataSO không được tạo chính thức

- **Vị trí:** TDD §15.3
- **Mô tả:** `LootBox` có comment "hoặc LootBoxDataSO" nhưng không tạo SO. HP hiện là field trực tiếp trên MonoBehaviour, không tuân thủ nguyên tắc data-driven (§1.3).
- **Đề xuất:** Tạo `LootBoxDataSO` hoặc merge chỉ số vào `DropTableSO`.

---

### ISSUE-011: AudioLibrarySO — Dictionary<string, AudioClip> không serializable trong Unity

- **Vị trí:** TDD §22
- **Mô tả:** TDD nói tạo `AudioLibrarySO` chứa *"Dictionary map string → AudioClip"*, nhưng Unity `[SerializeField]` **không hỗ trợ** serialize `Dictionary`. Cần giải pháp thay thế.
- **Đề xuất:** Dùng `List<AudioEntry>` với `[System.Serializable] struct AudioEntry { string id; AudioClip clip; }` + runtime lookup Dictionary, hoặc dùng package `SerializableDictionary`.

---

### ISSUE-012: Damage number / floating text được đề cập nhưng không thiết kế

- **Vị trí:** TDD §1.2 đề cập TextMeshPro cho *"damage number"*
- **Mô tả:** Package list nói TextMeshPro dùng cho "damage number" nhưng không có system nào thiết kế floating damage text. GDD cũng không yêu cầu.
- **Đề xuất:** Xóa "damage number" khỏi §1.2 nếu không cần, hoặc thêm `DamageNumberUI` system nếu muốn.

---

## Mức độ: 🟡 Cần làm rõ — Không khớp GDD hoặc thiếu chi tiết

### ISSUE-013: SpellBehavior enum khác với GDD §16.3

- **Vị trí:** TDD §6.1 vs GDD §16.3
- **Mô tả:** GDD §16.3 liệt kê behaviors: `Projectile | Lob | Beam | MeleeBurst | Homing`. TDD §6.1 định nghĩa: `AreaLob | PiercingProjectile | ConeSpray | PassiveAura | Transform`. Hai bộ enum hoàn toàn khác nhau.
- **Hậu quả:** Elite Boss có thể cần dùng behaviors từ GDD (Projectile, Homing, Beam, MeleeBurst) mà TDD enum không hỗ trợ.
- **Đề xuất:** Merge cả hai: bổ sung `Projectile, Beam, MeleeBurst, Homing` vào enum cho Elite Boss spells, hoặc tách `PlayerSpellBehavior` và `EliteSpellBehavior`.

---

### ISSUE-014: Elite Boss spell dùng chung SpellDataSO nhưng config riêng

- **Vị trí:** TDD §6.3 `EliteSpellEntry.SpellData` là `SpellDataSO`
- **Mô tả:** GDD §6.3 nói Elite spells *"được config riêng về: damage, cooldown, tầm, tốc độ projectile"*. Nhưng TDD dùng chung `SpellDataSO` — nghĩa là Elite spell sẽ có cùng damage/cooldown với player spell.
- **Đề xuất:** `EliteSpellEntry` cần có override fields (damage, cooldown, range, projectileSpeed) hoặc tạo `EliteSpellDataSO` riêng.

---

### ISSUE-015: EnemyDataSO dùng float đơn cho PreferredDistance nhưng GDD dùng range

- **Vị trí:** TDD §6.2 vs GDD §5.2
- **Mô tả:** GDD: `ranged_preferred_dist` = **6–10m** (một khoảng). TDD: `m_PreferredDistance` là **single float**. Ranged enemy cần min/max preferred distance để có vùng hoạt động.
- **Đề xuất:** Tách thành `m_PreferredDistanceMin` và `m_PreferredDistanceMax`.

---

### ISSUE-016: GameConfigSO dùng public fields — vi phạm coding convention

- **Vị trí:** TDD §6.5 vs §4.3
- **Mô tả:** Coding convention (§4.3) yêu cầu dùng `[SerializeField] private`. Nhưng `GameConfigSO` (§6.5) dùng `public` fields trực tiếp (`public int PlayerMaxHP = 100`).
- **Đề xuất:** Đổi sang `[SerializeField] private` + public properties readonly, hoặc chấp nhận exception cho config SO nhưng ghi rõ.

---

### ISSUE-017: LootDropper và SpellPool là static class — khó test và inject dependency

- **Vị trí:** TDD §11.4, §15.1
- **Mô tả:** Cả `LootDropper` và `SpellPool` đều là `static class`. Nhưng `LootDropper.SpawnDrop()` cần `LevelDataSO`, `SaveManager`, `ObjectPool` — đây là dependencies mà static class không thể inject qua Inspector.
- **Hậu quả:** Khó unit test, phải truyền dependency qua parameter (service locator pattern ẩn).
- **Đề xuất:** Đổi sang MonoBehaviour Singleton hoặc non-static service class inject qua Inspector.

---

### ISSUE-018: LevelManager tìm Spawner "trong scene lúc Awake" — có thể dùng FindObjectsOfType

- **Vị trí:** TDD §19 vs §1.3
- **Mô tả:** §19 nói `m_Spawners` được *"Tìm trong scene lúc Awake"* — ngụ ý dùng `FindObjectsOfType<SpawnerController>()`. Ràng buộc §1.3 cấm `FindObjectOfType()` trong runtime.
- **Đề xuất:** Inject qua Inspector (drag-drop trong scene) hoặc dùng Spawner tự đăng ký vào LevelManager qua `OnEnable`.

---

### ISSUE-019: Thiếu spawn radius config cho Spawner

- **Vị trí:** TDD §13 vs GDD §5.1
- **Mô tả:** GDD nói Spawner *"sinh ra kẻ địch trong bán kính xung quanh"*. TDD không định nghĩa `spawn_radius` config trong SpawnerDataSO (chưa tồn tại — xem ISSUE-006).
- **Đề xuất:** Thêm `spawnRadius` field vào SpawnerDataSO.

---

### ISSUE-020: Elite Boss spell cooldown tracking chưa được thiết kế

- **Vị trí:** TDD §14
- **Mô tả:** Elite Boss có nhiều spell với cooldown riêng. `EliteSpellEntry` chỉ chứa data (SpellData, CastPriority, Range), không có runtime cooldown tracking. `EvaluateState` check `spell.IsReady` nhưng spell nào giữ cooldown state?
- **Đề xuất:** Thiết kế runtime wrapper: `EliteSpellRuntime { EliteSpellEntry entry; float currentCooldown; bool IsReady; }` hoặc mỗi Elite instantiate SpellBase con.

---

### ISSUE-021: Thiếu Dash duration config

- **Vị trí:** TDD §10.3
- **Mô tả:** DashController ghi *"trong ~0.15s"* nhưng 0.15s là `dash_iframe_duration` (thời gian bất tử). Thời gian dash thực tế (animation/movement duration) chưa có config key.
- **Đề xuất:** Thêm `dash_duration` vào `GameConfigSO`, tách biệt với `dash_iframe_duration`.

---

### ISSUE-022: SpellSelection là scene riêng — có thể không cần thiết

- **Vị trí:** TDD §3 (Scenes/)
- **Mô tả:** SpellSelection được tạo thành scene riêng (`SpellSelection.unity`). GDD mô tả đây chỉ là màn chọn phép trước level — có thể là UI overlay trong Level scene thay vì load scene riêng.
- **Đề xuất:** Cân nhắc dùng UI panel trong Level scene để tránh overhead scene loading. Nếu giữ scene riêng, cần thiết kế data passing giữa scenes.

---

### ISSUE-023: SpellBehaviorParams dùng flat struct — nhiều field unused

- **Vị trí:** TDD §6.1
- **Mô tả:** `SpellBehaviorParams` chứa tất cả params cho mọi loại spell (ProjectileSpeed, AoeRadius, ConeAngle, AuraRadius, SlowAmount...). Với mỗi spell cụ thể, đa số fields là unused. Inspector sẽ hiển thị nhiều field không liên quan.
- **Đề xuất:** Dùng `[SerializeReference]` + abstract `SpellBehaviorParams` với subclass (`LobParams`, `ProjectileParams`, `ConeParams`...) + custom PropertyDrawer. Hoặc chấp nhận flat struct nhưng dùng `#if UNITY_EDITOR` custom editor.

---

## Tổng hợp

| Mức độ | Số lượng | Issues |
|---|---|---|
| 🔴 Nghiêm trọng | 5 | 001–005 |
| 🟠 Quan trọng | 6 | 006–012 |
| 🟡 Cần làm rõ | 11 | 013–023 |
| **Tổng** | **22** | |
