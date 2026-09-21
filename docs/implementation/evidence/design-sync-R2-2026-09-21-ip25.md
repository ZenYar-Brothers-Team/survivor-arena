# IP-25 — Economy/profile verification, 2026-09-21

Scope revision: design-sync-R2. Основание: [DECISION-0037](../../decisions/0037-meta-economy-and-persistence.md).
Текущий execution status хранится только в [STATUS](../STATUS.md#ip-25--persistent-profile-meta-currency-unlocks-и-permanent-progression).

## Implementation

- `Assets/Game/Meta`: validated production/fixture economy catalogs, IProfileService,
  ProfileService, ProfileCodec, IProfileStore/FileProfileStore/MemoryProfileStore,
  explicit migration interface, ProfileAccessProvider, ProfileRunBinding.
- `Assets/Resources/Content/Meta/MetaEconomy.json`: четыре upgrades, 70 explicit
  character/field/skill/set/passive access definitions, reward 5×level и Book 50.
  Production catalog references проверены против полных canonical content IDs.
  `FixtureMetaEconomy.json` отдельно описывает реально доступный runtime roster.
- Main-thread transaction coordinator публикует только committed профиль;
  FileProfileStore выполняет IO через Task.Run, temp+Flush+atomic replace, backup.
  Profile schema v1 сохраняет исходный регистр stable dictionary keys. Известные
  старые версии поддерживаются явными migration steps; initial v1 не выдумывает
  историческую production v0. Synthetic 0→1 проверяет механизм и backup исходника.
- Terminal receipt содержит level reward, ранее начисленную BookCurrency и новые
  unlock IDs. RunId, balance и unlocks записываются одной транзакцией. Duplicate
  result/load/Retry и stale purchase level не повторяют списание/начисление.
- Save failure держит pending immutable-derived transaction, блокирует следующий
  run/покупки, позволяет Retry Save. Purchase failure не меняет профиль.
  Corrupt main восстанавливается из backup с сообщением; reset двух повреждённых
  копий — явный UI intent с сохранением файлов. Неизвестная версия не сбрасывается.
- Composition загружает fixture profile до выбора; default disk file —
  `Application.persistentDataPath/fixture-profile-v1.json`. Явные test/composition
  entry points используют MemoryProfileStore; smoke scene inject выполняется до
  Start, тесты не пишут в пользовательский профиль.
- Character/field roster читают profile access; draft definitions фильтруются по
  profile unlocks. Постоянный source modifier применяется до создания Health,
  нейтральный modifier не добавляется. Покупки разрешены только между run.
- `MetaPresenter` + immutable states + `MetaScreen.uxml`/USS поставляют Result,
  currency, upgrade levels/effects/prices/Buy, condition unlocks, save/error/reset,
  character choice для personal upgrades и immediate Retry same character/field.
  HP/DMG используются как текстовые semantic icon placeholders функциональной
  fixture поверхности; новая растровая графика не создавалась. Полный shell,
  Settings, итоговые art bindings и дополнительный UI polish остаются IP-26/catalog IP.
- Normal application quit сначала завершает run с живыми contributors и ожидает
  уже начатую запись; ошибка best-effort save не запирает выход. Hard-crash
  checkpoints/resume отсутствуют по DECISION-0037.
- UI disposal безопасен при уничтожении independently owned UIDocument раньше
  root: проверки для profile, character и field selection, регрессионные smokes.

## Checks

Runner: `scripts/Test-Unity.ps1`, Unity **6000.6.0f1**, Windows, batchmode/nographics.
Перед каждым запуском свежая проверка Win32_Process: interactive Unity для проекта
не открыт. Финальные runner exit codes: 0/0.

| Scope | Passed / total | Failed | Skipped | XML end time UTC |
|---|---:|---:|---:|---|
| Game.* EditMode | 624 / 624 | 0 | 0 | 2026-09-21 18:41:36 |
| Game.* PlayMode | 18 / 18 | 0 | 0 | 2026-09-21 18:40:55 |

Third-party test cases: 0. Финальная версия C#/JSON/UXML одинакова для обоих прогонов;
после них правились только документы. Копии результатов/логов:
`TestResults/IP25/EditMode.xml`, `EditMode.log`, `PlayMode.xml`, `PlayMode.log`
(локальные generated artifacts, не подмена repository evidence).

Coverage:
- L1 Quit/Retry/Error=5, unstarted=0; L20+2 Books=200; duplicate/reload receipt;
- pause time исключён, смерть до deadline не превращается в victory; shortened
  fixture victory не открывает production field; вся последовательность fields,
  бесплатные character/skill/set unlocks и четыре покупки персонажей;
- insufficient/locked/duplicate/in-flight purchase, caps, global+personal health
  и damage, цена пяти уровней, сохранение уровней/доступа после load;
- pending result failure/retry, purchase rollback, corrupt main/backup/reset,
  unknown old/future version без overwrite, explicit migration, incomplete result;
- real file-store atomic-write/backup integration в уникальном temporary directory;
- fake-view presenter/condition/no fake price, semantic UXML IDs;
- real UI Result→Buy→Retry, новый RunId при тех же character/field, ровно один
  reward и полный увеличенный HP следующего run;
- profile/character/field view destroyed-before-owner teardown;
- остальные critical paths исходного suite: lifecycle, damage/death, XP/draft,
  active skills/sets, waves/spawn/pools, composition, content, presenters, gameplay smoke.

Дополнительно: вся Content JSON syntax, 70 production IDs, asset metas и
`git diff --check` проверены. Это automated layout/interaction evidence; отдельного
ручного художественного review нового UI не заявляется.

## Documentation impact и limits

IP-25 API/schema/persistence/reset contracts, consumer IP-26 и regression map
синхронизированы. Утверждённая экономика не менялась. FixtureRunSetup Book amount
синхронизирован с Meta catalog на 50; прямые synthetic тесты могут задавать иные суммы.
Production gameplay definitions и изображения не поставляются этим profile packet.
Полный G-16 settings/audio/shake и G-20 production difficulty остаются отдельными gates.
Pre-existing изменение Packages/manifest.json не редактировалось этой задачей.
