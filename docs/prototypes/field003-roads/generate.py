"""Geometric study only; no production data or Unity assets are changed."""
from pathlib import Path
import json
import heapq
import numpy as np
import matplotlib
matplotlib.use('Agg')
import matplotlib.pyplot as plt
from matplotlib.patches import Circle
from matplotlib.lines import Line2D
from scipy.sparse.csgraph import minimum_spanning_tree

OUT = Path(__file__).parent
STEP = 0.5
axis = np.arange(0, 200 + STEP, STEP)
X, Y = np.meshgrid(axis, axis)
GRID = np.stack((X, Y), axis=-1)

def distance(a, b, grid=GRID):
    v = b - a
    t = np.clip(np.sum((grid - a) * v, axis=-1) / np.dot(v, v), 0, 1)
    return np.linalg.norm(grid - (a + t[..., None] * v), axis=-1)

def road_distance(paths):
    result = np.full(X.shape, np.inf)
    for path in paths:
        for a, b in zip(path[:-1], path[1:]):
            result = np.minimum(result, distance(a, b))
    return result

def ring_path():
    ring = []
    for center, start in [((174, 26), -90), ((174, 174), 0),
                          ((26, 174), 90), ((26, 26), 180)]:
        angles = np.deg2rad(np.linspace(start, start + 90, 13))
        ring.extend(np.array(center) + 14 * np.c_[np.cos(angles), np.sin(angles)])
    ring.append(ring[0])
    return np.array(ring)

def angle(v, w):
    return np.degrees(np.arccos(np.clip(np.dot(v,w)/np.linalg.norm(v)/np.linalg.norm(w),-1,1)))

def segment_clear(a,b,c,d):
    # Distance between planar line segments, including crossings.
    v,w=b-a,d-c
    cross=lambda x,y: x[0]*y[1]-x[1]*y[0]
    denom=cross(v,w)
    if abs(denom)>1e-8:
        t,u=cross(c-a,w)/denom,cross(c-a,v)/denom
        if 0<=t<=1 and 0<=u<=1:
            return 0.0
    return min(float(distance(a,b,c)),float(distance(a,b,d)),
               float(distance(c,d,a)),float(distance(c,d,b)))

def shortest_path(graph,start,end):
    queue=[(0,start)]; best={start:0}
    while queue:
        length,node=heapq.heappop(queue)
        if node==end:return length
        if length>best[node]:continue
        for other,weight in graph.get(node,[]):
            candidate=length+weight
            if candidate<best.get(other,float('inf')):
                best[other]=candidate;heapq.heappush(queue,(candidate,other))
    return float('inf')

def cycles_long_enough(points,edges,anchors,owners,ring):
    graph={};minimum=float('inf')
    def add(i,j,w):
        graph.setdefault(i,[]).append((j,w));graph.setdefault(j,[]).append((i,w))
    for i,j in edges:add(i,j,np.linalg.norm(points[i]-points[j]))
    positions=[]
    lengths=np.linalg.norm(np.diff(ring,axis=0),axis=1)
    cumulative=np.r_[0,np.cumsum(lengths)]
    for k,(p,owner) in enumerate(zip(anchors,owners)):
        node=len(points)+k
        add(owner,node,np.linalg.norm(p-points[owner]))
        distances=[float(distance(a,b,p)) for a,b in zip(ring[:-1],ring[1:])]
        n=int(np.argmin(distances))
        positions.append((cumulative[n]+np.linalg.norm(p-ring[n]),node))
    positions.sort()
    for n,(position,node) in enumerate(positions):
        next_position,other=positions[(n+1)%len(positions)]
        arc=(next_position-position)%cumulative[-1]
        cycle=shortest_path(graph,node,other)+arc
        minimum=min(minimum,cycle)
        if cycle<160:return False,minimum
        add(node,other,arc)
    return True,minimum

