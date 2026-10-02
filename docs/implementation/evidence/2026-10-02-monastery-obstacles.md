# FIELD-007 — проверка готовых препятствий

2026-10-02. Контракт DECISION-0146, текущий статус только STATUS. User scope — готовые препятствия с плотностью FIELD-001, отдельный коммит без background changes.

Working-tree graphics run `TestResults/checks/20261002T142333-014162Z`: 12/12 EditMode + 1/1 PlayMode прошли, но runner не записал reusable PASS из-за параллельного изменения inputs. Включены ProductionMonasteryContentTests, ProductionMonasteryObstacleTests, MonasteryGroundImportTests и ProductionMonasterySmokeTests. Проверены восемь пар seeds, существующие шесть typed Prop refs и размещение всех 36 алтарей с foundation clearance. Текущая карта 120×120; screenshot `TestResults/field007-minimap.png` наблюдает 97 препятствий из целевых 98. Правило допустимых пропусков pattern generator при отсутствии места сохраняется. Попытка использовать полный effect-radius clearance не поместила алтари; специальный foundation radius настроен отдельно.

Art scope `TestResults/checks/20261002T142500-081117Z/summary.json`: 112/112 EditMode и manifest 316/316 PASS. Исходные obstacle raster assets не менялись. Снимок просмотрен: bridge/root props и outlines мини-карты присутствуют, пространства вокруг оснований свободны. Пользовательский visual review остаётся отдельным.

В obstacle commit включены только obstacle authoring/generator/clearance/tests/docs. Ранее сделанные изменения размера/радиусов/индикации и отдельный full-resolution ground import preview сохранены в working tree; они не смешиваются с этой границей коммита. Проверки выше относятся к working tree, а не к изолированному commit snapshot.
