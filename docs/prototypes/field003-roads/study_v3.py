"""Compare random Voronoi road density, escape choices and branch capacity."""
from pathlib import Path
import json
import time
import numpy as np
from scipy.spatial import Voronoi
import matplotlib.pyplot as plt
from matplotlib.lines import Line2D
from generate import GRID,X,Y,STEP,axis,distance,road_distance,draw,segment_clear,shortest_path

OUT=Path(__file__).parent

def clip_segment(a,b):
    v=b-a;t0,t1=0.,1.
    for k in range(2):
        if abs(v[k])<1e-9:
            if not 12<=a[k]<=188:return None
            continue
        u,w=sorted(((12-a[k])/v[k],(188-a[k])/v[k]))
        t0,t1=max(t0,u),min(t1,w)
        if t0>=t1:return None
    return a+t0*v,a+t1*v

def network(seed,n,corner_links=True):
    rng=np.random.default_rng(seed)
    spacing=176/n
    sites=[]
    for row in range(n):
        for col in range(n):
            sites.append([12+(col+.5)*spacing,12+(row+.5)*spacing]+rng.uniform(-.32,.32,2)*spacing)
    # Reflected points make finite ridges; only original/original ridges are roads.
    sites=np.array(sites)
    copies=[sites,sites*np.array([-1,1])+[24,0],sites*np.array([-1,1])+[376,0],
            sites*np.array([1,-1])+[0,24],sites*np.array([1,-1])+[0,376]]
    vor=Voronoi(np.concatenate(copies))
    segments=[]
    for (i,j),vertices in zip(vor.ridge_points,vor.ridge_vertices):
        if i>=len(sites) or j>=len(sites) or -1 in vertices:continue
        edge=clip_segment(*vor.vertices[vertices])
        if edge is not None:segments.append(edge)
    boundary=[np.array([12,12]),np.array([188,12]),np.array([188,188]),np.array([12,188])]
    if corner_links:
        interior=[]
        for a,b in segments:
            for p in [a,b]:
                if np.all(p>12.01) and np.all(p<187.99) and not any(np.linalg.norm(p-q)<1e-5 for q in interior):
                    interior.append(p)
        for corner in boundary:
            for p in sorted(interior,key=lambda p:np.linalg.norm(p-corner)):
                if any(segment_clear(corner,p,a,b)<1e-4 for a,b in segments
                       if np.linalg.norm(p-a)>1e-4 and np.linalg.norm(p-b)>1e-4):continue
                segments.append((corner,p));break
    for side,(a,b) in enumerate(zip(boundary,boundary[1:]+boundary[:1])):
        points=[a,b]
        for c,d in segments:
            for p in [c,d]:
                if float(distance(a,b,p))<1e-5:points.append(p)
        points.sort(key=lambda p:np.linalg.norm(p-a))
        for c,d in zip(points[:-1],points[1:]):
            if np.linalg.norm(d-c)>1e-5:segments.append((c,d))
    # Construct exact graph, then traverse degree-two chains for choice distances.
    graph={};coords={}
    for a,b in segments:
        i,j=tuple(np.round(a,5)),tuple(np.round(b,5))
        graph.setdefault(i,[]).append((j,float(np.linalg.norm(b-a))))
        graph.setdefault(j,[]).append((i,float(np.linalg.norm(b-a))))
        coords[i]=a;coords[j]=b
    lengths=[];visited=set()
    for node,neighbors in graph.items():
        if len(neighbors)<3:continue
        for other,length in neighbors:
            if frozenset((node,other)) in visited:continue
            previous,current=node,other
            visited.add(frozenset((previous,current)))
            while len(graph[current])==2:
                following,weight=next((q,w) for q,w in graph[current] if q!=previous)
                length+=weight;previous,current=current,following
                visited.add(frozenset((previous,current)))
            lengths.append(length)
    cycle_lengths=[]
    for node,neighbors in graph.items():
        for other,weight in neighbors:
            if node>=other:continue
            reduced={k:[(q,w) for q,w in ns if frozenset((k,q))!=frozenset((node,other))]
                     for k,ns in graph.items()}
            cycle_lengths.append(weight+shortest_path(reduced,node,other))
    return [np.array([a,b]) for a,b in segments],{
        'sites':n*n,'choice_nodes':sum(len(ns)>=3 for ns in graph.values()),
        'max_between_choices':max(lengths),'p95_between_choices':float(np.percentile(lengths,95)),
        'mean_between_choices':float(np.mean(lengths)),
        'shortest_segment':min(lengths),
        'minimum_cycle':min(cycle_lengths),
    }