def make_roads(rng,count_range=(8,14),node_gap=32,extra_links=0,min_ring_connections=0):
    ring=ring_path()
    for retry in range(300):
        points=[]
        count=int(rng.integers(*count_range))
        for _ in range(500):
            p=rng.uniform(32,168,2)
            if all(np.linalg.norm(p-q)>node_gap for q in points):
                points.append(p)
                if len(points)==count: break
        if len(points)!=count: continue
        points=np.array(points)
        costs=np.linalg.norm(points[:,None,:]-points[None,:,:],axis=-1)
        tree=minimum_spanning_tree(costs).tocoo()
        edges=[(int(i),int(j)) for i,j in zip(tree.row,tree.col)]
        neighbors={i:[] for i in range(count)}
        for i,j in edges: neighbors[i].append(j);neighbors[j].append(i)
        if any(angle(points[j]-points[i],points[k]-points[i])<60
               for i,ns in neighbors.items() for n,j in enumerate(ns) for k in ns[n+1:]):
            continue
        if any(np.linalg.norm(points[i]-points[j])<25 for i,j in edges): continue
        if any(segment_clear(points[i],points[j],points[k],points[l])<13
               for n,(i,j) in enumerate(edges) for k,l in edges[n+1:]
               if len({i,j,k,l})==4): continue
        initial_tree_edges=len(edges)
        added=0
        candidates=[(i,j) for i in range(count) for j in range(i+1,count)
                    if j not in neighbors[i]]
        if extra_links:rng.shuffle(candidates)
        for i,j in candidates:
            if added>=extra_links:break
            if np.linalg.norm(points[i]-points[j])>95:continue
            if len(neighbors[i])>=3 or len(neighbors[j])>=3:continue
            if any(angle(points[j]-points[i],points[k]-points[i])<60 for k in neighbors[i]):continue
            if any(angle(points[i]-points[j],points[k]-points[j])<60 for k in neighbors[j]):continue
            if any(segment_clear(points[i],points[j],points[k],points[l])<13
                   for k,l in edges if len({i,j,k,l})==4):continue
            graph={}
            for k,l in edges:
                w=float(np.linalg.norm(points[k]-points[l]))
                graph.setdefault(k,[]).append((l,w));graph.setdefault(l,[]).append((k,w))
            if shortest_path(graph,i,j)+np.linalg.norm(points[j]-points[i])<160:continue
            edges.append((i,j));neighbors[i].append(j);neighbors[j].append(i);added+=1
        segments=[(points[i],points[j]) for i,j in edges]
        anchors=[];owners=[]
        valid=True
        leaves=[i for i in range(count) if len(neighbors[i])==1]
        rng.shuffle(leaves)
        extra_owners=[i for i in range(count) if i not in leaves and len(neighbors[i])<3] if min_ring_connections else []
        if extra_owners:rng.shuffle(extra_owners)
        for i in leaves+extra_owners:
            if i not in leaves and len(anchors)>=min_ring_connections:break
            p=points[i]
            candidates=[np.array([12,np.clip(p[1],28,172)]),
                        np.array([188,np.clip(p[1],28,172)]),
                        np.array([np.clip(p[0],28,172),12]),
                        np.array([np.clip(p[0],28,172),188])]
            candidates.sort(key=lambda q: np.linalg.norm(q-p))
            accepted=False
            for q in candidates:
                if angle(q-p,points[neighbors[i][0]]-p)<60: continue
                if any(np.linalg.norm(q-r)<25 for r in anchors):continue
                if any(segment_clear(p,q,a,b)<13 for a,b in segments
                       if not(np.allclose(p,a) or np.allclose(p,b))): continue
                segments.append((p,q));anchors.append(q);owners.append(i);accepted=True;break
            if not accepted and i in leaves:valid=False;break
        if not valid or len(anchors)<min_ring_connections:continue
        valid,min_cycle=cycles_long_enough(points,edges,anchors,owners,ring)
        if not valid:continue
        # Replace degree-two sharp bends by quadratic curves with a 7-unit trim.
        paths=[ring]
        for a,b in segments:
            aa,bb=a.copy(),b.copy()
            for p,other,which in [(a,b,0),(b,a,1)]:
                incident=[d if np.allclose(c,p) else c for c,d in segments
                          if np.allclose(c,p) or np.allclose(d,p)]
                if len(incident)==2:
                    trimmed=p+(other-p)/np.linalg.norm(other-p)*7
                    if which==0:aa=trimmed
                    else:bb=trimmed
            paths.append(np.array([aa,bb]))
        for p in points:
            incident=[d if np.allclose(c,p) else c for c,d in segments
                      if np.allclose(c,p) or np.allclose(d,p)]
            if len(incident)==2:
                a,b=[p+(q-p)/np.linalg.norm(q-p)*7 for q in incident]
                t=np.linspace(0,1,9)[:,None]
                paths.append((1-t)**2*a+2*t*(1-t)*p+t*t*b)
        return paths,{'inner_nodes':count,'ring_connections':len(anchors),
                      'tree_edges':initial_tree_edges,'extra_links':added,'network_retries':retry,
                      'minimum_graph_cycle':float(min_cycle)}
    raise RuntimeError('No valid connected random graph in 300 tries')

