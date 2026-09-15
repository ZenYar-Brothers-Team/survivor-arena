# DECISION-0009 — Контент сущностей в JSON вместо хардкода

Status: Approved

Date: 2026-09-15

Related IP: IP-03, IP-04, IP-08, IP-09, IP-12; правило применяется ко всем будущим production IP (IP-17…IP-23)

Related content IDs: FIXTURE-SKILL-*, FIXTURE-ENEMY-SEEKER, FIXTURE-PASSIVE-*, FIXTURE-CHARACTER-* — все как Draft/fixture, не production

## Context

Все текущие значения баланса (характеристики врагов, базовые статы персонажа, уровни пассивок, полные деревья эффектов активных умений) были захардкожены как C#-литералы в статических `Fixture*Catalog`-классах или как `[SerializeField]`-поля по умолчанию на `PlayerCharacterRuntime`. Пользователь отметил, что для будущей автоматической балансировки (AI-прогонятор игры, выравнивающий баланс умений/мобов) нужно единое место конфигурации, доступное как обычные файлы, а не значения внутри кода.

## Decision

- Новый модуль `Game.Content.Json` с `JsonContentFile` — общая точка загрузки JSON-ресурсов (`Resources.Load<TextAsset>` + Newtonsoft.Json, enum как строки).
- Добавлен пакет `com.unity.nuget.newtonsoft-json` (родное решение Unity для JSON, `JsonUtility` не умеет в полиморфизм).
- Сами JSON-файлы физически собраны в одном общем дереве `Assets/Resources/Content/<Категория>/*.json` (не разбросаны по модулям) — так их видно одним местом для правки/аудита/будущего AI-балансировщика. Код загрузки (DTO, конвертеры, `Fixture*Catalog`) при этом остаётся в модуле, которому принадлежит соответствующий доменный тип — так asmdef-границы между Game.Enemy/Game.Character/Game.Progression/Game.ActiveSkill не размываются, и не появляется один большой класс, знающий про все типы контента сразу. `Resources.Load` не зависит от физического расположения файла, только от логического пути внутри любой папки `Resources/`, поэтому объединение файлов — чисто механическое изменение путей, а не архитектурное.
  - `Assets/Game/Enemy/Model/FixtureEnemyCatalog.cs` → `Resources/Content/Enemies/FixtureEnemies.json`
  - `Assets/Game/Character/Model/FixtureCharacterCatalog.cs` и `Assets/Game/Progression/Character/FixtureCharacterDefinitionCatalog.cs` → `Resources/Content/Characters/FixtureCharacters.json`
  - `Assets/Game/Progression/Passive/FixturePassiveCatalog.cs` → `Resources/Content/Passives/FixturePassives.json`
  - `Assets/Game/ActiveSkill/Progression/FixtureActiveSkillCatalog.cs` → `Resources/Content/ActiveSkills/FixtureActiveSkills.json`
- Полиморфизм 7 типов эффектов активных умений (`ProjectileBurstEffect`, `BeamEffect`, `OrbitEffect`, `BoomerangEffect`, `ChainEffect`, `AreaEffect`, `MineEffect`) решён через `"kind"`-дискриминатор и `ActiveSkillEffectJsonConverter` — паттерн, который стоит переиспользовать для любого будущего полиморфного контента.
- `PlayerCharacterRuntime` больше не хранит базовые статы как `[SerializeField]`; composition root передаёт ему статы выбранного определения, загруженного из character JSON. Сцена `Gameplay.unity` не содержала кастомных (отличных от дефолтных) значений в удалённых полях — проверено перед их удалением, потери данных нет.
- IP-12 расширил тот же character JSON от одного профиля базовых статов до двух полных fixture-определений: display name, unlock flag, starting active skill, base stats и per-skill draft weights. Значения остаются fixture-конфигурацией и не утверждают Draft CHAR-001…010 как production content.
- Все значения в JSON воспроизводят прежние хардкоженные (включая формулы `level * X` для пассивок, развёрнутые в явные 6 значений на уровень) — проверено полным прогоном существующего test suite (142/142 EditMode, включая точные assertions на уровни 1→6 для всех 8 умений), а не только компиляцией.
- Правило для будущего контента записано в `AGENTS.md` (раздел «Coding conventions»).

## Consequences

- Game Design и Content Design не затронуты — балансные значения и так были определены как Draft/TBD, изменился только механизм их хранения.
- IP-03/IP-04/IP-08/IP-09 evidence: источник конфигурации сменился с C#-кода на JSON-ресурсы; поведение идентично, подтверждено полным прогоном тестов.
- Два теста (`PlayerCharacterSceneIntegrationTests`, `Game.Movement.Tests.GameplaySceneIntegrationTests`) переписаны: раньше они через `SerializedObject`-рефлексию проверяли значения `[SerializeField]`-полей, которых больше нет — теперь проверяют результат `Stats`/`FixtureCharacterCatalog` после явного вызова `Awake()`.
- Будущие production-каталоги (IP-17…IP-23) наследуют тот же паттерн: регистрируют реальный JSON вместо `Fixture*`, доменные типы и валидация не меняются.
- Не сделано в этом решении: сам «AI-прогонятор баланса» не реализован — только формат данных, которым он сможет пользоваться.

## Approval

Пользователь явно утвердил 2026-09-15: «нужно настройки сущностей сделать в json, добавить правило что это не надо добавлять хардкодом... вынести все значения в конфиги... в будущем это также будет использоваться для корректировки значений автоматическим ИИ-прогонятором игры» и выбрал «Сразу все типы контента» + «Да, добавить пакет [Newtonsoft.Json]» при уточнении.
