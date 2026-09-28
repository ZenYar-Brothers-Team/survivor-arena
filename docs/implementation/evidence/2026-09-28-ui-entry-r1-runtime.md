# UI entry R1 — Unity evidence, 2026-09-28

## Итоговый прогон после завершения фидбека

Пользователь: «Отлично», затем «Все, идем дальше» — feedback завершён, проверки
возобновлены. `python scripts/check_project.py --scope full --graphics`: PASS,
Unity 6000.6.0f1, безопасный batch при закрытом Editor. EditMode **928/928**,
PlayMode **35/35**, failed/skipped **0**, third-party **0**; generation UP TO DATE,
audio 28 files / 15 cues PASS, art **256** PASS.
Receipt: `TestResults/checks/20260928T201401-014729Z/summary.json`.

В прогон вошли все последующие дельты ниже: диагональный медленный дрейф/волны,
ray speed, отсутствие информации закрытых героев, пять мечей сложности,
10-field/no-scroll и 20-field/scroll, обе геометрии. Обновлённые captures в
TestResults просмотрены; статические изображения не измеряют ощущение движения.
Все критические gameplay-пути покрыты полным набором, не только UI filter.
Старые NOT RUN и просьба отложить тесты ниже — хронология, а не текущая блокировка.

## Последующие маркеры сложности

По отзыву пользователя Field Select заменяет надпись N/5 на пять небольших мечей:
N заполненных золотистых, остальные контурные приглушённые. Значение читается из
существующего Difficulty; никакой правки данных полей. Значки построены обычными
UITK/USS shapes без шрифтовых emoji и новых raster assets. Проверки пяти слотов,
заполнения FIELD-001/003 и отсутствия дроби добавлены в UiEntrySmokeTests;
запуск отложен по прямому поручению пользователя. Предыдущие screenshots/PASS
ниже не подтверждают эту дельту.

## Последующее скрытие сведений закрытого героя

По уточнению пользователя до получения героя Character Select показывает только
силуэт и «?». Карточки не выдают имя/статус; большой просмотр скрывает панель
сведений и очищает её значения/иконку; footer не выдаёт имя. После выбора доступного
героя обычный вид восстанавливается. Meta/purchase rules не менялись.
`UiEntrySmokeTests` дополнен проверками отсутствия имени/деталей и восстановления
обычного вида. Эти изменения не проверялись запуском: пользователь временно
запретил тесты до окончания фидбека. Старые captures/PASS ниже предшествуют правке.

## Последующая корректировка движения

Следующее уточнение пользователя: основная линия не вертикальная. Основной дрейф заменён на `dustDriftX=0.38`, `dustDriftY=-0.12` долей экрана за цикл (вправо и немного вверх); частицы распределены по высоте перед обоими героями. Волны и время цикла сохранены. Допустимо |X|=0.05–1 и Y=-0.5–0.5. Добавлен `DustMainDrift_IsSidewaysWithGentleRise`; тесты не запускались по явному поручению пользователя. Это уточнение заменяет описанный ниже вертикальный подъём.

По отзыву пользователя свет замедлен до 70% прежней скорости, основной подъём пыли — до 20% (цикл 55–85 с вместо 11–17 с). У каждой частицы смешаны две боковые волны с различными фазами/частотами и небольшое вертикальное покачивание; общий подъём и слабый боковой дрейф сохранены. Нет случайных скачков координат между кадрами и обращения к глобальному RNG. Размеры, количество, прозрачность, исходный арт и parallax не менялись.

`MenuArtProfile.json`: `raySpeed` / `dustSpeed` — множители времени, допустимо 0.05–2; `dustWander` — доля ширины экрана, 0–0.05 (выбрано 0.018); `dustWanderSeconds` — базовый период волн, 10–120 с (выбрано 36). Все значения валидируются. При скрытом меню часы по-прежнему стоят.

Добавлены tests `DustMotion_IsSlowerCurvedAndDifferentBetweenParticles` и `Rays_AreThirtyPercentSlowerWithoutChangingAmplitude`. Попытка scoped EditMode через safe runner: NOT RUN, REST connection refused / WinError 10061 при открытом интерактивном Editor. Его не закрывали, batch параллельно не запускали. Результаты полного прогона ниже относятся к предыдущей версии. Раскладка и raster imports не изменялись; art packet повторно не применялся.

