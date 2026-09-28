# DECISION-0084 — Управление мышью и быстрые клавиши паузы

Status: Approved (явное поручение пользователя)
Date: 2026-09-28
Related IP: IP-01, IP-02, IP-26

## Решение

Клавиатурное управление остаётся default. В Settings добавляется сохраняемая
галочка `Mouse movement`, default Off. При включении персонаж движется с полной
текущей movement speed по направлению от своей authoritative world position к
указателю мыши. Скорость не зависит от расстояния до указателя: результат всегда
либо полный input magnitude 1, либо остановка.

Вокруг персонажа действует круглая deadzone радиусом `1.0 world unit` включительно.
Указатель внутри круга или на его границе даёт нулевой input. Радиус задаётся в
Settings JSON как `mouseDeadzoneWorldUnits`; допустимый диапазон `0.25…2.0` units.
Screen position переводится в world point через gameplay camera на глубине игрока.
Pause и terminal states по-прежнему обнуляют движение; forced displacement остаётся
независимым дополнительным каналом.

Ручная пауза переключается `Escape`, `Space` или правой кнопкой мыши. Shortcut
работает только для Running ↔ manual pause. Он не снимает level-up или system pause,
не срабатывает поверх открытых Settings и сохраняет reason-based ownership.

## Persistence и совместимость

Settings schema повышается до v2. Документ v1 мигрируется без потери прежних
audio/shake/video значений и получает `Mouse movement = Off`; новые записи всегда v2.
Повреждённые, неполные v2 и неизвестные версии сохраняют прежнюю recovery policy
DECISION-0038.

## Проверки

- default Off, сохранение On и миграция v1 → Off;
- UI intent проходит View → presenter → settings service;
- pointer внутри deadzone останавливает, снаружи даёт нормализованное направление;
- composed PlayMode проверяет движение вправо, остановку в deadzone и manual-pause toggle;
- целевой EditMode и полный smoke фиксируются в evidence реализации.
