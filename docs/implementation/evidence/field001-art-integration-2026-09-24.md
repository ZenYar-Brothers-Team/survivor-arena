# FIELD-001 — интеграция изображений 2026-09-24

Scope: арт для стартового FIELD-001, F1-04/06/07/08. Execution status — только
[STATUS](../STATUS.md#field001-execution).

## Источники и подготовка

- Пользователь передал два [ChatGPT image-share post 1](https://chatgpt.com/s/m_6ab55b496d088191acd1d8fc36dbf88d),
  [post 2](https://chatgpt.com/s/m_6ab55b75922c8191a1ab8dd2575012c9).
  Из них получены восемь body PNG и один FIELD-001 thumbnail. Полные исходные
  PNG без изменений сохранены как `original-generated.png` в каждом source role;
  SHA-256 и ссылка на post указаны в соответствующем `asset-record.json`.
- В этих share posts доступны изображения, но не внутренние тексты generation
  prompts. Поле `prompt` у девяти полученных изображений явно фиксирует эту
  недоступность; prompts не реконструировались задним числом. Это ограничение
  provenance относительно требования [ASSET_PIPELINE](../../art/ASSET_PIPELINE.md)
  хранить точный prompt.
- По отдельному поручению пользователя сгенерированы отсутствовавшие body
  `ENEMY-007` и projectile `BOSS-001`. Их полные prompts, исходные PNG и hashes
  сохранены в [hound record](../../../Art/Source/Enemies/enemy-007/body/asset-record.json)
  и [projectile record](../../../Art/Source/Bosses/boss-001/projectile/asset-record.json).
- Техническая подготовка сделала body derivatives 256×256 для обычных врагов и
  Путников, 512×512 для boss/mid-boss; thumbnail 512×341. Исходные изображения
  сохранены рядом с derivatives. Для каждого body вычислен максимально вписанный
  contact circle через `scripts/fit-body-contacts.py`.

## Подключённые роли

| Контент | Runtime role | Источник |
|---|---|---|
| ENEMY-003/004/005 | body | post 1 |
| ENEMY-007 | body | новая генерация |
| MIDBOSS-001, BOSS-001 | body | post 1 |
| BOSS-001 | projectile для fan и ring | новая генерация |
| TRAVELER-001/002/005 | body | post 1 |
| FIELD-001 | background thumbnail для Field Select | post 2 |

Все 11 owner/role импортированы через `scripts/art_pipeline.py` в
`Art/Source`, `Art/asset-manifest.json`, `Art/ImportProfiles.json`,
`Assets/Resources/Art/Sprites` и `FixtureSprites.json`. Production JSON
перегенерирован из baseline через `scripts/generate_field001_content.py`.
Typed references подключают тела, один общий boss projectile к двум атакам и
thumbnail к Field Select. Для полей без thumbnail остаётся текстовый placeholder.

## Проверка

| Проверка | Результат |
|---|---|
| `scripts/art_pipeline.py <packet> --apply` | PASS, 11 owner/role |
| `python scripts/validate-art-manifest.py` | PASS, 103 owner/role records |
| `python scripts/fit-body-contacts.py` | PASS, все новые body circles совпадают с сохранёнными значениями в пределах 0.000001 |
| `python scripts/generate_field001_content.py --check` | UP TO DATE |
| `python scripts/check_project.py --scope full` | Unity 6000.6.0f1: EditMode 709/709, PlayMode 26/26, zero skipped; `TestResults/checks/20260924T173828-391919Z/summary.json` |

Первый smoke выявил устаревшее тестовое ожидание отсутствия ENEMY-003 body
(708/709 EditMode). После обновления теста полный повтор прошёл. Визуальная
оценка в реальном масштабе, столкновения и ощущение карты остаются предметом
пользовательского прогона F1-09.