Scope: Main Menu, Character Select, Field Select. Основание: принятый browser R1 / Menu E v002 и поручение «давай». HUD/Draft/Pause не переоформлялись; новые production definitions не добавлялись. Execution status и дальнейший порядок — только [STATUS](../STATUS.md).

## Изменения и границы

- Локальная `EntryStyles.uss`: матовая сливовая поверхность, тёплый текст, терракотовое primary action, hover/pressed/focus/selected/disabled. Settings/Meta/Results сохраняют прежнюю тему. DEV в меню свёрнут по умолчанию.
- Герои: прокручиваемый roster, крупный просмотр, иконка стартового умения, краткие особенности; неполученные body — одноцветные силуэты в обоих местах. Просмотр закрытого героя не меняет допустимый session selection и отключает подтверждение. Сам запуск повторно проверяет access.
- Поля: thumbnail, имя, сложность, lock state; описание окружения не выводится. Причина блокировки под сеткой, выбранный герой и действия в footer. Runtime показывает только FIELD-001…003; десять/двадцать карточек проверены отдельно fixture harness, а не выдуманными production fields.
- Menu E: два full-canvas спрайта по approved packet, отдельные слои лучей/пыли, pointer parallax и медленный drift. UI-кнопки не двигаются; часы декорации останавливаются при скрытии меню/потере фокуса. Это Unity 2D-смещения, не CSS 3D perspective. Точную выразительность движения оценивает пользователь в игре.
- `Art/Packets/ui-entry-r1.json` применён через art pipeline. RGB backplate получает только непрозрачный alpha; foreground копируется byte-identical. Оба исходных approved master сохранены. Import — Single FullRect, без автоматической нарезки героев.

## Финальные проверки

`python scripts/check_project.py --scope full --graphics`

- Unity 6000.6.0f1; safe runner → закрытый Editor, batch с graphics.
- EditMode: **925/925**, PlayMode: **35/35**, failed/skipped: **0**, third-party: **0**.
- Generation UP TO DATE; audio 28 files / 15 cues PASS; art 256 owner/role records PASS.
- Receipt: `TestResults/checks/20260928T194538-272877Z/summary.json`, XML и логи рядом.
- Критические пути входят в полный прогон: run lifecycle, damage/death, XP/draft, active skills, wave/spawn/pooling, content/composition и UI presenters.
- Python workflow tools: 25 tests PASS; в том числе RGB→RGBA без изменения цвета/размеров и отказ RGB для обычного copy.
- `git diff --check` без ошибок whitespace.

Новый `UiEntrySmokeTests` проверяет 1920×1080 и 1280×720, доступность fixed footer, inspection закрытого героя, отдельный confirm, сетку десяти полей без scroll и двадцати со scroll. `MenuArtProfileTests` проверяет профиль/невалидные числа; `SpriteAssetImportTests` — сохранение целого canvas у обоих menu layers.

## Снимки Unity

В `TestResults/` (ignored generated evidence) сохранены пары 1920×1080 / 1280×720:

- `ui-entry-menu-<resolution>.png`;
- `ui-entry-character-<resolution>.png`;
- `ui-entry-locked-<resolution>.png`;
- `ui-entry-fields-<resolution>.png`;
- `ui-entry-ten-fields-<resolution>.png` (fixture layout, намеренно без production thumbnails).

Снимки просмотрены агентом. Тестовые render targets используют depth/stencil 24 для корректного rounded clipping. Character captures показывают собственную панель; Back рисуется отдельным AppShell в игре, поэтому не входит в isolated-panel capture.

## Исправления, найденные проверками

- Добавлена недостающая assembly-ссылка UI на существующий Game.Content.Json.
- UI background role разрешён импортным каталогом; import version обновлена, чтобы старый автоматический multiple-sprite import не обрезал Шепотку.
- Первые успешные scoped тесты после переимпорта дали INCOMPLETE из-за изменения import metadata; они не выдаются за финальный PASS.
- Первый полный прогон: 925/925 + 34/35, единственный fail — 21 px scroll в fixture grid. Высота карточек скорректирована; targeted повтор и указанный выше финальный полный прогон PASS.

Пользовательская visual acceptance Unity не подменяется тестами. Предсуществовавший `Packages/packages-lock.json` от установки Unity Pipeline не откатывался и не включался в отдельный commit; commits/push в этом шаге не выполнялись.
