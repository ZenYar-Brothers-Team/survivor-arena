# Согласование параллельных изменений — 2026-09-30

Сведены изменения баланса, арта, волн, slow presentation и интерфейса из трёх
одновременных сессий. Утверждённые решения и authoring sources сохранены.

## Исправленные расхождения

- FIELD-001: проверка и обзорная таблица ENEMY-001 теперь отражают скорость
  1.056 из DECISION-0099, а не исторические 0.96.
- FIELD-002 review packet: порог телепорта BOSS-002 согласован с общей
  величиной 7.1875 по DECISION-0111. Stress count FIELD-001 поднят с 250
  до нового общего предела 300 по DECISION-0115.
- Тест вкладки «Открытия» принимает как обычный sprite, так и размытую
  texture закрытой карты по DECISION-0118. Не подключённый к UI panel
  EditMode-тест синтетического PointerEnter удалён: без panel событие не
  доставляется; PlayMode UI-сценарии остаются в полном smoke.
- DECISION-0112 и UI/UX синхронизированы с более поздним DECISION-0118:
  карточки Draft/Book выбираются мышью, клавиатурная активация карточек
  и фокусное переключение просмотра отменены.
- Из `OPEN_ISSUES.md` по просьбе пользователя удалены GI-02…06 и GI-08…10.
  Ссылки на удалённые разделы заменены ссылками на решения и исходное
  наблюдение. В реестре остаются GI-01 и GI-07; по новому запросу добавлена
  GI-11 о локализациях на разные языки.

## Проверки

- `scripts/content/generate.py --check`: UP TO DATE.
- Все шесть balance validators: PASS (FIELD-001, FIELD-002, FIELD-003,
  field rhythm, characters, bosses); для Unicode-отчётов использован UTF-8 stdout.
- `python -m unittest discover -s scripts/tests -v`: 26/26 PASS.
- Art packet SKILL-008 повторно проверен: `changedFiles=0`.
- Full graphics `python scripts/check_project.py --scope full --graphics`:
  Unity 6000.6.0f1, 1101/1101 EditMode, 59/59 PlayMode, 0 failed/skipped;
  generation/audio PASS, art manifest 270/270 PASS.
  Receipt: `TestResults/checks/20260930T115751-305090Z/summary.json`.

Ручная оценка новых размеров, диска, slow overlay и раскладки UI остаётся
отдельным игровым просмотром; автоматические проверки не заменяют её.
