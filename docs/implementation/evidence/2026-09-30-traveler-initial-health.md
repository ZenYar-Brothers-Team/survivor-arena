# Начальное здоровье Путников — 2026-09-30

По [DECISION-0127](../../decisions/0127-traveler-initial-health-scaling.md)
здоровье Путника отделено от множителя урона. В `0:00` health multiplier равен
`K/3`; затем он линейно достигает `K` к `T−120`. Урон, скорость, presence и
support не менялись. В production и fixture schedule добавлено явное
`initialHealthMultiplier: 0.333333`.

Проверки: targeted Traveler EditMode **35/35 PASS**, 0 failed/skipped;
`TestResults/checks/20260930T165351-114531Z/summary.json`.
