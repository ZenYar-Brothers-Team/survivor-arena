# DECISION-0038 — Настройки, camera shake и сложность 1–5

Status: Approved (явный выбор шкалы и поручение самостоятельно выбрать остальные правила)
Date: 2026-09-21
Related IP: IP-02, IP-12A, IP-16, IP-23, IP-26, IP-29

## Approval boundary

Пользователь: «сложность будет 1-5, в остальном выбирай между классикой для такой
игры и простой реализацией». Конкретные defaults, mapping и policy ниже выбраны
в пределах этого поручения. Это подготовка IP-26; реализация не начинается
автоматически. G-16/G-20 получают полные правила, без нового approval loop.

## Сложность поля

Единая целочисленная display difficulty 1…5. Explicit mapping в карточках CD:
FIELD-001/002 → 1; FIELD-003/004 → 2; FIELD-005/006 → 3;
FIELD-007/008 → 4; FIELD-009/010 → 5. Объединены соседние ступени прежней оценки,
с сохранением порядка десяти полей. Runtime читает authored metadata, не вычисляет
оценку из номера ID. Это UI-рейтинг, не множитель характеристик/награды.
Отдельный Traveler progression rank 1…10 из DECISION-0035 и его scaling сохранены.
Текущие fixture difficulty 2/1 остаются самостоятельными synthetic данными.

## Settings ownership и defaults

Один app-scoped settings service и отдельный versioned settings JSON, без связи
с прогрессом/валютой. Settings доступны из Main Menu и Pause, возвращают на исходный
экран; открытие из Pause не снимает его pause ownership. Remap/localization позже;
Controls сейчас показывают фактически действующие movement bindings.

Defaults: Master 80%, Music 60%, SFX 80%, Screen Shake On. Video: borderless fullscreen
в текущем desktop resolution; второй режим — обычное Windowed. Exclusive fullscreen,
мониторы, refresh-rate selector и дополнительные quality switches не добавляются.
Defaults, video confirmation timeout и shake numbers поставляются JSON в IP-26,
не полями MonoBehaviour с незаметными tuning defaults.

Громкость и Shake применяются сразу. Запись один раз при Back/закрытии Settings,
также при штатном выходе, если есть dirty settings; не на каждом движении slider.
Внутри меню нет Cancel для этих мгновенных параметров. При ошибке записи текущие
значения продолжают работать в памяти, показывается «Не удалось сохранить настройки»
и Retry Save; игра/навигация не блокируются. Повтор сохраняет последнюю выбранную
версию, а не устаревший snapshot. IO вне игрового потока, записи сериализованы.

Отсутствующий файл создаёт defaults. Повреждённый, неполный или неизвестной версии
файл сохраняется с diagnostic suffix; применяются defaults и показывается сообщение.
Если сохранить исходник не удалось, автоматическая перезапись запрещена; остаются
session defaults и ошибка. Устаревшая неподдерживаемая версия трактуется так же;
сложная миграция preferences не нужна. Все значения валидируются: finite volume
0…1, bool shake, положительные width/height и один из двух режимов. Invalid целый
документ заменяется defaults; профиль IP-25 не сбрасывается и не переписывается.

## Видео: Apply / Keep / Revert

Dropdown показывает уникальные поддерживаемые разрешения для окна. В borderless
resolution read-only, равен текущему desktop mode. Первый старт использует desktop
borderless; при невозможности — безопасное окно 1280×720, уменьшенное до доступной
рабочей области экрана с сохранением пропорций. Фактически применённый размер
показывается в UI. Saved mode повторно проверяется при старте; если он уже недоступен,
применяется этот fallback с уведомлением.

Изменение video controls создаёт candidate. Apply временно применяет его; Keep
подтверждает, Revert/Escape либо отсутствие ответа 10 секунд возвращает предыдущий
confirmed mode. Таймер использует real/unscaled time и продолжает идти без фокуса,
не зависит от gameplay pause. До Keep candidate никогда не записывается на диск.
Keep сохраняет snapshot с последними audio/shake и подтверждённым video; ошибка
записи даёт общий Retry Save. Закрытие Settings до Apply отбрасывает video candidate,
закрытие при pending confirmation сначала выполняет Revert. Новый Apply недоступен
до решения текущего confirmation. Если применение не удалось — rollback + сообщение;
если прежний mode тоже недоступен — safe window fallback. Preview state не переживает
перезапуск приложения: загружается последний успешно сохранённый confirmed mode.