def generate(seed, attempts=720, compact=False,road_builder=make_roads,entrance_gap=20,junction_gap=0):
    rng = np.random.default_rng(seed)
    paths,graph_stats = road_builder(rng)
    endpoint_counts={}
    for path in paths[1:]:
        for p in [path[0],path[-1]]:
            key=tuple(np.round(p,5));endpoint_counts[key]=endpoint_counts.get(key,0)+1
    # Degree-one endpoints of interior paths attach to the ring; degree-three
    # endpoints are real interior junctions. Degree-two curve joins are bends.
    junctions=[np.array(p) for p,count in endpoint_counts.items() if count==1 or count>=3]
    rd = road_distance(paths)
    main = rd <= 4
    occupied_distance = np.full(X.shape, np.inf)
    branches = []
    lengths = []
    # Uniform sampling by total road arclength, either side of every road.
    segments = [(a, b, np.linalg.norm(b-a)) for p in paths for a, b in zip(p[:-1], p[1:])]
    weights = np.array([s[2] for s in segments]); weights /= weights.sum()
    for _ in range(attempts):
        a, b, _ = segments[rng.choice(len(segments), p=weights)]
        base = a + rng.uniform() * (b-a)
        if junction_gap and any(np.linalg.norm(base-p)<junction_gap for p in junctions):
            continue
        # Density control, not a book-count cap: keep entrances ~two screens apart.
        if any(np.linalg.norm(base-old_base)<entrance_gap for old_base,_ in branches):
            continue
        tangent = (b-a) / np.linalg.norm(b-a)
        normal = np.array([-tangent[1], tangent[0]]) * rng.choice([-1, 1])
        tilt=np.deg2rad(rng.uniform(-18,18))
        normal=np.array([[np.cos(tilt),-np.sin(tilt)],
                         [np.sin(tilt),np.cos(tilt)]])@normal
        # Allowed range remains 20-40. Favor shorter branches to reserve capacity.
        u = rng.uniform()
        length = 20 + 20 * (u*u if compact else u)
        radius = 6.0  # Study assumption: 12-unit rounded end, wider than 7.5 entrance.
        entry_distance=4/np.cos(tilt)
        center = base + normal * (entry_distance + length - radius)
        # Evaluate only a candidate's bounding box instead of the entire map.
        lo=np.floor((np.minimum(base,center)-10)/STEP).astype(int).clip(0,len(axis)-1)
        hi=np.ceil((np.maximum(base,center)+10)/STEP).astype(int).clip(0,len(axis)-1)+1
        sl=np.s_[lo[1]:hi[1],lo[0]:hi[0]]
        grid=GRID[sl]
        ds = distance(base, center,grid)
        dc = np.linalg.norm(grid - center, axis=-1)
        shape = np.minimum(ds - 3.5, dc - radius)
        mask = shape <= 0
        # Junction with its parent road is allowed only within 10 units of base.
        outside_junction = np.linalg.norm(grid-base, axis=-1) > 10
        if np.any(mask & outside_junction & (rd[sl] < 7)):
            continue
        if np.any(mask & (occupied_distance[sl] < 3)):
            continue
        if np.any(mask & ((X[sl] < 3) | (X[sl] > 197) | (Y[sl] < 3) | (Y[sl] > 197))):
            continue
        # Store clearance distances out to three units beyond the shape.
        occupied_distance[sl] = np.minimum(occupied_distance[sl], shape)
        branches.append((base, center))
        lengths.append(float(length))
    all_walkable = main | (occupied_distance <= 0)
    return paths, branches, {
        'seed': seed, 'dead_ends': len(branches), 'attempts': attempts, **graph_stats,
        'entrance_gap':entrance_gap,'junction_gap':junction_gap,
        'length_sampling': 'short-biased' if compact else 'uniform',
        'length_min': min(lengths), 'length_max': max(lengths),
        'length_mean': float(np.mean(lengths)),
        'main_area': float(main.sum()*STEP**2),
        'walkable_area': float(all_walkable.sum()*STEP**2),
        'branch_lengths': lengths,
    }

