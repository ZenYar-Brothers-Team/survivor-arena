"""Sparse random branching roads with optional large-loop cross connections."""
from pathlib import Path
import json
import time
import numpy as np
import matplotlib.pyplot as plt
from matplotlib.lines import Line2D
from generate import generate,make_roads,draw

OUT=Path(__file__).parent
PROFILE=json.loads((OUT/'profile.json').read_text(encoding='utf-8'))
# Geometry is fixed by the saved helper; reject a mismatched profile instead
# of silently presenting different widths as the accepted reference.
assert [PROFILE[k] for k in ['referenceScreenHeight','arenaSideLength','mainRoadWidth',
                            'deadEndWidth','deadEndLengthMin','deadEndLengthMax',
                            'deadEndEndRadius','independentSurfaceGap']]==[10,200,8,7,20,40,6,3]
assert [PROFILE[k] for k in ['minimumCycleLength','minimumJunctionAngleDegrees',
                            'independentAxisGap']]==[160,60,13]

def choice_metrics(paths,book_bases=()):
    graph={}
    for path in paths:
        for a,b in zip(path[:-1],path[1:]):
            i,j=tuple(np.round(a,5)),tuple(np.round(b,5))
            w=float(np.linalg.norm(b-a))
            graph.setdefault(i,[]).append((j,w));graph.setdefault(j,[]).append((i,w))
    # Interior roads join the continuous ring between its sampled vertices.
    ring=paths[0]
    for node,ns in list(graph.items()):
        if len(ns)!=1:continue
        p=np.array(node)
        for a,b in zip(ring[:-1],ring[1:]):
            v=b-a;t=np.dot(p-a,v)/np.dot(v,v)
            if not 0<t<1:continue
            if np.linalg.norm(p-(a+t*v))>1e-4:continue
            i,j=tuple(np.round(a,5)),tuple(np.round(b,5))
            # May need to split a ring segment already split by another anchor.
            candidates=[(k,q) for k,neighbors in graph.items() for q,_ in neighbors
                        if k<q and abs((q[0]-k[0])*(p[1]-k[1])-(q[1]-k[1])*(p[0]-k[0]))<1e-4
                        and np.dot(p-np.array(k),p-np.array(q))<0]
            for i,j in candidates:
                graph[i]=[(q,w) for q,w in graph[i] if q!=j]
                graph[j]=[(q,w) for q,w in graph[j] if q!=i]
                for other in [i,j]:
                    w=float(np.linalg.norm(p-np.array(other)))
                    graph[node].append((other,w));graph[other].append((node,w))
            break
    forced=set()
    for p in book_bases:
        node=tuple(np.round(p,5));forced.add(node)
        for i,ns in list(graph.items()):
            for j,w in list(ns):
                if i>=j:continue
                a,b=np.array(i),np.array(j);v=b-a
                t=np.dot(p-a,v)/np.dot(v,v)
                if not 0<t<1 or np.linalg.norm(p-(a+t*v))>1e-4:continue
                graph[i]=[(q,w) for q,w in graph[i] if q!=j]
                graph[j]=[(q,w) for q,w in graph[j] if q!=i]
                graph.setdefault(node,[])
                for other in [i,j]:
                    weight=float(np.linalg.norm(p-np.array(other)))
                    graph[node].append((other,weight));graph[other].append((node,weight))
    lengths=[];visited=set()
    for node,ns in graph.items():
        if len(ns)<3 and node not in forced:continue
        for other,length in ns:
            key=frozenset((node,other))
            if key in visited:continue
            visited.add(key);previous,current=node,other
            while len(graph[current])==2 and current not in forced:
                following,weight=next((q,w) for q,w in graph[current] if q!=previous)
                length+=weight;previous,current=current,following
                visited.add(frozenset((previous,current)))
            lengths.append(length)
    return {'choice_nodes':sum(len(ns)>=3 or node in forced for node,ns in graph.items()),
            'road_segments':len(lengths),'segments_below_2H':sum(w<20 for w in lengths),
            'min_segment':min(lengths),'mean_segment':float(np.mean(lengths)),
            'max_segment':max(lengths),'segment_lengths':lengths}

def build(seed):
    start=time.perf_counter()
    def builder(rng):
        # These change structural density and topology, not just a fixed template.
        count=int(rng.integers(PROFILE['interiorNodeCountMin'],PROFILE['interiorNodeCountMax']+1))
        desired_links=int(rng.integers(PROFILE['additionalLinkCountMin'],PROFILE['additionalLinkCountMax']+1))
        return make_roads(rng,count_range=(count,count+1),node_gap=PROFILE['interiorNodeGap'],
                          extra_links=desired_links,min_ring_connections=PROFILE['minimumRingConnections'])
    paths,branches,stats=generate(seed,attempts=PROFILE['bookPlacementAttempts'],road_builder=builder,
                                 entrance_gap=PROFILE['bookEntranceGap'],junction_gap=PROFILE['bookEntranceJunctionGap'])
    stats.update(choice_metrics(paths))
    stats['with_book_entrances']=choice_metrics(paths,[base for base,_ in branches])
    stats['seconds']=time.perf_counter()-start
    return paths,branches,stats

if __name__=='__main__':
    results=[]
    for seed in PROFILE['referenceSeeds']:
        r=build(seed);results.append(r)
        print(json.dumps({k:v for k,v in r[2].items() if k not in ['segment_lengths','branch_lengths','with_book_entrances']}),flush=True)
    references=[{'seed':stats['seed'],'roadCenterlines':[p.tolist() for p in paths],
                 'deadEnds':[{'entrance':a.tolist(),'endCenter':b.tolist(),
                             'bookPosition':b.tolist(),'length':length}
                            for (a,b),length in zip(branches,stats['branch_lengths'])]}
                for paths,branches,stats in results]
    (OUT/'reference-layouts.json').write_text(json.dumps(references,indent=2),encoding='utf-8')
    accepted=json.loads((OUT/'accepted-metrics.json').read_text(encoding='utf-8'))
    for (_,_,actual),expected in zip(results,accepted):
        assert actual['seed']==expected['seed']
        assert actual['dead_ends']==expected['dead_ends']
        assert actual['with_book_entrances']['segments_below_2H']==0
        np.testing.assert_allclose(actual['branch_lengths'],expected['branch_lengths'],rtol=0,atol=1e-8)
        np.testing.assert_allclose(actual['segment_lengths'],expected['segment_lengths'],rtol=0,atol=1e-8)
    print('Accepted v4 geometry replay PASS',flush=True)
    # Preserve the user-approved image; replay only exports numerical references.