## Звук

Master применяется один раз к общему выходу; Music и SFX — к соответствующим
источникам. UI sounds относятся к SFX. Для channel c:
`outputGain = master × channelVolume × sourceGain`, каждое значение в [0,1],
безразмерная линейная амплитуда. Например Master=.8, SFX=.8, sourceGain=1 → .64;
Music=.6 при том же Master → .48. Нулевой channel действительно mute; Master=0
глушит оба. Без дополнительных dB-кривых, ducking и mixer effects в первой версии.

Две явные Test Music / Test SFX кнопки воспроизводят короткие preview clips через
тот же service routing, без обхода Master. Для функциональной проверки допускаются
простые synthetic audio fixtures; полноценный саундтрек/SFX library не требуются.
Preview по одному на канал, повтор перезапускает его, закрытие Settings останавливает
preview. Settings preview работает на паузе; gameplay SFX на паузе не запускаются,
уже звучащие gameplay SFX ставятся на паузу и продолжаются при Resume. Music продолжает
звучать на паузе. End run останавливает gameplay SFX; UI/preview имеют отдельный lifetime.
Наличие пустого production music catalog не делает Music control декоративным:
рабочий routing проверяется preview и fake/real audio checks.

## Camera shake

Default On. Единственный начальный trigger — фактически полученный игроком урон >0
пока run Running. Blocked/нулевой damage, чужие попадания и урон после terminal не
вызывают shake. Если удар завершил run, terminal cleanup имеет приоритет.

Baseline camera follow точно центрирован без задержки. Временный visual XY offset
накладывается отдельно после вычисления центра; он не меняет player/physics, follow
anchor, spawn/visibility queries или координаты интерфейса. Это явное визуальное
исключение к правилу центрирования, не задержка следования камеры.

Amplitude A = 0.005 × H, где H >0 — полная высота базового orthographic viewport в
world units; duration D=0.12 s, частота 25 Hz. При H=10 A=0.05 world units.
Envelope `a(t)=A×(1−t/D)` для t∈[0,D], далее 0; длина XY offset ≤a(t).
Простое ограниченное колебание без gameplay RNG, без rotation/zoom. Повторный hit
перезапускает один shake; эффекты не складываются. Pause, Shake Off, terminal,
смена run/сцены и отключение consumer немедленно обнуляют offset и оставшееся время;
после Resume прежняя тряска не возвращается.

Числа настраиваемы через JSON: amplitude fraction 0…0.01 от H (сила), duration
0.05…0.25 s (длительность), frequency 10…40 Hz (темп); начальные .005/.12/25.
IP-26 consumer использует IScreenShakePreference/ScreenShakeRequestGate IP-12A;
настройки живут в service, visual offset — у camera presentation consumer.

## Verification / consequences

Синхронизированы GDD camera rule, CD поля, UI §18, IP-16/23/26 и STATUS/DESIGN_SYNC.
Код/config/assets этим решением не меняются. Для реализации IP-26 проверить:
- defaults/load/invalid/future settings, preserve failure, save failure/latest retry;
- Master/Music/SFX isolation, zero mute, preview/playback pause/end cleanup;
- video candidate/Keep/Revert, 10-second real-time timeout, unsupported/fallback,
  exit from pending preview и load последнего confirmed mode;
- shake actual-damage gate, bound/envelope, repetition, Off/pause/end/scene reset,
  centered baseline и независимость gameplay queries;
- explicit 1…5 production difficulty, отсутствие связи с rank/scaling;
- полный player flow, full Game.* EditMode/PlayMode и обязательный ручной
  1920×1080/video/audio/shake review из IP-26. Исторические tests этого не доказывают.

## Пользовательская приёмка — 2026-09-21

После ручного отзыва «проверил, всё в порядке», исправления замечания о кнопках
после поражения и отчёта об automated regression пользователь явно поручил:
«ставь верифайд и комить». Это явная приёмка functional packet и разрешение закрыть
оставшиеся manual acceptance gates без отдельного подтверждения разрешения
1920×1080 и нового ручного повтора поражения. Новые измерения или прогоны этим
решением не утверждаются. Основание — существующие 637/637 EditMode, 22/22 PlayMode,
пользовательский отзыв и исправление с failing-before/passing-after regression.
Production content/art и IP-12A gameplay density review этим решением не закрываются.
