"""FIELD-009 study: circular platforms joined by bridges. 200x200, H=10, W=H*16/9."""
from pathlib import Path
import json, math
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Circle, Polygon

OUT = Path(__file__).parent
SIZE, H = 200.0, 10.0
W = H * 16 / 9
R_MIN, R_MAX = H / 2 * 1.1, W    # min diameter 1.1*H .. 2W
BRIDGE_W = H / 2 * 1.1
SMALL_R, BOOK_P = 7.5, 0.5       # two platforms below SMALL_R never joined; Book chance per platform
GAP_MAX = 26.0                   # bridge length between circle edges
MARGIN = 2.0
GIANT = R_MAX * 0.8


def sample_radius(rng, big_p):
    if rng.random() < big_p:
        return rng.uniform(GIANT, R_MAX)
    return R_MIN + (GIANT - R_MIN) * rng.beta(1.3, 2.6)


def place(rng, target, big_p, gap_lo):
    pts, tries = [], 0
    while len(pts) < target and tries < 30000:
        tries += 1
        r = sample_radius(rng, big_p)
        c = rng.uniform(r + MARGIN, SIZE - r - MARGIN, 2)
        if all(np.linalg.norm(c - p[:2]) >= r + p[2] + gap_lo for p in pts):
            pts.append((c[0], c[1], r))
    return np.array(pts)


def gap(a, b):
    return np.linalg.norm(a[:2] - b[:2]) - a[2] - b[2]


def clear(P, i, j):
    """Bridge i-j must not touch a third platform."""
    a, b = P[i, :2], P[j, :2]
    v = b - a
    L2 = v @ v
    for k in range(len(P)):
        if k in (i, j):
            continue
        t = np.clip((P[k, :2] - a) @ v / L2, 0, 1)
        if np.linalg.norm(P[k, :2] - (a + t * v)) < P[k, 2] + BRIDGE_W / 2 + 1.5:
            return False
    return True


def candidates(P):
    E = []
    for i in range(len(P)):
        for j in range(i + 1, len(P)):
            g = gap(P[i], P[j])
            if P[i][2] < SMALL_R and P[j][2] < SMALL_R:
                continue
            if g <= GAP_MAX and clear(P, i, j):
                E.append((g, i, j))
    return sorted(E)


def angle_ok(P, edges, i, j, min_deg):
    for _, a, b in edges:
        for s, o_new in ((i, j), (j, i)):
            if s in (a, b):
                o = b if s == a else a
                v1 = P[o_new, :2] - P[s, :2]
                v2 = P[o, :2] - P[s, :2]
                ang = math.degrees(math.acos(np.clip(v1 @ v2 / np.linalg.norm(v1) / np.linalg.norm(v2), -1, 1)))
                if ang < min_deg:
                    return False
    return True


def build(P, rng, loops, mode):
    cand = candidates(P)
    n = len(P)
    if mode == 'dense':
        out = []
        for e in cand:
            if angle_ok(P, out, e[1], e[2], 30):
                out.append(e)
        return out
    jit = sorted(cand, key=lambda e: e[0] * rng.uniform(0.8, 1.25))
    par = list(range(n))

    def find(x):
        while par[x] != x:
            par[x] = par[par[x]]
            x = par[x]
        return x

    tree = []
    for e in jit:
        a, b = find(e[1]), find(e[2])
        if a != b and angle_ok(P, tree, e[1], e[2], 35):
            par[a] = b
            tree.append(e)
    extra = []
    for e in jit:
        if e not in tree and angle_ok(P, tree + extra, e[1], e[2], 40):
            extra.append(e)
    rng.shuffle(extra)
    return tree + extra[:loops]


LEAF_MIN_R = 12.0   # only a big platform may be a dead end


def fix_leaves(P, E, start):
    """Give every small dead end a second bridge, else delete it. Repeats until stable."""
    alive = set(range(len(P)))
    E = list(E)
    cand = candidates(P)
    while True:
        deg = {i: 0 for i in alive}
        for _, i, j in E:
            deg[i] += 1
            deg[j] += 1
        leaves = [i for i in alive if deg[i] <= 1 and P[i, 2] < LEAF_MIN_R and i != start]
        if not leaves:
            break
        i = leaves[0]
        fix = None
        for e in cand:
            g, a, b = e
            if i in (a, b) and a in alive and b in alive and e not in E and angle_ok(P, E, a, b, 40):
                fix = e
                break
        if fix:
            E.append(fix)
        else:
            alive.discard(i)
            E = [e for e in E if i not in (e[1], e[2])]
    return alive, E


def reach(n, E, start):
    adj = {i: [] for i in range(n)}
    for _, i, j in E:
        adj[i].append(j)
        adj[j].append(i)
    seen = {start: 0}
    q = [start]
    for u in q:
        for v in adj[u]:
            if v not in seen:
                seen[v] = seen[u] + 1
                q.append(v)
    return seen


