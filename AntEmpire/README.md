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
- **Ants (прототип рабочего муравья, этап 4)** — `AntStateMachine`
  (Idle → SearchResource → MoveToResource → CollectResource → ReturnToNest →
  DepositResource, чистый C#), `AntController` (движение, реализация
  `IAntAgent`), `WorkerAntCollector` («рюкзак» муравья), `AntTypeData`
  (ScriptableObject-конфиг: скорость, вместимость, время сбора),
  `AntSpawner` (восстановление популяции из сейва + `HatchWorker()` для
  будущего инкубатора), `PlaceholderAntVisual` (муравей из примитивов).
- **World** — `ResourceNode` (узлы ресурсов с истощением, визуальным
  уменьшением и восстановлением; поиск ближайшего без физики),
  `NestEntrance` (вход в гнездо).
- **Rooms (этап 6)** — `RoomTypeData` (ScriptableObject-конфиг: стоимость,
  рост цены, макс. уровень), `RoomService` (уровни в сейве, атомарные
  улучшения, формулы эффектов: лимит населения, вместимость склада,
  скорость рождения). Комнаты: Queen Chamber, Food Storage, Larva Nursery.
- **Queen (колония растёт сама)** — `QueenController`: после постройки
  Larva Nursery королева периодически выводит новых рабочих за еду, пока
  не упрётся в лимит населения. Склад еды ограничивает запас (излишки
  теряются — стимул улучшать Food Storage).
- **Sandbox** — `PrototypeSandbox`: собирает тестовую сцену из примитивов
  одним компонентом (земля, гнездо, королева, узлы ресурсов, муравьи,
  камера, свет, HUD). HUD показывает ресурсы, население, статус королевы
  и кнопки постройки/улучшения комнат.
- **Workforce** — распределение рабочих по ресурсам (кнопки +/− в HUD),
  назначения сохраняются; узлы ресурсов после истощения отрастают в новом
  случайном месте.
- **Combat + Events** — атаки паука (масштабируется с колонией: HP и
  награда DNA), муравьи-защитники сбегаются в ближний бой, паук убивает
  рабочих; полоска HP, вспышки урона, анимация смерти, всплывающие тексты.
- **Evolution** — 5 уровней рабочих за Food+DNA с видимой трансформацией
  (размер → челюсти → броня → золотое свечение) и ростом статов.
- **Underground «ферма»** — вид муравейника в разрезе с живыми камерами
  комнат (переключение кнопкой в HUD).
- **Offline progress** — колония собирает ресурсы, пока игра закрыта
  (60% эффективности, кап 8 часов, еда ограничена складом); попап
  «While you were away…» с кнопкой Collect.

## Как запустить прототип (муравьи уже бегают!)

1. Открой проект в Unity (см. ниже).
2. **File → New Scene** (пустая сцена, можно без сохранения).
3. Создай пустой GameObject: **GameObject → Create Empty**.
4. Добавь ему компонент **PrototypeSandbox** (кнопка Add Component → набери
   `PrototypeSandbox`).
5. Нажми **Play** ▶ — 3 муравья побегут к листьям и еде, принесут ресурсы в
   гнездо, счётчики Food/Leaves вырастут. Останови и запусти Play снова —
   прогресс сохранится (JSON-сейв в `persistentDataPath`).

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
