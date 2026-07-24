# 🐜 Ant Empire: Rise of the Queen

Мобильная 3D idle-игра про муравьиную колонию (Unity, Android → iOS).

Игрок начинает с одной королевы и пары рабочих муравьёв, собирает ресурсы,
строит комнаты муравейника, эволюционирует муравьёв через 5 визуальных
стадий и защищает колонию от врагов. Игра спроектирована вокруг удержания
(retention): видимый рост колонии, оффлайн-прогресс, ежедневные и случайные
события.

## Как открыть проект

1. Установи [Unity Hub](https://unity.com/download) и Unity 6 (6000.0 LTS
   или новее) с модулем **Android Build Support**.
2. В Unity Hub: **Add → Add project from disk** → выбери папку `AntEmpire`.
3. Открой проект — Unity сама сгенерирует `Library/` и недостающие настройки.

## Что уже реализовано (v0.0.1 — архитектурный фундамент)

- `CLAUDE.md` — полный контекст проекта для Claude Code (концепт, MVP-скоуп,
  правила архитектуры и кодинга).
- **SaveSystem** — `SaveData` (JSON-совместимый снимок прогресса) и
  `SaveManager` (загрузка/сохранение в `persistentDataPath`, автосейв,
  защита от повреждённых файлов, атомарная запись).
- **Economy** — `ResourceType` (Food / Leaves / Soil / DNA), `ResourceWallet`,
  `ResourceManager` с событиями для UI и атомарными мульти-ресурсными
  тратами.
- **Core** — `GameManager`: bootstrap сервисов, автосейв-таймер, сохранение
  при сворачивании приложения.
- Заглушки с TODO: `AntController`, `AntStateMachine`, `WorkerAntCollector`,
  `QueenController`, `RoomController`, `OfflineProgressService`.

## Дорожная карта (из дизайн-документа)

| Версия | Содержимое |
|--------|-----------|
| 0.1 Prototype | рабочие муравьи, сбор Food/Leaves, вход в гнездо, save/load, базовый UI |
| 0.2 Colony | королева, население, комнаты (Queen Chamber, Food Storage, Larva Nursery) |
| 0.3 Evolution | 5 уровней рабочего, визуальная трансформация, DNA, Evolution Chamber |
| 0.4 Events | Spider Attack, Golden Leaf, дождь, оффлайн-награды, ежедневная награда |
| 0.5 Soft Launch | Android-билд, аналитика, rewarded ads, баланс, тест retention |

## Структура

```
AntEmpire/
  CLAUDE.md                  ← контекст для Claude Code
  Assets/_Project/
    Scripts/
      Core/                  ← GameManager (bootstrap)
      SaveSystem/            ← SaveData, SaveManager
      Economy/               ← ResourceType, ResourceWallet, ResourceManager
      Ants/                  ← state machine рабочего муравья (TODO)
      Colony/                ← королева (TODO)
      Rooms/                 ← комнаты муравейника (TODO)
      OfflineProgress/       ← оффлайн-прогресс (TODO)
    Scenes/                  ← Boot, MainMenu, Underground, Surface
    Prefabs/  Art/  ScriptableObjects/
  Packages/manifest.json
  ProjectSettings/
```