def metrics(P, E, start):
    n = len(P)
    seen = reach(n, E, start)
    plat = sum(math.pi * p[2] ** 2 for p in P)
    bridges = sum(g * BRIDGE_W for g, _, _ in E)
    return dict(platforms=n, bridges=len(E), connected=len(seen) == n,
                cycles=len(E) - n + 1,
                platform_cover_pct=round(100 * plat / SIZE ** 2, 1),
                total_cover_pct=round(100 * (plat + bridges) / SIZE ** 2, 1),
                radius_min=round(float(P[:, 2].min()), 1), radius_max=round(float(P[:, 2].max()), 1),
                giants=int((P[:, 2] >= GIANT).sum()), max_hops=max(seen.values()), leaves=sum(1 for i in range(n) if sum(i in (a, b) for _, a, b in E) <= 1), small_leaves=sum(1 for i in range(n) if sum(i in (a, b) for _, a, b in E) <= 1 and P[i, 2] < LEAF_MIN_R),
                bridge_len=[round(min(e[0] for e in E), 1), round(max(e[0] for e in E), 1)])


def gen(seed, target, big_p, gap_lo, loops, mode):
    c = np.array([SIZE / 2, SIZE / 2])
    for attempt in range(40):
        rng = np.random.default_rng(seed * 100 + attempt)
        P = place(rng, target, big_p, gap_lo)
        E = build(P, rng, loops, mode)
        start = int(np.argmin(np.linalg.norm(P[:, :2] - c, axis=1)))
        alive, E = fix_leaves(P, E, start)
        keep = sorted(set(reach(len(P), E, start)) & alive)
        if len(keep) >= 0.7 * len(P):
            break
    idx = {o: n for n, o in enumerate(keep)}
    P2 = P[keep]
    E2 = [(g, idx[i], idx[j]) for g, i, j in E if i in idx and j in idx]
    return P2, E2, idx[start]


def draw(ax, P, E, start, title):
    books = np.random.default_rng(int(P[:, 0].sum() * 1000) % 2**31).random(len(P)) < BOOK_P
    ax.set_facecolor('#d9b8a0')
    ax.add_patch(plt.Rectangle((0, 0), SIZE, SIZE, fill=False, ec='#333', lw=2))
    for _, i, j in E:
        a, b = P[i, :2], P[j, :2]
        v = (b - a) / np.linalg.norm(b - a)
        n = np.array([-v[1], v[0]]) * BRIDGE_W / 2
        ax.add_patch(Polygon([a + n, b + n, b - n, a - n], fc='#8fa6b8', ec='none', zorder=2))
    for k, (x, y, r) in enumerate(P):
        ax.add_patch(Circle((x, y), r, fc='#c9e0b8' if k == start else '#a9c7a0', ec='#4d6b47', lw=1.2, zorder=3))
        if k != start and books[k]:
            ax.plot(x, y, 'o', ms=3.2, mfc='#fff9dc', mec='#65461d', zorder=4)
    ax.plot(*P[start, :2], marker='*', ms=11, c='#c0392b', zorder=5)
    cx, cy = P[start, :2]
    ax.add_patch(plt.Rectangle((cx - W / 2, cy - H / 2), W, H, fill=False, ec='#c0392b', lw=1.6, ls='--', zorder=6))
    ax.set(xlim=(0, SIZE), ylim=(0, SIZE), aspect='equal', xticks=range(0, 201, 50), yticks=range(0, 201, 50))
    ax.set_title(title, fontsize=10)


VARIANTS = [(f'Генерация {k}, seed {sd}', dict(seed=sd, target=34, big_p=0.22, gap_lo=9, loops=8, mode='tree'))
            for k, sd in enumerate([201, 202, 203, 204, 205, 206], 1)]

if __name__ == '__main__':
    fig, axes = plt.subplots(2, 3, figsize=(21, 14.5))
    res = {}
    for ax, (name, kw) in zip(axes.flat, VARIANTS):
        P, E, s = gen(**kw)
        m = metrics(P, E, s)
        res[name] = m
        draw(ax, P, E, s, f"{name}\n{m['platforms']} платформ, {m['bridges']} мостов, циклов {m['cycles']}, "
                          f"земля {m['total_cover_pct']}%, r {m['radius_min']}-{m['radius_max']}")
    fig.suptitle('FIELD-009 | 200x200 | H=10, W~17.8 | мост 5.5 | мин. диаметр 11 | точка - книга (50%), звезда - старт, красный пунктир - экран 17.8x10 на старте', fontsize=13)
    fig.tight_layout(rect=(0, 0, 1, .96))
    fig.savefig(OUT / 'layouts-v4.png', dpi=70)
    (OUT / 'metrics-v4.json').write_text(json.dumps(res, ensure_ascii=False, indent=1), encoding='utf-8')
    print(json.dumps(res, ensure_ascii=False, indent=1))
