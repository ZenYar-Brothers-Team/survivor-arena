# Balance check: progression / OBS-01, 2026-09-21

Review ID: IP32-OBS01-r1. Health: CONCERNS — неустановленная причина gameplay/readability наблюдения. Решение анализа: **insufficient-evidence / no-change**. Это результат AI-review, не одобрение пользователем исправления.

## Baseline и источники

Один реальный fixture run: [запись и OBS-01](../playtests/2026-09-21_108ff5b3.md), [raw report](../playtests/reports/2026-09-21_108ff5b3/run.json). Report `108ff5b3e8ed457e84704dcbfa25e0f8`, run `aa847240c4c5405f8cdd22ab4caf4f7d`, commit `d81430340488bf6f988ec6d8e37a3c6b0d3a0dea`, dirty=true, config `f11cbc70dae46145df990c79bc897acff02f0316efa92cf223dd4f92a8b1154e`.

Прочитаны snapshots `Content/Enemies/FixtureEnemies`, `Content/ActiveSkills/FixtureActiveSkills` и resolved config внутри report; текущий [FixtureEnemies.json](../../Assets/Resources/Content/Enemies/FixtureEnemies.json). Код: [ExperienceDropFactory](../../Assets/Game/Progression/Runtime/ExperienceDropFactory.cs), [ExperienceDropRuntime](../../Assets/Game/Progression/Runtime/ExperienceDropRuntime.cs), [EnemyExperienceDropSink](../../Assets/Game/Progression/Runtime/EnemyExperienceDropSink.cs), [EnemyFactory](../../Assets/Game/Enemy/Runtime/EnemyFactory.cs), [EnemyRuntime](../../Assets/Game/Enemy/Runtime/EnemyRuntime.cs). Design baseline: [GDD «Опыт и level-up»](../Game_design.md#опыт-и-level-up). FIXTURE IDs не имеют production карточек в Content Design; production значения не выводятся из fixture.

Условия: WindowsEditor, AGILE, BOLT L1, timeline FIXTURE-WAVE-TIMELINE, duration=900 s, draft seed=12345, wave seed=24680; field и persistent profile unsupported. Случайные позиции spawn не покрыты seed; dirty diff целиком в packet не сохранён. Проверка исходников объясняет возможный путь, но не восстанавливает состояние сцены на кадре marker.

## Факты и расчёты

- Пользователь: «опыт стреляет»; marker 2 / sequence 10 / 17.8085632 s: «кажется опыт становится врагом - он стреляем». Ожидание пользователя не уточнено. GDD описывает опыт как pickup в точке смерти, со сбором и expiry; стрельба не описана.
- 17 результатов EnemyProjectile от FAN, 34 applied HP: 17 × 2 HP = 34 HP. Это атрибуция попаданий, не число выпущенных снарядов и не идентификация видимого объекта на 17.81 s.
- BOLT: attempted=124, applied=108, overkill=16 HP; 124 − 108 = 16. Наблюдаемый DPS = 108 HP / 29.9932442 simulation s = 3.60081088 HP/s. Pause 34.1921856 wall s исключена.
- Теоретический BOLT L1 при одном попадании за каждую активацию: 4 HP × 1 / 0.9 s = 4.44444444 HP/s. Это estimate без промахов/overkill/времени поиска цели; отличие от observed не доказывает проблему баланса.
- 10 kills: 8 SEEKER + 2 FAN; каждый fixture reward=1 XP. Dropped=10, ground=10, collected/expired/recovered=0; 10 × 1 = 10 XP. Pickup radius=0.2 world units, но расстояния игрока до drops не записаны. Нулевой collected не доказывает ошибку pickup.

## Гипотеза и альтернативы

Основная проверяемая гипотеза: объект, воспринятый как XP, был FAN-врагом либо перекрывался с ним. XP и FAN fallback используют PlaceholderSprite.Shared; XP имеет cyan tint и scale 0.35, враг red tint и FAN collision scale 0.85. Сходство формы допускает путаницу; без наблюдения кадра это гипотеза низкой уверенности.

Статически XP создаётся отдельным GameObject с ExperienceDropRuntime; enemy и XP pools типизированы отдельно. XP Tick проверяет expiry и pickup, не запускает атаки. Это не воспроизведение бага и не исключение внешнего вмешательства/ошибки сцены. Альтернатива: снаряд уже летел после смерти врага рядом с новым drop; агрегат не содержит per-projectile траекторий, поэтому отличить это нельзя.

Уверенность в арифметике и source attribution отчёта высокая в пределах adapters; в диагнозе низкая. Outliers/dominant options: не оценивались, одного прогона и одной жалобы недостаточно. No design target для предлагаемого изменения damage/pickup radius; новый target не назначается.

## Решение и следующий сценарий

Реальный proposed diff отсутствует; числа, механики и presentation не меняются. Approval: не запрашивался, поскольку review не предлагает patch. Applied diff/rollback: отсутствуют. OBS-01 остаётся открытым.

Следующий ручной диагностический прогон:

1. Сохранить те же fixture config/character/build и записать новый commit/dirty/hash. Изменившиеся условия перечислить; это сравнение сценария, не deterministic replay.
2. При повторении поставить marker и pause. В Editor определить имя и компоненты видимого объекта: Experience Drop / ExperienceDropRuntime или Enemy [FIXTURE-ENEMY-FAN] / EnemyRuntime. Зафиксировать позицию объекта и видимый источник снаряда; сохранить screenshot/video, если доступно.
3. Продолжить и наблюдать, создаются ли новые снаряды тем же объектом, или видны ранее выпущенные. Отдельно проверить pickup при подходе к drop; не менять damage/radius ради диагностики.
4. Сохранить новый report и запись со ссылкой на OBS-01. Если source действительно XP — передать конкретный reproduction владельцу Progression/Enemy; если FAN/перекрытие — оформить presentation-задачу с evidence. Если повторить не удалось, записать это; не закрывать как исправленное.

Этот follow-up ещё не выполнен. Первый review cycle завершён допустимым insufficient-evidence результатом. Accept/apply/rollback проверяются отдельно на [synthetic exercise](../playtests/exercises/IP32/README.md).
