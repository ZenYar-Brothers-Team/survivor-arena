# DECISION-0162 — оригинальные звуки молнии и круговых ударов FIELD-010

Дата: 2026-10-03. После отдельного прослушивания пользователь поручил: «вставить эти звуки для круговых ударов и для ударов с эффектом-молнией».

## Привязка

- `screen.lightning` (SCREEN-EVENT-003/004/007/008/012) использует `lightning-v1`: резкий треск, несколько нерегулярных разрядов и короткий низкий грохот, 1.05 с.
- `screen.circle` (SCREEN-EVENT-005/006/009) использует `light-column-v1`: воздушный спуск 0.16 с, затем мягкий удар и магический звон, всего 1.20 с. Спуск является частью клипа; звук запускается в прежней точке начала удара, gameplay telegraph не меняется.
- Оружие/облако и прочие игровые семьи сохраняют свои клипы. У прежних `zone_strike`, `zone_burst`, `lightning_cast` остаются собственные другие потребители.

Gain обеих семей 0.60, real-time cooldown 0.12/0.16 с, priority 1, bounded important pool и pause/resume остаются из [DECISION-0159](0159-field010-strike-motion-and-audio.md). Плотная серия не создаёт отдельный source на каждую опасность; player hurt priority 2 имеет преимущество.

## Источник и импорт

Оригинальный процедурный синтез Codex по запросу пользователя, без сторонних записей/сэмплов. Воспроизводимый [generator](../../Art/Prototypes/field010-audio-sketches/generate.py) содержит deterministic seeds 10031/10032; source hashes и runtime hashes фиксируются в [SOURCES](../audio/SOURCES.json). Это собственный материал проекта, а не скачанный CC0-пакет; provenance не приписывает ему стороннего автора, URL или лицензию.

Runtime: `Assets/Resources/Audio/Sfx/lightning-v1.wav` и `light-column-v1.wav`, 44.1 kHz stereo PCM16, peak −3.35 dBFS, мягкие края и короткие отражения. Стандартный `ProductionAudioImportPostprocessor` готовит DecompressOnLoad, Vorbis quality 0.75; metadata создаёт Unity. Ресурсные пути стабильны, расширение не входит в cue path.

Audio integrity validator проверяет WAV/OGG одинаково по каталогу и hashes. Для внешних файлов сохраняется проверка CC0/https; для оригинального синтеза проверяется локальный generator/hash и отсутствие сторонних samples. Artwork, hazard geometry, урон и время событий не меняются. Отдельная галерея остаётся доступна для прослушивания, а игровые pause/resume и bounded-catalog checks выполняются после импорта.
