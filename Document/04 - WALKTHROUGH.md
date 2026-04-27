# Implementation Walkthrough — SpellStrike

Tài liệu này ghi chú lại chi tiết các bước được thực hiện, những file sinh ra và logic cốt lõi.

> **Hiện tại đang thực thi:** Phase 3 (Spell System)

---

## 1. Phase 1 — Core Infrastructure (Hoàn tất)

Các script đã được xây dựng làm móng cho toàn bộ game:

- **Event Channels:** Triển khai SO Event Channel patterns để liên lạc cross-system (`VoidEventChannelSO`, `HPEventChannelSO`...).
- **GameManager & Utilities:** `GameManager` singleton quản lý loop, `ObjectPool<T>` tái sử dụng GameObject (tránh GC spikes), `Timer`, và `WeightedRandom`.
- **Data Layer (SOs):** Định nghĩa toàn bộ asset dữ liệu (Spells, Enemies, Boss, Levels, GameConfig, DropTable, AudioLibrary).
- **State Machine:** Bộ khung `IState` và `StateMachine` bằng pattern Dictionary Transitions (học từ file tham chiếu).

---

## 2. Phase 2 — Player & Input (Hoàn tất)

Mục tiêu của phase này là tạo ra bộ khung thực thể Player hoàn chỉnh (HP, Stamina, Movement, Dash, FSM) nhưng chưa cast phép.

### 2.1 Base Combat Components

Các file này được đặt trong `Scripts/Combat/` và nhét vào **cả Player, Enemy, Spawner, LootBox**.

- **StatusEffectController:** Chứa logic buff/debuff.
- **HealthComponent:** Quản lý lượng HP + bắn event cho UI (nếu là Player).
- **DamageDealer:** Nhúng vào bất cứ thứ gì gây damage (đạn bay, vùng nổ, đòn đánh tay).

### 2.2 Player Logic & State Machine

- **PlayerController:** Hub trung tâm kết nối các component. Tự khởi tạo `StateMachine` và map các Condition Transitions bằng Delegate.
- **Player States (Idle, Move, Dash, Dead):** Triển khai FSM logic di chuyển theo hướng input, xoay theo mặt/chuột, và Gravity.
- **DashController:** Dùng Coroutine di chuyển quãng ngắn, apply i-frame.
- **StaminaComponent:** Hồi nội năng tự động sau delay.

### 2.3 Camera & UI

- `CameraFollow.cs` bám đuôi Player tạm thời (sẽ setup Cinemachine sau).
- Dựng UI placeholder `HPBarUI.cs` và `StaminaBarUI.cs` lắng nghe event SO.

---

## 3. Phase 3 — Spell System (Hoàn tất phần Script)

Tập trung xây dựng bộ khung abstract cho phép thuật và tích hợp vào Player. Đã code xong core của toàn bộ 5 loại phép:

### 3.1 Bộ khung hệ thống

- `SpellBase`: Lớp nền abstract cho mọi loại phép.
- `SpellPoolService`: Runtime Singleton tách riêng quản lý `ObjectPool<SpellBase>`. Nó sẽ lazy-load Pool theo ID của `SpellDataSO` khi lần đầu cast.
- `PlayerSpellController`: Quản lý cooldown, đọc Input Cast và gọi Pool để lấy Object ra. Hỗ trợ đầy đủ Click, Hold và Passive.
- `SpellPanelUI`: Hiển thị icon và radial cooldown fade-out.

### 3.2 Bộ 5 Phép thuật (Cơ chế đặc thù)

- **FireballSpell (AreaLob):** Bắn ra quả cầu lửa. Tính quỹ đạo parabol trực tiếp trong runtime bằng công thức lerp + `4_h_r*(1-r)`. Nổ OverlapSphere khi chạm đích.
- **SplashSpell (PiercingProjectile):** Phóng thẳng xuyên hitbox. DamageDealer được bật luôn liên tục và trả về Pool sau khi hết sải.
- **InfernoBreatheSpell (Cone/Hold):** Gắn lên người cast. Khi hold chuột sẽ Tick cooldown nội bộ và quét `OverlapSphere` + math góc (Cone check) để áp Slow/Sát thương định kỳ.
- **StaticFieldSpell (Passive Aura):** Lên đồ là tự chạy. Update liên tục mỗi frame và check Timer Tick. Nếu cooldown xong thì giật sét mọi quái thù địch xung quanh.
- **BlizzardSpell (Transform):** Kích hoạt `Physics.IgnoreLayerCollision` giữa layer Player và Enemy. Cung cấp bộ I-Frames. Tạo Tick damage xoay quanh target và cho Player tự do di chuyển qua lại giữa các quái bị đóng băng. Đưa thẳng logic vào Coroutine để tự dọn dẹp sau duration mà không cần chế `BlizzardState` rườm rà phía PlayerController.

