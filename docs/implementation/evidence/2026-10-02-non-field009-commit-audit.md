# Академия и Монастырь — аудит перед коммитом

Поручение пользователя 2026-10-02: сохранить полезные незакоммиченные изменения,
исключив всю незавершённую работу FIELD-009. Проверены состав изменений,
authoring/generated соответствие, art provenance и метаданные Unity.

В scope входят зоны/порталы FIELD-006, altar feedback/мини-карта/контакт и
full-resolution ground import FIELD-007, связанные тесты, документы и исходники.
Исправления двух повреждённых `.meta` сохраняют исходные GUID, удаляя случайно
приписанный текст git diff. Новый `.meta` существующего DraftActiveSkillGuaranteeTests
сохраняется вместе с проектом. Выбранные арт-кандидаты и явно сохранённая
альтернатива Академии имеют самостоятельную provenance/review ценность.
Незавершённые материалы FIELD-009 и перенос его блока PROJECT_MAP исключены.

## Свежие проверки

- Content generation: UP TO DATE.
- Audio integrity: PASS, 28 files / 15 cues.
- Art manifest: PASS, 317 owner/role records (общий manifest, включая ранее
  закоммиченные записи FIELD-009; его рабочие изменения не входят в commit scope).
- Art pipeline PLAN для печати и портала FIELD-006: PASS, changedFiles=0.
- Ground FIELD-007: pipeline повторно подтверждает документированный отказ
  менять существующий import profile; это отдельная reviewed техническая правка,
  а не новый raster. Пакет сохранён как evidence выполненного PLAN.
- Full graphics runner: тесты Unity НЕ ЗАПУЩЕНЫ; подключение к локальному
  серверу редактора 127.0.0.1:8090 отклонено (WinError 10061). Повтор с разрешением
  на process/network access дал ту же ошибку. Evidence попыток:
  `TestResults/checks/20261002T160649-448258Z/` и
  `TestResults/checks/20261002T160740-231324Z/`.

Исторические scoped результаты в evidence Академии/Монастыря не являются
новым PASS этой объединённой версии. Полный runtime PASS и пользовательская
visual acceptance не заявляются; текущие execution статусы — только STATUS.