def draw(ax, paths, branches, stats):
    ax.set_facecolor('#c9d6bd')
    # Draw in coordinate units, independent of image resolution.
    unit_points = ax.get_window_extent().width / 200 * 72 / ax.figure.dpi
    for path in paths:
        ax.plot(path[:,0], path[:,1], color='#4f8094', lw=8*unit_points,
                solid_capstyle='round', solid_joinstyle='round')
    for base, center in branches:
        ax.plot([base[0],center[0]], [base[1],center[1]], color='#d69750',
                lw=7*unit_points, solid_capstyle='round')
        ax.add_patch(Circle(center,6,color='#d69750',lw=0))
        ax.scatter(*center,s=17,c='#fff9dc',edgecolors='#65461d',linewidths=.6,zorder=5)
    ax.set(xlim=(0,200),ylim=(0,200),aspect='equal',xticks=np.arange(0,201,50),yticks=np.arange(0,201,50))
    ax.grid(alpha=.13,color='black')
    ax.set_title(f"Seed {stats['seed']} | {stats['dead_ends']} dead ends\n"
                 f"Mean length {stats['length_mean']:.1f} | Walkable {stats['walkable_area']/400:.1f}%",fontsize=11)

if __name__ == '__main__':
    results = [generate(seed) for seed in [17, 42, 91, 123, 256, 777]]
    fig, axes = plt.subplots(2,3,figsize=(16,12),dpi=150)
    fig.subplots_adjust(left=.035,right=.985,top=.90,bottom=.09,wspace=.16,hspace=.20)
    fig.canvas.draw()
    for ax,(paths,branches,stats) in zip(axes.flat,results):
        draw(ax,paths,branches,stats)
    fig.suptitle('FIELD-003 random graph study | 200 x 200 | Screen height H = 10',fontsize=16,y=.96)
    fig.legend(handles=[Line2D([0],[0],color='#4f8094',lw=8,label='Main roads: width 8'),
                        Line2D([0],[0],color='#d69750',lw=8,label='Dead ends: width 7, length 20-40'),
                        Line2D([0],[0],marker='o',color='none',markerfacecolor='#fff9dc',markeredgecolor='#65461d',label='Book: exactly one upgrade')],
               loc='lower center',ncol=3,frameon=False,bbox_to_anchor=(.5,.015))
    fig.savefig(OUT/'layouts.png')
    fig.savefig(OUT/'layouts.svg')
    metrics = [r[2] for r in results]
    (OUT/'metrics.json').write_text(json.dumps(metrics,indent=2),encoding='utf-8')
    print(json.dumps([{k:v for k,v in m.items() if k!='branch_lengths'} for m in metrics],indent=2))
