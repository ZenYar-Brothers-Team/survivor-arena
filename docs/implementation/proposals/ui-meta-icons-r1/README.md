# Простые значки развития

Запрос пользователя 2026-09-29: «в окно улучшений нагенери простеньких иконок перед названием».
Preview: index.html, 12 отдельных PNG; точные prompts и references — candidates.json.
Генератор: OpenAI built-in image generation. Пользователь принял 12 вариантов
2026-09-29: [approval](approval.md); hashed packet — Art/Packets/ui-meta-stat-icons-r1.json.

Назначение: маленький slot 32×32 перед названием; без рамки, без роста высоты
карточки. Role icon; владельцы META-003…014 — уже утверждённые personal upgrades.
Стиль: простой storybook cutout, сливовый контур, крупные цветовые массы.
Здоровье/регенерация различаются наличием круговой стрелки, лечение — бутылка;
опыт — кристалл, радиус подбора — магнит. Баланс не меняется.

Пакет применён через art_pipeline.py plan/apply. Source/master в Art/Source/UI,
runtime — Assets/Resources/Art/UI/Icons/Meta. RuntimeContentCatalog включает
META-*-VISUAL-ICON для каждого типа production MetaCatalog; presenter разрешает
SpriteDefinition и передаёт Image. Нет прямых загрузок source PNG из view.
Проверки и текущий execution status — STATUS. Исходный reference регенерации
оставлен только для provenance. HTML сохраняет композиционный предпросмотр.
