import numpy as np
import heapq

def beam_search(mprobability,mdistance, beamsize=40):
    """ Do a beam search using the probability matrix (mprobability)
        at the end of the beam search there should be beamsize
        soutions, select the best one by minimizing solution
        distance using distance matrix (mdistance).
    """
    # get number of nodes from matrix
    nnodes=mprobability[0].size
    # insert first node (0) in heap
    nodes_not_assigned=np.arange(1, nnodes).tolist()
    node_assigned=np.arange(0,1).tolist()
    H=[(0,0,node_assigned,nodes_not_assigned,0)] #cost, last node in solution, actual solution, nodes nos assigned, total distance
    heapq.heapify(H)
    # repeat until not more node to select
    count=1
    while(count<nnodes):
        H=add_new_node_to_beam_search(mprobability,mdistance,H, beamsize)
        count=count+1
    return obtener_solucion_minima(mdistance,H)

def add_new_node_to_beam_search(mprobability,mdistance,Hold, beamsize):
    """ Do a beam search using the probability matrix (mprobability)
        at the end of the beam search there should be beamsize
        soutions, select the best one by minimizing solution
        distance using distance matrix (mdistance).
    """
    # create the new heap
    Hnew=[]
    heapq.heapify(Hnew)    
    # get all solutions from heap to add new solutions
    for h in Hold:
        add_next_nodes_to_solution(mprobability,mdistance,  beamsize, h,Hnew)
    return Hnew


def add_next_nodes_to_solution(mprobability,mdistance,  beamsize, actual_solution,Heap):
    cost=actual_solution[0]
    last_node=actual_solution[1]    
    distance=actual_solution[4]
    # mira si los nuevos nodos entran en heap 
    for node in actual_solution[3]:
        new_cost=cost*mprobability[last_node][node]
        new_distance=distance+mdistance[last_node][node]
        new_solution=actual_solution[2].copy()
        new_solution.append(node)
        new_nodes=actual_solution[3].copy()
        new_nodes.remove(node)
        mincost=0
        if(len(Heap)>beamsize):
            # Si el nuevo valor es mayor que el minimo, si no no lo mete
            mincost=min(Heap)[0]
            if(new_cost>mincost):
                heapq.heappushpop(Heap,(new_cost,node,new_solution,new_nodes,new_distance))
        else:
            heapq.heappush(Heap,(new_cost,node,new_solution,new_nodes,new_distance))

def obtener_solucion_minima(mdistance,Heap):
    """ Do a beam search using the probability matrix (mprobability)
        at the end of the beam search there should be beamsize
        soutions, select the best one by minimizing solution
        distance using distance matrix (mdistance).
    """
    min_distance=-1
    min_solution=[]
    # get all solutions from heap to add new solutions
    for h in Heap:
        distance=h[4]+mdistance[h[1]][0]
        if(min_distance<0 or min_distance>distance ):
            min_distance=distance
            min_solution=h[2].copy()
    return min_distance, min_solution

