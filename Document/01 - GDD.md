# Game Design Document (GDD)

# **Spell It! — Top-Down Magic Shooter

> **Phiên bản:** 0.4
> **Ngày:** 2026-04-25 **
> Thể loại:** Top-Down Shooter 3D (Single Player)

---

## Mục lục

1. [Tổng quan dự án](#1-t%E1%BB%95ng-quan-d%E1%BB%B1-%C3%A1n)
2. [Cơ chế điều khiển](#2-c%C6%A1-ch%E1%BA%BF-%C4%91i%E1%BB%81u-khi%E1%BB%83n)
3. [Hệ thống phép thuật](#3-h%E1%BB%87-th%E1%BB%91ng-ph%C3%A9p-thu%E1%BA%ADt)
4. [Hệ thống nhân vật](#4-h%E1%BB%87-th%E1%BB%91ng-nh%C3%A2n-v%E1%BA%ADt)
5. [Kẻ địch và Spawner](#5-k%E1%BA%BB-%C4%91%E1%BB%8Bch-v%C3%A0-spawner)
6. [Elite Boss](#6-elite-boss)
7. [Power-Up](#7-power-up)
8. [Drop & Loot](#8-drop--loot)
9. [Loot Box](#9-loot-box)
10. [Unlock & Progression](#10-unlock--progression)
11. [Điều kiện thắng / thua](#11-%C4%91i%E1%BB%81u-ki%E1%BB%87n-th%E1%BA%AFng--thua)
12. [Luồng gameplay](#12-lu%E1%BB%93ng-gameplay)
13. [Giao diện người dùng (HUD)](#13-giao-di%E1%BB%87n-ng%C6%B0%E1%BB%9Di-d%C3%B9ng-hud)
14. [Âm thanh & Visual Feedback](#14-%C3%A2m-thanh--visual-feedback)
15. [Config & Balancing](#15-config--balancing)
16. [Định hướng kỹ thuật](#16-%C4%91%E1%BB%8Bnh-h%C6%B0%E1%BB%9Bng-k%E1%BB%B9-thu%E1%BA%ADt)

---

## 1. Tổng quan dự án

### 1.1 Mô tả

**SpellStrike** là game top-down shooter 3D, nơi người chơi không sử dụng vũ khí thông thường mà thay vào đó wielding các phép thuật đa dạng. Mỗi phép có hành vi (behavior) và cơ chế casting riêng biệt, buộc người chơi liên tục thích nghi. Sau khi phá hủy toàn bộ Spawner trong màn, một Elite Boss xuất hiện — đánh bại nó để mở khóa phép mới và hoàn thành màn chơi.

### 1.2 Pillars thiết kế

|Pillar|Mô tả|
|---|---|
|**Đa dạng phép thuật**|Mỗi phép có behavior độc lập, tạo cảm giác hoàn toàn khác nhau|
|**Áp lực thường trực**|Kẻ địch liên tục được sinh ra — người chơi không thể đứng yên|
|**Rủi ro & phần thưởng**|Nhặt phép mới đồng nghĩa từ bỏ phép cũ; loot ngẫu nhiên|
|**Kỹ năng người chơi**|Chiến thắng phụ thuộc vào định vị, timing, và lựa chọn phép|
|**Progression rõ ràng**|Mỗi Elite đánh bại = 1 phép mới được mở khóa vĩnh viễn|

---

## 2. Cơ chế điều khiển

### 2.1 Bảng điều khiển

|Input|Hành động|
|---|---|
|`W / A / S / D`|Di chuyển nhân vật theo 4 hướng|
|`Shift` (nhấn)|Dash — tiêu thụ Stamina|
|`Chuột trái (LMB)`|Cast phép hiện tại|
|`Di chuyển chuột`|Xoay hướng nhìn / hướng cast của nhân vật|

### 2.2 Di chuyển

- Nhân vật luôn **xoay mặt về phía con trỏ chuột** trên mặt phẳng ngang với tốc độ xoay rất nhanh (`player_rotate_speed` — config), cảm giác gần như tức thì.
- Tốc độ di chuyển cơ bản: `player_move_speed` _(config — default: 5 m/s)_.
- Không có animation lock — có thể vừa di chuyển vừa cast.

> **Lưu ý:** Một số phép thuật có thể **giảm tốc độ xoay** trong khi đang sử dụng (xem mục 3.4 — Inferno Breathe).

### 2.3 Dash

- Nhấn `Shift`: lướt nhanh theo **hướng WASD hiện tại**. Nếu đứng yên: dash về **phía con trỏ chuột**.
- Con trỏ chuột bị giới hạn trong màn hình — điểm target luôn hợp lệ trong viewport.
- Khi hết Stamina: không thể dash, không có penalty khác.

|Thông số|Config key|Default|
|---|---|---|
|Khoảng cách dash|`dash_distance`|4 m|
|Chi phí Stamina|`dash_stamina_cost`|25|
|I-frame trong dash|`dash_iframe_duration`|0.15s|

---

## 3. Hệ thống phép thuật

### 3.1 Nguyên tắc chung

- Người chơi **chỉ giữ được 1 phép tại một thời điểm**.
- Cast theo hướng **con trỏ chuột** (luôn trong màn hình).
- Nhặt phép drop: phép cũ bị **thay thế ngay lập tức**, không có confirmation.
- Phép **Passive** (Static Field) luôn hoạt động tự động khi được trang bị — LMB không có tác dụng.
- Phép **Transform** (Blizzard) thay đổi hoàn toàn trạng thái người chơi trong thời gian hiệu lực.

### 3.2 Phân loại phép

#### Phép Cơ Bản (Basic Spells)

Có sẵn từ đầu game, luôn có trong pool chọn phép khởi đầu. Không cần unlock.

#### Phép Mở Khóa (Unlocked Spells)

Mở khóa bằng cách đánh bại Elite Boss (xem mục 10). Sau khi mở khóa:

- Xuất hiện trong **pool chọn phép khởi đầu** của tất cả màn (kể cả màn đã chơi).
- Có thể **drop trong game** từ địch, Spawner, và Loot Box.

### 3.3 Danh sách phép

---

#### **Fireball** _(Cơ Bản)_

> _Tham chiếu: R Ziggs — League of Legends_

**Behavior: Area Lob**

Ném một quả cầu lửa theo quỹ đạo cầu vào vị trí con trỏ chuột chỉ định. Khi chạm đất, nổ và gây sát thương AoE trong bán kính vùng nổ.

|Tham số|Config key|Default|
|---|---|---|
|Sát thương|`fireball_damage`|60|
|Bán kính AoE|`fireball_aoe_radius`|2.5m|
|Cooldown|`fireball_cooldown`|3.0s|

---

#### **Splash** _(Cơ Bản)_

> _Tham chiếu: Q Lissandra — League of Legends_

**Behavior: Piercing Projectile**

Phóng một tia nước bay thẳng theo hướng con trỏ chỉ định. Tia **xuyên qua tất cả kẻ địch** trên đường đi, gây sát thương cho mọi đối tượng bị chạm. Bay đến khi hết tầm tối đa.

|Tham số|Config key|Default|
|---|---|---|
|Sát thương / địch|`splash_damage`|35|
|Tốc độ bay|`splash_projectile_speed`|18 m/s|
|Tầm tối đa|`splash_range`|16m|
|Cooldown|`splash_cooldown`|1.5s|

---

#### **Inferno Breathe** _(Mở Khóa sau Level 1)_

> _Tham chiếu: Q Rumble — League of Legends_

**Behavior: Cone Spray (Hold)**

Giữ LMB để phun lửa liên tục theo hình nón về phía con trỏ. Gây sát thương liên tục theo tick và **làm chậm** địch trong vùng phun.

**Cơ chế đặc biệt:**

- Có một **khoảng delay khởi động** ngắn từ khi nhấn LMB đến khi lửa thực sự phun ra (`inferno_startup_delay` — config).
- Trong khi đang phun, **tốc độ xoay nhân vật bị giảm đáng kể** (`inferno_rotate_speed` — config, thấp hơn nhiều so với `player_rotate_speed` bình thường).
- **Không có cooldown** — chỉ bị giới hạn bởi thời gian người chơi giữ LMB.

|Tham số|Config key|Default|
|---|---|---|
|Sát thương / tick|`inferno_damage_per_tick`|8|
|Tần suất tick|`inferno_tick_rate`|0.1s|
|Delay khởi động|`inferno_startup_delay`|0.2s|
|Góc hình nón|`inferno_cone_angle`|60°|
|Bán kính hình nón|`inferno_cone_radius`|6m|
|Tốc độ xoay khi phun|`inferno_rotate_speed`|(config riêng — thấp hơn default)|
|Slow % áp lên địch|`inferno_slow_amount`|30%|
|Thời lượng slow|`inferno_slow_duration`|0.5s (reset mỗi tick)|

---

#### **Static Field** _(Mở Khóa sau Level 2)_

> _Tham chiếu: R Kennen — League of Legends_

**Behavior: Passive Aura**

Khi trang bị, tự động tạo một **vùng điện từ trường** hình tròn lấy nhân vật làm tâm. Mọi kẻ địch bước vào vùng liên tục nhận sát thương theo tick. Không cần nhấn LMB — LMB không có tác dụng khi cầm phép này.

|Tham số|Config key|Default|
|---|---|---|
|Sát thương / tick|`staticfield_damage_per_tick`|10|
|Tần suất tick|`staticfield_tick_rate`|0.5s|
|Bán kính vùng|`staticfield_radius`|5m|

---

#### **Blizzard** _(Mở Khóa sau Level 3)_

> _Tham chiếu: E Kennen — League of Legends_

**Behavior: Transform**

Nhấn LMB để người chơi **hóa thành cơn bão tuyết** trong một khoảng thời gian. Trong trạng thái bão:

- Người chơi **miễn nhiễm toàn bộ sát thương**.
- Người chơi **di chuyển xuyên qua kẻ địch** (không bị block về vật lý).
- Kẻ địch **tiếp xúc với cơn bão** (chạm vào hitbox người chơi) liên tục nhận sát thương theo tick.
- Kết thúc khi hết thời lượng; sau đó vào cooldown.

|Tham số|Config key|Default|
|---|---|---|
|Sát thương / tick|`blizzard_damage_per_tick`|15|
|Tần suất tick|`blizzard_tick_rate`|0.2s|
|Thời lượng bão|`blizzard_duration`|4s|
|Cooldown|`blizzard_cooldown`|12s|

---

### 3.4 Bảng tổng hợp phép

|Tên phép|Loại|Behavior|Input|Cooldown|Ghi chú|
|---|---|---|---|---|---|
|Fireball|Cơ Bản|Area Lob|Click|3.0s|AoE khi chạm đất|
|Splash|Cơ Bản|Piercing Projectile|Click|1.5s|Xuyên qua địch|
|Inferno Breathe|Mở Khóa (LV1)|Cone Spray|Hold|Không có|Giảm tốc xoay khi phun|
|Static Field|Mở Khóa (LV2)|Passive Aura|Tự động|Không có|LMB vô hiệu|
|Blizzard|Mở Khóa (LV3)|Transform|Click|12s|Miễn sát thương + xuyên địch|

### 3.5 Chọn phép khởi đầu

Trước khi vào màn, người chơi chọn **1 trong N phép** được rút ngẫu nhiên từ pool _(N config theo màn)_. Pool bao gồm phép Cơ Bản và các phép Mở Khóa mà người chơi **đã unlock**. Màn chọn hiển thị:

- Tên phép + loại (Cơ Bản / Mở Khóa)
- Mô tả behavior + cơ chế đặc biệt
- Chỉ số chính: Sát thương, Cooldown, Tầm / Bán kính

---

## 4. Hệ thống nhân vật

### 4.1 Chỉ số HP

|Chỉ số|Config key|Default|
|---|---|---|
|HP tối đa|`player_max_hp`|100|
|HP hồi tự nhiên|—|Không có (chỉ qua power-up)|

### 4.2 Chỉ số Stamina

Stamina là tài nguyên độc lập, **chỉ dùng cho Dash**.

|Chỉ số|Config key|Default|
|---|---|---|
|Stamina tối đa|`player_max_stamina`|100|
|Chi phí Dash|`dash_stamina_cost`|25|
|Delay trước khi hồi|`stamina_regen_delay`|0.5s|
|Tốc độ hồi|`stamina_regen_rate`|20/giây|

### 4.3 Status Effects

|Trạng thái|Nguồn|Hiệu ứng|Stack?|
|---|---|---|---|
|**Slowed**|Phép địch / Ice Bolt|Giảm tốc di chuyển `slow_amount`%|Không (lấy giá trị cao nhất)|
|**Shielded**|Power-up|Hấp thụ 1 đòn tiếp theo|Không|
|**Invincible**|Power-up / Dash i-frame|Miễn toàn bộ sát thương|Không (timer reset)|
|**Hasted**|Power-up|Tăng tốc `haste_amount`%|Không (timer reset)|

---

## 5. Kẻ địch và Spawner

### 5.1 Spawner

- Được đặt cố định bởi Level Designer trong scene.
- Định kỳ **sinh ra kẻ địch** trong bán kính xung quanh.
- Có HP riêng — bị phá hủy bằng phép của người chơi.
- Khi phá xong: dừng sinh địch, trigger drop loot, kiểm tra điều kiện kích hoạt Elite.

|Thuộc tính|Config key|Default|
|---|---|---|
|HP|`spawner_hp`|200|
|Chu kỳ spawn|`spawner_interval`|5s|
|Số địch mỗi lần|`spawner_count_min/max`|1–3|
|Pool loại địch|`spawner_enemy_pool`|Tùy level design|

- Các Spawner có thể có **pool địch khác nhau** tùy Level Designer.
- Chỉ số scale theo màn qua hệ số config (xem mục 15).

### 5.2 Loại kẻ địch thường

#### Cận chiến (Melee)

|Chỉ số|Config key|Default|
|---|---|---|
|HP|`melee_hp`|40|
|Tốc độ|`melee_speed`|3.5 m/s|
|Sát thương / đòn|`melee_damage`|10|
|Tầm tấn công|`melee_attack_range`|1.2m|
|Attack rate|`melee_attack_rate`|1.0s|

**Hành vi:** Truy đuổi → tấn công khi trong tầm.

#### Tầm xa (Ranged)

|Chỉ số|Config key|Default|
|---|---|---|
|HP|`ranged_hp`|60|
|Tốc độ|`ranged_speed`|2.0 m/s|
|Sát thương / đòn|`ranged_damage`|15|
|Tầm tấn công|`ranged_attack_range`|12m|
|Attack rate|`ranged_attack_rate`|2.5s|
|Khoảng cách duy trì|`ranged_preferred_dist`|6–10m|

**Hành vi:** Tiếp cận đến tầm lý tưởng → đứng bắn → lùi nếu bị áp sát.

### 5.3 AI hành vi địch thường

```
Spawn
  ↓
Pathfind đến người chơi (NavMesh — không xuyên tường, bị block bởi địch khác)
  ├─ Melee:  đuổi đến attack_range → tấn công
  └─ Ranged: đến preferred_dist → đứng bắn; nếu player < 3m → lùi
  ↓
Mất target quá lose_aggro_time (config) → tuần tra quanh vị trí spawn
```

---

## 6. Elite Boss

### 6.1 Trigger xuất hiện

- Elite Boss **không spawn trong quá trình chiến đấu với Spawner**.
- Khi **toàn bộ Spawner trong màn bị phá hủy** → Elite Boss xuất hiện tại vị trí được chỉ định bởi Level Designer.
- Mỗi màn có **đúng 1 Elite Boss**.

### 6.2 Chỉ số Elite Boss

Mỗi Elite Boss có ScriptableObject riêng. Placeholder chỉ số:

|Chỉ số|Config key|Placeholder|
|---|---|---|
|HP|`elite_hp`|500|
|Tốc độ di chuyển|`elite_speed`|2.5 m/s|
|Tầm phát hiện người chơi|`elite_aggro_range`|Toàn bộ màn|

### 6.3 Spell của Elite Boss

- Mỗi Elite Boss có **danh sách phép riêng** (1–3 phép, config theo từng Elite).
- Elite cast phép theo behavior tương tự người chơi (Projectile, Lob, Beam, Burst, Homing) nhưng được config riêng về: damage, cooldown, tầm, tốc độ projectile.
- Elite có thể có **nhiều phép** và luân phiên cast theo logic riêng (ví dụ: ưu tiên Lob khi xa, Burst khi sát).
- **Placeholder hành vi AI Elite:**

```
Aggro người chơi ngay khi spawn
  ↓
Đánh giá khoảng cách đến người chơi
  ├─ Xa (> elite_range_threshold): cast phép tầm xa (Projectile / Lob / Homing)
  ├─ Gần (≤ elite_range_threshold): cast phép cận chiến (Burst)
  └─ Cooldown tất cả phép: di chuyển về phía người chơi
```

### 6.4 Phần thưởng khi đánh bại Elite

- Khi Elite Boss chết: **mở khóa 1 phép mới** được gắn vào Elite đó (config trong EliteBossData).
- Spell mở khóa được **lưu vào SaveData** của người chơi — tồn tại vĩnh viễn qua các lần chơi.
- Elite **không drop item loot** khi chết (phần thưởng là unlock spell).

---

## 7. Power-Up

Power-up được nhặt **tự động** khi người chơi đi vào vùng collider. Biến mất sau `powerup_lifetime` giây nếu không nhặt; nhấp nháy cảnh báo trong `drop_warning_time` giây cuối.

|Power-Up|Hiệu ứng|Thời lượng|Config key|
|---|---|---|---|
|**Hồi Máu** ❤️|Hồi ngay `heal_amount` HP (không vượt max)|Tức thì|`heal_amount`|
|**Tăng Tốc** ⚡|+`haste_amount`% tốc độ di chuyển|`haste_duration`|`haste_amount`, `haste_duration`|
|**Lá Chắn** 🛡️|Hấp thụ 1 đòn tiếp theo|Đến khi bị kích hoạt|—|
|**Bất Tử** ✨|Miễn toàn bộ sát thương|`invincible_duration`|`invincible_duration`|

**Config mặc định:**

|Config key|Default|
|---|---|
|`heal_amount`|25|
|`haste_amount`|60%|
|`haste_duration`|8s|
|`invincible_duration`|5s|
|`powerup_lifetime`|10s|
|`drop_warning_time`|3s|

---

## 8. Drop & Loot

### 8.1 Drop từ kẻ địch thường

Roll theo **drop table** config riêng cho từng loại địch (weighted random).

**Placeholder drop table — Melee:**

|Item|Weight|
|---|---|
|Không có|75|
|Power-up ngẫu nhiên|20|
|Phép ngẫu nhiên|5|

**Placeholder drop table — Ranged:**

|Item|Weight|
|---|---|
|Không có|70|
|Power-up ngẫu nhiên|22|
|Phép ngẫu nhiên|8|

### 8.2 Drop từ Spawner

Khi Spawner bị phá: **luôn drop 1 item** (guaranteed). Roll từ drop table riêng của Spawner.

**Placeholder drop table — Spawner:**

|Item|Weight|
|---|---|
|Phép ngẫu nhiên|60|
|Power-up ngẫu nhiên|30|
|Hồi Máu|10|

### 8.3 Pool phép drop trong màn

- `spell_random` roll từ **spell drop pool** được config trong LevelData.
- Pool bao gồm phép Cơ Bản và các phép Mở Khóa mà người chơi **đã unlock**.
- Phép đang cầm **vẫn có thể drop** (người chơi có thể bỏ qua).

### 8.4 Thời gian tồn tại của drop

|Config key|Default|Áp dụng cho|
|---|---|---|
|`spell_drop_lifetime`|15s|Phép drop trên sàn|
|`powerup_lifetime`|10s|Power-up drop trên sàn|
|`drop_warning_time`|3s|Thời gian nhấp nháy trước khi biến mất|

### 8.5 Cơ chế nhặt

- **Tự động** khi đi vào vùng collider của item — không cần nhấn phím.
- Nếu không muốn nhặt: **tránh không đi vào vùng** của item.
- Nhặt phép: thay thế phép cũ ngay lập tức.

---

## 9. Loot Box

### 9.1 Mô tả

Loot Box là **vật thể tĩnh** được đặt trên bản đồ bởi Level Designer. Người chơi phá hủy box bằng phép để nhận loot.

### 9.2 Chỉ số

|Thuộc tính|Config key|Default|
|---|---|---|
|HP|`lootbox_hp`|30|
|Có thể bị block đường đi không|—|Tùy Level Designer (có thể dùng làm cover)|

### 9.3 Drop

Khi bị phá hủy: **luôn drop 1 item** (guaranteed). Roll từ drop table riêng của Loot Box.

**Placeholder drop table — Loot Box:**

|Item|Weight|
|---|---|
|Phép ngẫu nhiên|50|
|Power-up ngẫu nhiên|40|
|Hồi Máu|10|

- Loot Box **không drop "Không có"** — luôn có phần thưởng.
- Item drop từ Loot Box tuân theo thời gian tồn tại và cơ chế nhặt như mục 8.

---

## 10. Unlock & Progression

### 10.1 Cơ chế mở khóa phép

- Mỗi **Elite Boss** gắn với 1 phép mở khóa duy nhất, được khai báo trong `EliteBossData`.
- Khi Elite Boss bị đánh bại lần đầu: phép đó được **thêm vào danh sách đã unlock** của người chơi.
- Mỗi Elite chỉ cần đánh bại **1 lần duy nhất** để unlock — các lần chơi lại không unlock lại.

### 10.2 Lưu trữ tiến trình

- Danh sách phép đã unlock được lưu vào **SaveData** (persistent).
- Cần reset SaveData để mất tiến trình unlock (không có mechanic reset trong gameplay bình thường).

### 10.3 Ảnh hưởng đến gameplay

|Khi nào|Ảnh hưởng|
|---|---|
|Chọn phép khởi đầu|Pool mở rộng thêm các phép đã unlock|
|Drop trong game|Phép đã unlock có thể xuất hiện trong drop / Loot Box|
|Elite Boss chưa unlock spell|Elite Boss vẫn tồn tại; spell chỉ unlock khi đánh bại|

---

## 11. Điều kiện thắng / thua

### 11.1 Thắng

> Đánh bại **Elite Boss** của màn.

- Màn hình chiến thắng hiện ra.
- Thông báo phép mới được mở khóa (nếu là lần đầu đánh bại Elite này).
- Thống kê: Thời gian hoàn thành, Địch tiêu diệt, Phép đang dùng, HP còn lại.

### 11.2 Thua

> HP người chơi về **0** (trong bất kỳ giai đoạn nào — chiến đấu Spawner hoặc Elite).

- Màn hình thất bại hiện ra.
- Tùy chọn: **Thử lại** (từ đầu màn) / **Menu chính**.
- Spell đã unlock **không bị mất** khi thua.

---

## 12. Luồng gameplay

```
[Màn chọn phép khởi đầu]
(Pool = phép Cơ Bản + phép đã Unlock)
        ↓
[Bắt đầu màn — giai đoạn Spawner]
        ↓
[Di chuyển / Dash / Cast phép] ◄────────────────────────┐
    ↙                    ↘                              │
[Tiêu diệt địch]      [Bị tấn công → mất HP]           │
    ↓                       ↓                           │
[Roll drop table]       [HP = 0 → THUA]                 │
    ↓                                                   │
[Item drop lên sàn]                                     │
[Đi vào vùng → nhặt tự động]                           │
    ├─ Phép → thay phép đang cầm                        │
    └─ Power-up → áp dụng hiệu ứng                      │
        ↓                                               │
[Phá Loot Box → drop guaranteed]                        │
        ↓                                               │
[Phá Spawner → drop guaranteed]                         │
        ↓                                               │
[Toàn bộ Spawner bị phá?]                               │
    └─ Chưa → tiếp tục ─────────────────────────────────┘
    └─ Rồi ↓
[Giai đoạn Elite Boss xuất hiện]
        ↓
[Chiến đấu Elite Boss]
    ↙               ↘
[Elite chết]     [HP = 0 → THUA]
    ↓
[Mở khóa phép mới (nếu lần đầu)]
    ↓
[THẮNG]
```

---

## 13. Giao diện người dùng (HUD)

### 13.1 Layout

Dựa trên bản thiết kế (Mockup), giao diện chính trong trận đấu bao gồm:

```text
┌──────────────────────────────────────────────────────────┐
│  [Health Bar] (Thanh màu đỏ, liền mạch)                  │
│  [Stamina Bar] (Chia khối, mỗi khối = 1 lần Dash)        │
│                                                          │
│                                                          │
│               [Player]  ───(Hitbox)──> [Cursor]          │
│                                                          │
│                                                          │
│                                 [ Elite Name         ]   │
│  [Spell Icon]                   [ Elite Health Bar   ]   │
│  (Làm mờ dọc / Cooldown)                                 │
└──────────────────────────────────────────────────────────┘
```

### 13.2 Bảng chi tiết

| Element | Vị trí | Cơ chế hoạt động |
|---|---|---|
| **Health Bar** | Góc trên trái | Thanh Bar đỏ mượt (Solid continuous bar). Hiển thị lượng HP hiện tại. |
| **Stamina Bar** | Góc trên trái (Dưới HP) | Chia thành **các khối (segments)** (vd: 4 khối màu cam). Mỗi khối cứng tương ứng với cost của **1 lần Dash**. Khối xám = đang hồi. |
| **Spell Hitbox** | Tại Player & Chuột (World-space) | Luôn hiển thị vùng tác dụng của phép sát hướng chuột: <br>- **Circle**: Đặt tại chuột <br>- **Beam**: Chiếu thẳng từ Player đến chuột <br>- **Cone**: Tỏa hình nón từ Player theo hướng chuột. |
| **Spell Icon** | Góc dưới trái | Biểu tượng phép đang cầm. **Hiệu ứng làm mờ (Fill)** che phủ icon sẽ tụt dần từ trên xuống theo Cooldown. Thường xuyên sáng nếu phép dạng Passive/Không CD. |
| **Elite Boss HP** | Cạnh vát dưới, giữa màn hình | Chỉ hiện khi đấu Boss. Thanh HP cực lớn nằm ngang, ở giữa thanh ghi tên Elite Boss. |
| **Minimap** _(Tùy chọn)_ | Góc trên phải | Radar hiển thị chấm màu (Địch, Spawner, Boss). |
| **Feedback UI** | Giữa màn / World-space | Chữ nổi sát thương, thông báo nhặt item, thông báo unlock spell. |

---

## 14. Âm thanh & Visual Feedback

> ⚠️ **Placeholder** — Định nghĩa sự kiện cần SFX/VFX. Asset cụ thể xác định trong giai đoạn production. Placeholder chỉ bao gồm những gì đã được mô tả trong thiết kế — không mở rộng thêm. Đồng thời không đại diện cho những SFX/VFX sẽ có trong sản phẩm cuối

### 14.1 SFX

|Sự kiện|Placeholder ID|
|---|---|
|Cast phép (theo behavior)|`sfx_cast_{behavior_type}`|
|Cooldown kết thúc|`sfx_cooldown_ready`|
|Nhặt phép|`sfx_pickup_spell`|
|Nhặt power-up|`sfx_pickup_powerup`|
|Dash|`sfx_dash`|
|Người chơi nhận sát thương|`sfx_player_hurt`|
|Người chơi chết|`sfx_player_death`|
|Địch thường nhận sát thương|`sfx_enemy_hurt`|
|Địch thường chết|`sfx_enemy_death`|
|Spawner nhận sát thương|`sfx_spawner_hurt`|
|Spawner bị phá hủy|`sfx_spawner_destroy`|
|Loot Box nhận sát thương|`sfx_lootbox_hurt`|
|Loot Box bị phá hủy|`sfx_lootbox_destroy`|
|Elite Boss xuất hiện|`sfx_elite_spawn`|
|Elite Boss cast phép|`sfx_elite_cast_{behavior_type}`|
|Elite Boss nhận sát thương|`sfx_elite_hurt`|
|Elite Boss chết|`sfx_elite_death`|
|Unlock phép mới|`sfx_spell_unlock`|
|Drop sắp biến mất|`sfx_drop_warning`|
|Thắng|`sfx_win`|
|Thua|`sfx_lose`|

### 14.2 BGM

|Trạng thái|Placeholder ID|
|---|---|
|Màn chọn phép|`bgm_menu`|
|Giai đoạn Spawner|`bgm_gameplay`|
|Giai đoạn Elite Boss|`bgm_elite`|
|Thắng|`bgm_win`|
|Thua|`bgm_lose`|

### 14.3 VFX

|Sự kiện|Placeholder ID|
|---|---|
|Projectile đang bay|`vfx_projectile_trail`|
|Projectile trúng đích|`vfx_projectile_hit`|
|Area Lob — vòng chỉ định trên đất|`vfx_lob_indicator`|
|Area Lob — chạm đất|`vfx_lob_impact`|
|Beam đang bắn|`vfx_beam_active`|
|Melee Burst — sóng lan|`vfx_burst_wave`|
|Homing — trail lượn|`vfx_homing_trail`|
|Dash — afterimage|`vfx_dash_trail`|
|Nhặt phép|`vfx_pickup_spell`|
|Lá Chắn vỡ|`vfx_shield_break`|
|Bất Tử đang hiệu lực|`vfx_invincible_aura`|
|Địch thường chết|`vfx_enemy_death`|
|Spawner bị phá|`vfx_spawner_destroy`|
|Loot Box bị phá|`vfx_lootbox_destroy`|
|Elite Boss xuất hiện|`vfx_elite_spawn`|
|Elite Boss nhận sát thương|`vfx_elite_hurt`|
|Elite Boss chết|`vfx_elite_death`|
|Unlock phép mới|`vfx_spell_unlock`|
|Drop phép trên sàn (idle)|`vfx_spell_drop_idle`|
|Drop sắp biến mất|`vfx_drop_fade`|

---

## 15. Config & Balancing

Toàn bộ chỉ số đọc từ **ScriptableObject / config file**. Không có giá trị hardcode trong code logic.

### 15.1 Hệ số scale theo màn

Mỗi màn có thể định nghĩa hệ số nhân áp lên chỉ số gốc:

|Config key|Áp dụng cho|
|---|---|
|`level_enemy_hp_mult`|HP tất cả địch thường|
|`level_enemy_dmg_mult`|Sát thương địch thường|
|`level_enemy_speed_mult`|Tốc độ địch thường|
|`level_spawner_hp_mult`|HP Spawner|
|`level_spawn_rate_mult`|Chu kỳ spawn (< 1 = nhanh hơn)|
|`level_elite_hp_mult`|HP Elite Boss|
|`level_elite_dmg_mult`|Sát thương phép của Elite Boss|

### 15.2 Config drop table

Drop table định nghĩa dưới dạng danh sách weighted, ví dụ:

```json
{
  "enemy_melee_drop_table": [
    { "item": "nothing",        "weight": 75 },
    { "item": "powerup_random", "weight": 20 },
    { "item": "spell_random",   "weight": 5  }
  ],
  "spawner_drop_table": [
    { "item": "spell_random",   "weight": 60 },
    { "item": "powerup_random", "weight": 30 },
    { "item": "heal",           "weight": 10 }
  ],
  "lootbox_drop_table": [
    { "item": "spell_random",   "weight": 50 },
    { "item": "powerup_random", "weight": 40 },
    { "item": "heal",           "weight": 10 }
  ]
}
```

- `spell_random`: roll từ spell drop pool của màn (phép Cơ Bản + đã Unlock).
- `powerup_random`: roll đều giữa 4 loại power-up (hoặc weighted riêng nếu cần).

### 15.3 Config lifetime drop

|Config key|Default|
|---|---|
|`spell_drop_lifetime`|15s|
|`powerup_lifetime`|10s|
|`drop_warning_time`|3s|

---

## 16. Định hướng kỹ thuật

### 16.1 Engine đề xuất

- **Unity 3D** (URP pipeline).

### 16.2 Camera

- Top-down, góc ~60–70°, smooth follow với `camera_lerp_speed` (config).
- Con trỏ chuột giới hạn trong viewport — không cần xử lý out-of-screen targeting.

### 16.3 Hệ thống phép

```
SpellData (ScriptableObject)
├── id, displayName, icon
├── spellCategory: Basic | Unlocked
├── behaviorType: Projectile | Lob | Beam | MeleeBurst | Homing
├── damage, cooldown, range
└── behaviorParams: Dictionary<string, float>
    (ví dụ: projectile_speed, aoe_radius, beam_max_charge, knockback_force...)

SpellBase (abstract MonoBehaviour)
├── SpellData data
├── float currentCooldown
├── Cast(Vector3 targetPoint)   // abstract
├── OnEquip() / OnUnequip()
└── GetCooldownRatio() → float [0..1]

// Concrete implementations
ProjectileSpell : SpellBase
LobSpell        : SpellBase
BeamSpell       : SpellBase
BurstSpell      : SpellBase
HomingSpell     : SpellBase

PlayerSpellController (MonoBehaviour)
├── SpellBase equippedSpell
├── EquipSpell(SpellData data)
└── TryCast(Vector3 mouseWorldPos)
```

### 16.4 Drop Item

```
DropItem (MonoBehaviour)               // Base class cho mọi item drop
├── float lifetime                     // Từ config
├── float warningTime                  // Từ config
├── Collider triggerCollider
├── OnTriggerEnter(Player) → OnPickup()  // abstract
└── Coroutine: LifetimeCountdown → flash(vfx_drop_fade) → Destroy

SpellDropItem : DropItem
├── SpellData spellData
└── OnPickup() → PlayerSpellController.EquipSpell(spellData)

PowerUpDropItem : DropItem
├── PowerUpType type
└── OnPickup() → PlayerStatusController.ApplyPowerUp(type)
```

### 16.5 Loot Box

```
LootBox (MonoBehaviour)
├── LootBoxData (ScriptableObject)
│   ├── hp
│   └── dropTable: List<WeightedItem>
├── HealthComponent
└── OnDeath() → LootDropper.SpawnDrop(dropTable, position)
```

### 16.6 Spawner

```
SpawnerController (MonoBehaviour)
├── SpawnerData (ScriptableObject)
│   ├── hp, spawnInterval, spawnCountMin/Max
│   └── enemyPool: List<EnemyData>
├── HealthComponent
├── Coroutine: SpawnLoop
└── OnDeath()
    ├── LootDropper.SpawnDrop(dropTable, position)
    └── LevelManager.OnSpawnerDestroyed(this)

LevelManager
├── List<SpawnerController> spawners
├── OnSpawnerDestroyed(spawner)
│   └── Nếu tất cả spawners chết → TriggerElitePhase()
└── TriggerElitePhase() → EliteBoss.Activate()
```

### 16.7 Elite Boss

```
EliteBossData (ScriptableObject)
├── id, displayName
├── hp, speed
├── spellList: List<EliteSpellEntry>
│   └── EliteSpellEntry { SpellData, castPriority, minRange, maxRange }
└── unlockedSpellReward: SpellData   // Phép mở khóa khi đánh bại

EliteBoss (MonoBehaviour)
├── EliteBossData data
├── HealthComponent
├── EliteAI                          // State machine: Idle → Combat
│   ├── EvaluateRange() → chọn spell phù hợp
│   └── Cast(SpellData, targetPoint)
└── OnDeath()
    ├── SaveData.UnlockSpell(data.unlockedSpellReward)
    └── LevelManager.OnEliteDefeated()
```

### 16.8 Unlock & Save

```
SaveData
├── List<string> unlockedSpellIds    // Lưu persistent (PlayerPrefs / file)
├── UnlockSpell(SpellData)
└── IsUnlocked(SpellData) → bool

SpellPool (static / service)
├── GetStartingPool() → Basic spells + unlocked spells
└── GetDropPool(LevelData) → Filtered by level config + unlocked
```

### 16.9 LootDropper (shared utility)

```
LootDropper (static / service)
├── SpawnDrop(List<WeightedItem> table, Vector3 position)
│   ├── Roll weighted random → ItemType
│   ├── Nếu spell_random → SpellPool.GetDropPool() → roll spell
│   ├── Nếu powerup_random → roll powerup type
│   └── Instantiate DropItem tại position
└── RollWeighted(List<WeightedItem>) → ItemType
```

### 16.10 Level Data

```
LevelData (ScriptableObject)
├── levelName
├── startingSpellPoolSize: int         // N phép hiện khi chọn khởi đầu
├── spellDropPool: List<SpellData>     // Phép có thể drop trong màn
├── scalingConfig: LevelScalingConfig  // Hệ số nhân chỉ số
└── eliteBossData: EliteBossData       // Elite của màn này
```

---

_Tài liệu v0.3 — Cập nhật: Elite Boss system, Loot Box, Unlock & Progression, làm rõ placeholder SFX/VFX, bổ sung architecture cho EliteBoss / LootBox / SpellPool / LootDropper._
