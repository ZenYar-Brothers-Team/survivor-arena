# DECISION-0028 — Enemy pattern phases and projectile lifecycle

Status: Proposed

Date: 2026-09-21

Related IP: IP-04, IP-05, IP-13, IP-14, IP-15, IP-20, IP-21, IP-29

## Context

IP-13 сохраняет уже существующие movement/projectile families и переносит на них approved source/control semantics DECISION-0017. Ранее fixture enemy controls могли не присутствовать, dash contact не отличался от обычного, а возвращённый projectile удерживал source/profile до следующего Initialize. Фаза attack не была доступна для наблюдения.

## Implementation record

EnemyAttackController владеет cooldown/telegraph/burst phase и spiral rotation. Content задаёт wind-up и per-kind fields; runtime только рисует предупреждение и создаёт commands. При ненулевом wind-up первый выстрел ждёт полный telegraph, следующие burst shots используют interval; cooldown отсчитывается от первого shot, а очередной wind-up начинается после cooldown и завершения burst. Это fixture scheduler contract; production attack sequences/HP phases остаются IP-15/20/21. Новых wave burst rules не вводится.

EnemyRuntime использует общие controls поверх base/scaled velocity и отдельный DashContactControls только в Dashing. Aim telegraph ranged follows target; dash direction остаётся locked по существующему контракту. Pause сохраняет observable phase. Development snapshot проходит через существующий UI presenter и остаётся внутри drawer.

EnemyProjectileRuntime подписан на конкретный RunModel жизни projectile. Terminal event освобождает projectile немедленно, pause останавливает скорость. Hit/expiry сначала снимает snapshot в locals и возвращает object, затем вызывает player combat pipeline. Callback может арендовать тот же object: старое попадание не обращается к нему после damage. Pool return/reinit/disable очищают runtime references/source/velocity/trail. Handler сверяет полученное state с current bound run, чтобы оставшийся multicast callback старого забега не освободил новую running life.

WaveEnemyScaler переносит telegraph, обычные и dash controls без изменения семантики. Profiles/source category-neutral: ordinary, boss и Traveler используют один pipeline.

## Limits

Техническая межслойная запись для review, не новое product approval. Production values/cards и NPC sequences не изменены. Fixtures имеют явные synthetic knockback/wind-up values. Новые изображения, VFX art approval и W-01 не входят в поставку.
