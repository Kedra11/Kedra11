# Reference / Etalon

Эта страница — канонические значения проекта, выверенные против оригинальной NES Battle City (через ROM-дамп [`dogballs/cattle-bity`](https://github.com/dogballs/cattle-bity)). Любое отклонение от этого — баг.

## Координатная сетка (1:1 с cattle-bity)

| Параметр | Значение | Соответствие в оригинале |
|---|---|---|
| `CELL_SIZE` | **32 px** | sub-tile (TILE_SIZE_MEDIUM в cattle-bity) |
| `TANK_CELL_SIZE` | 64 px | macro tile (TILE_SIZE_LARGE), 1 макро = 2×2 sub-tile |
| `DESTRUCTION_CELL_SIZE` | 16 px | brick chunk (TILE_SIZE_SMALL) |
| `GRID_COLUMNS` × `GRID_ROWS` | 26 × 26 | 13×13 макро = 26×26 sub-tile |
| `BOARD_SIZE` | 832 × 832 | FIELD_SIZE = 13 × 64 = 832 |
| `BOARD_ORIGIN` | (224, 32) | левый отступ под панель + верхняя кромка |
| `SCREEN_SIZE` | 1280 × 896 | 224 + 832 + 224 ширина, 32 + 832 + 32 высота — серая рамка вокруг поля |

## Сущности

| Параметр | Значение | Источник |
|---|---|---|
| `TANK_DRAW_SIZE` | 64 px | 1 макро |
| `TANK_RADIUS` | 28.5 px | ≈ 90% TANK_DRAW_SIZE/2, чтобы избегать edge-flicker |
| `BULLET_RADIUS` | 5 px | cattle-bity BULLET_WIDTH = 12 → радиус 6, у нас чуть меньше |
| `BULLET_SPAWN_OFFSET` | 40 px | пуля спавнится перед центром танка |

## Скорости (cattle-bity, без масштабирования)

| Параметр | px/s | cattle-bity |
|---|---|---|
| `PLAYER_MOVE_SPEED` | 180 | MoveSpeed.Medium |
| `ENEMY_SPEED_SLOW` | 120 | MoveSpeed.Slow (Basic, Power, Armor) |
| `ENEMY_SPEED_FAST` | 240 | MoveSpeed.Fast (Fast tank) |
| `BULLET_SPEED_SLOW` | 600 | BulletSpeed.Slow |
| `BULLET_SPEED_FAST` | 900 | BulletSpeed.Fast |

## Тайминги

| Параметр | Значение | Источник |
|---|---|---|
| `RESPAWN_INVULNERABLE_TIME` | 2.8 s | NES BC ~2-3 sec |
| `ENEMY_SPAWN_FLASH_TIME` | 1.7 s | NES BC ~2 sec |
| `ENEMY_SPAWN_DELAY` | 3.0 s | cattle-bity ENEMY_SPAWN_DELAY |
| `SHIELD_DURATION` | 10 s | cattle-bity SHIELD_POWERUP_DURATION |
| `BASE_DEFENCE_DURATION` | 17 s | cattle-bity BASE_DEFENCE_POWERUP_DURATION |
| `FREEZE_DURATION` | 10 s | cattle-bity FREEZE_POWERUP_DURATION |

## Очки

| Враг | Очки |
|---|---|
| Basic (`a`) | 100 |
| Fast (`b`) | 200 |
| Power (`c`) | 300 |
| Armor (`d`) | 400 |
| Power-up pickup | 1000 |
| Extra life | при 20000 |

## Спавн-точки (sub-tile / наша «cell»)

```
PLAYER_SPAWN_CELLS  = [(8, 24), (16, 24)]   # P1 слева от базы, P2 справа
SPAWN_CELLS         = [(0, 0), (12, 0), (24, 0)]  # враги: левый угол, центр, правый угол
BASE_MACRO_CELLS    = [(12, 24), (13, 24), (12, 25), (13, 25)]  # 2×2 макро = орёл
```

`_tank_cell_center(cell)` возвращает **центр** 2×2-макро футпринта, не угол. Танк визуально 64×64, центр на sub-tile-границе.

## Стены вокруг базы (destruction-клетки 16 px)

```
Top wall:   x 22..29, y 46..47   (8 cells × 2 cells)   = 4 sub-tile × 1 sub-tile
Left wall:  x 22..23, y 48..51   (2 cells × 4 cells)   = 1 sub-tile × 2 sub-tile
Right wall: x 28..29, y 48..51   (2 cells × 4 cells)   = 1 sub-tile × 2 sub-tile
Eagle:      x 24..27, y 48..51   (4 cells × 4 cells)   = 2 sub-tile × 2 sub-tile
```

Точно как в cattle-bity `Base.WALL_REGIONS`.

## Разрушение стен (cattle-bity TerrainTileDestroyer)

| Тип пули | Глубина разрушения | Где используется |
|---|---|---|
| Low | **1 destruction-клетка** (16 px) | обычный игрок (0–2 звезды) и **все** враги |
| High | **2 destruction-клетки** (32 px) | 3-звёздочный игрок; ломает также 1 макро стали |

Поперечная ширина разрушения = ширина танка (4 destruction-клетки = 64 px).

«BB» в layout (1 макро = 4 destruction-клетки в глубину) = **4 выстрела** обычной пули.

## Логика волн

- **20 врагов** на стейдж, до **4** одновременно на поле.
- Тип каждого врага задан в `STAGE_ENEMY_TIERS[stage]` (последовательность из 20 букв `a/b/c/d`).
- Враги под индексами в `STAGE_ENEMY_DROP_INDICES[stage]` (обычно 4-й, 11-й, 18-й = индексы 3, 10, 17) мерцают и при убийстве дают рандомный power-up.
- Power-up на поле всегда один. Новый затирает предыдущий.

## Управление (закреплено)

| Действие | P1 (single + 2P) | P2 (только 2P) |
|---|---|---|
| Движение | WASD (+ стрелки в single) | стрелки |
| Выстрел | Space (+ Enter в single) | Enter / Numpad Enter |
| Пауза | Esc | — |
| Меню после игры | Esc или Space/Enter | — |

В меню: ↑/↓ выбор, Space/Enter подтвердить.

В Construction Mode: WASD/стрелки — курсор, **1..6** — кисть (Empty / Brick / Steel / Water / Forest / Ice), Space (удержать) — рисовать, F — залить всё, Enter — сохранить и тестировать, Esc — назад.

## Файлы

- `battle_city_test.gd` — вся игра
- `battle_city_test.tscn` — корневая сцена с одним StatusLabel внизу
- `project.godot` — `viewport=1280×900`, главная сцена
- `user://hiscore.cfg` — high score (ConfigFile)
- `user://custom_level.json` — последний уровень из редактора (JSON)

## FX-слой (не канон)

Поверх эталонного геймплея добавлен слой полировки, который **не меняет** ни одно каноническое значение выше:

- Процедурный 8-битный звук (генерируется при старте, без ассетов): выстрел, кирпич/сталь, взрывы, паверапы, джинглы стейджа/game over, блип меню.
- Тряска экрана (взрывы, гибель танка, разрушение базы, wipeout).
- Частицы-искры при попаданиях, вспышки выстрела, трейл у пуль.
- Шторка «STAGE N» при старте уровня (геймплей заморожен на `STAGE_INTRO_TIME`).
- Белая вспышка на броневике при непробитии, синий оттенок врагов при freeze.
- Анимация воды, руины базы после разрушения.

## История фиксов от исходного 24-px проекта

После переезда на `CELL_SIZE = 32` пришлось вычистить три легаси-бага из Phase 0 проекта:

1. `_tank_cell_center` возвращал угол макро вместо центра танка → танк сдвигался на 32 px вверх-влево.
2. `_nearest_lane_center` clamp `lane_count - 1` отрезал крайний lane → игрок на спавн-линии не мог повернуть.
3. Боковые стены базы рисовались 2 destruction-клетки вместо 4.

Состояние после этих фиксов — **эталон**. Любое будущее изменение должно сверяться с этим документом.