def generate(seed,n,attempts=1200,corner_links=True,compact=False,entrance_gap=24):
    start=time.perf_counter()
    paths,stats=network(seed,n,corner_links)
    network_seconds=time.perf_counter()-start
    rd=road_distance(paths);main=rd<=4
    occupied=np.full(X.shape,np.inf);branches=[];lengths=[]
    rng=np.random.default_rng(seed+10000)
    segments=[(p[0],p[1],np.linalg.norm(p[1]-p[0])) for p in paths]
    weights=np.array([s[2] for s in segments]);weights/=weights.sum()
    for _ in range(attempts):
        a,b,_=segments[rng.choice(len(segments),p=weights)]
        base=a+rng.uniform()*(b-a)
        if any(np.linalg.norm(base-c)<entrance_gap for c,_ in branches):continue
        tangent=(b-a)/np.linalg.norm(b-a)
        normal=np.array([-tangent[1],tangent[0]])*rng.choice([-1,1])
        tilt=np.deg2rad(rng.uniform(-18,18))
        normal=np.array([[np.cos(tilt),-np.sin(tilt)],[np.sin(tilt),np.cos(tilt)]])@normal
        u=rng.uniform();length=20+20*(u*u if compact else u)
        center=base+normal*(4/np.cos(tilt)+length-6)
        lo=np.floor((np.minimum(base,center)-10)/STEP).astype(int).clip(0,len(axis)-1)
        hi=np.ceil((np.maximum(base,center)+10)/STEP).astype(int).clip(0,len(axis)-1)+1
        sl=np.s_[lo[1]:hi[1],lo[0]:hi[0]];grid=GRID[sl]
        shape=np.minimum(distance(base,center,grid)-3.5,np.linalg.norm(grid-center,axis=-1)-6)
        mask=shape<=0;away=np.linalg.norm(grid-base,axis=-1)>10
        if np.any(mask&away&(rd[sl]<7)):continue
        if np.any(mask&(occupied[sl]<3)):continue
        if np.any(mask&((X[sl]<3)|(X[sl]>197)|(Y[sl]<3)|(Y[sl]>197))):continue
        occupied[sl]=np.minimum(occupied[sl],shape)
        branches.append((base,center));lengths.append(float(length))
    stats.update(seed=seed,grid_size=n,dead_ends=len(branches),
                 length_mean=float(np.mean(lengths)) if lengths else 0,
                 branch_lengths=lengths,main_area=float(main.sum()*STEP**2),
                 walkable_area=float((main|(occupied<=0)).sum()*STEP**2),
                 network_seconds=network_seconds,total_seconds=time.perf_counter()-start,
                 graph_restarts=0,attempts=attempts,
                 compact_lengths=compact,entrance_gap=entrance_gap)
    return paths,branches,stats

if __name__=='__main__':
    all_results=[]
    for n in [4,5,6]:
        for seed in [17,42,91,123,256,777]:
            r=generate(seed,n,compact=True,entrance_gap=18)
            all_results.append(r)
            print(json.dumps({k:v for k,v in r[2].items() if k!='branch_lengths'}),flush=True)
    (OUT/'metrics-v3.json').write_text(json.dumps([r[2] for r in all_results],indent=2),encoding='utf-8')
    fig,axes=plt.subplots(3,3,figsize=(16,17),dpi=130)
    fig.subplots_adjust(left=.035,right=.985,top=.93,bottom=.07,wspace=.16,hspace=.27)
    fig.canvas.draw()
    for ax,r in zip(axes.flat,[all_results[i] for i in [0,1,2,6,7,8,12,13,14]]):
        draw(ax,*r)
        s=r[2]
        ax.set_title(f"{s['sites']} cells | Seed {s['seed']} | {s['dead_ends']} books\n"
                     f"Longest corridor {s['max_between_choices']/10:.1f}H | {s['total_seconds']:.2f}s",fontsize=11)
    fig.suptitle('FIELD-003 | 15-book target versus distance between escape choices',fontsize=16)
    fig.legend(handles=[Line2D([0],[0],color='#4f8094',lw=8,label='Main roads: width 8'),
                        Line2D([0],[0],color='#d69750',lw=8,label='Dead ends: width 7, length 20-40')],
               loc='lower center',ncol=2,frameon=False)
    fig.savefig(OUT/'density-comparison.png');fig.savefig(OUT/'density-comparison.svg')
    fig,axes=plt.subplots(2,3,figsize=(16,12),dpi=140)
    fig.subplots_adjust(left=.035,right=.985,top=.9,bottom=.08,wspace=.16,hspace=.23)
    fig.canvas.draw()
    for ax,r in zip(axes.flat,all_results[6:12]):
        draw(ax,*r)
        s=r[2]
        ax.set_title(f"Seed {s['seed']} | {s['dead_ends']} books\n"
                     f"Longest corridor {s['max_between_choices']/10:.2f}H | {s['total_seconds']:.2f}s",fontsize=11)
    fig.suptitle('FIELD-003 | 15-book target | Main-network corridor ceiling: 6H',fontsize=16)
    fig.legend(handles=[Line2D([0],[0],color='#4f8094',lw=8,label='Main roads: width 8'),
                        Line2D([0],[0],color='#d69750',lw=8,label='Dead ends: width 7, length 20-40')],
               loc='lower center',ncol=2,frameon=False)
    fig.savefig(OUT/'layouts-v3.png');fig.savefig(OUT/'layouts-v3.svg')