> **Trạng thái cấu phần (Dev): Phase 3 ĐÃ XONG MẶT LOGIC SCRIPT**. Chờ người dùng tạo Prefab, nhét SO và cấu hình Particle/Shader trong Editor.

---

## 4. Phase 4 — Enemy, Spawner & Combat Loop (Hoàn tất phần Script)

Giờ đây quái và đồ loot đã có system core:

### 4.1 Enemy FSM Architecture

- Xây dựng **`EnemyBase`** làm abstract class và cài đặt hệ thống **FSM Declarative** (giống Player).
- Chia hệ thống state thành các state tái sử dụng được: `EnemyChaseState`, `EnemyDeadState`, `EnemyAttackState`, `EnemyPatrolState` và `EnemyRetreatState`.
- Cài đặt 2 con quái chính:
  - **`MeleeEnemy`**: Logic áp sát (Patrol -> Chase -> Attack (khi đủ gần)). Sử dụng OverlapSphere mosh-pit slam.
  - **`RangedEnemy`**: Logic giữ khoảng cách/Hit & Run (Patrol -> Chase -> Attack -> Retreat nếu player tới gần). Bắn `EnemyProjectile`.

### 4.2 Spawner & Loot Drops

- **`SpawnerController`**: MonoBehaviour Coroutine sinh quái theo từng đợt tùy theo cấu hình `SpawnerDataSO`.
- **`LootDropperService`**: Singleton tiện ích. Bất kỳ khi nào Spawner, Quái hay Lootbox chết, nó sẽ gọi Service này và service sẽ Random dựa trên `DropTableSO` (Weighted Drops) để thả đồ ra đất.
- Được kết hợp với abstract component **`DropItem`** cùng 2 class **`SpellDropItem`** (mở khóa phép tức thời) và **`PowerUpDropItem`** (rớt máu, hồi thể lực, shield).
- **`LootBox`**: Chướng ngại vật tĩnh có dính HealthComponent để phá hủy và rớt đồ.

> **Trạng thái cấu phần (Dev): Phase 4 ĐÃ XONG MẶT LOGIC SCRIPT**. Các quái đã sẵn sàng được cấu hình vào Prefab có gắn NavMeshAgent.

---

## 5. Phase 5 — Level Flow & Elite Boss (Hoàn tất phần Script)

Giờ đây các cấu trúc để tạo ra một ván game đầy đủ (Win/Lose) đã kết nối với nhau:

### 5.1 Level Flow (Win/Lose)

- **`LevelManager`**: Đứng ở mỗi map, đếm số lượng `Spawner`. Bất cứ khi nào Spawner chết và biến mất, LevelManager sẽ giảm biến đến `m_SpawnersRemaining`. Khi đếm bằng 0, nó tự động cho xuất hiện Boss hoặc call thẳng Win State.
- Nếu Player chết (thông qua Action `OnDied` của HealthComponent), gọi ngay Lose State.

### 5.2 Save Game & Pre-Game Selection

- Đã cài đặt **`SaveManager`** sử dụng `JsonUtility` đọc/ghi ra `Application.persistentDataPath / SpellStrikeSaveData.json`. Cho phép lưu các phép đã unlock.
- Cài **`SpellSelectionUI`**: Popup hiện ra đầu game, tự động Pause TimeScale, yêu cầu Pool móc ra 3 ngẫu nhiên 3 phép (từ LevelData) để player chọn ấn `Equip`.

### 5.3 Elite Boss FSM

- **`EliteBossController`**: Phiên bản nâng cấp mạnh mẽ từ EnemyBase.
- Nó có khả năng đánh giá một list các kĩ năng đặc biệt `EliteSpellRuntime` cấu hình bên trong `EliteBossDataSO`. Mỗi kĩ năng có cooldown độc lập (Tick trừ theo update).
- Khi quyết định Attack, nếu có kĩ năng chưa dùng, Boss sẽ lấy thẳng kĩ năng từ `SpellPoolService`, setup vị trí bắn (`Override Settings`) và xả đạn như Player. Nếu kỹ năng đang cooldown, Boss đập tay `PerformEliteAttack()`.
- Phát HP riêng thông qua `HPEventChannel` ra cho **`EliteHPBarUI`**.
- Thay vì chết luôn, Boss chết sẽ trigger script thông báo lại cho `LevelManager` tiến đến Màn chiến thắng, đồng thời thả item xịn.

> **Trạng thái cấu phần (Dev): TẤT CẢ PHASE 1 ĐẾN 5 ĐÃ KẾT THÚC VỀ SCRIPT LOGIC MÃ NGUỒN.** Dự án giờ đã sẵn sàng để tích hợp Graphic/Audio hoàn chỉnh trong Unity Editor (Phase 6, 7).
