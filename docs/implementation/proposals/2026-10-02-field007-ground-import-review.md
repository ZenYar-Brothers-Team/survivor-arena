# FIELD-007 — технический просмотр резкости земли

По прямому запросу пользователя 2026-10-02: выяснить причину размытости и улучшить фон; commit препятствий отдельно.

Исходный approved raster `field-007-ground-tile.png`: 1254×1254, hash и PNG не изменены. Старый import profile maxSize 512 уменьшал его до 512×512. Bilinear smoothing включён, mipmaps и compression отключены; дополнительного blur ради performance не найдено. Низкоконтрастная зелёно-коричневая детализация и мелкие пятна присутствуют в исходнике.

Технический вариант: maxSize 2048 сохраняет 1254×1254, pixelsPerUnit 156.75 сохраняет повторение 8×8 wu (1254 / 156.75 = 8). Bilinear/no mipmaps/uncompressed сохраняются. RGBA32 память одной земли возрастает примерно с 1 MiB до 6 MiB; это цена сохранения исходных деталей. Настройки меняются только у FIELD-007, другие поля не затронуты.

Пакет `Art/Packets/field007-ground-full-resolution-preview.json` подготовлен и pipeline запущен. Pipeline отклонил изменение существующего import profile как требующее отдельной reviewed change; исходный raster не переподготавливался. Профиль изменён отдельной технической правкой по текущему запросу; AssetDatabase ForceUpdate применил его через штатный postprocessor. MonasteryGroundImportTests проверяет фактические texture dimensions, 8-unit repeat, no mipmaps и no compression. Текущий статус и результаты — STATUS/evidence; финальная пользовательская visual acceptance остаётся открытой. Этот просмотр не входит в obstacle commit.
