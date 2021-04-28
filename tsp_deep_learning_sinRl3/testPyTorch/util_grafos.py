import numpy as np
from scipy.spatial import distance_matrix
import matplotlib.pyplot as plt

def get_graph_mat(n=10, size=1):
    """ Throws n nodes uniformly at random on a square, and build a (fully connected) graph.
        Returns the (N, 2) coordinates matrix, and the (N, N) matrix containing pairwise euclidean distances.
    """
    coords = size * np.random.uniform(size=(n,2))
    dist_mat = distance_matrix(coords, coords)
    return coords, dist_mat

def plot_graph(coords, mat):
    """ Utility function to plot the fully connected graph
    """
    n = len(coords)
    
    plt.scatter(coords[:,0], coords[:,1], s=[50 for _ in range(n)])
    for i in range(n):
        for j in range(n):
            if j < i:
                plt.plot([coords[i,0], coords[j,0]], [coords[i,1], coords[j,1]], 'r', alpha=0.7,linewidth=1)
    plt.show()

def plot_tour_graph(coords, tour):
    """ utility function to plot a tour
    """
    n = len(coords)
    x=coords[:,0]
    y=coords[:,1]
    plt.scatter(x,y, s=[50 for _ in range(n)])
    idx_old=-1
    idx_first=-1
    for idx in tour:
        if (idx_old>-1):
            plt.plot([coords[idx_old,0], coords[idx,0]], [coords[idx_old,1], coords[idx,1]], 'r', alpha=0.7,linewidth=1)
        else:
            idx_first=idx
        idx_old=idx
    # Connect last with first
    plt.plot([coords[idx_old,0], coords[idx_first,0]], [coords[idx_old,1], coords[idx_first,1]], 'r', alpha=0.7,linewidth=1)
    plt.show()

def plot_indicator_matrix_graph(coords, indicator_matrix):
    """ utility function to plot a tour
    """
    n = len(coords)
    x=coords[:,0]
    y=coords[:,1]
    plt.scatter(x,y, s=[50 for _ in range(n)])
    idx_old=-1
    idx_first=-1
    rows = len(indicator_matrix)
    columns = len(indicator_matrix[0])
    for i in range(rows):
        for j in range(columns):
            if (indicator_matrix[i][j]==1):
                plt.plot([coords[i,0], coords[j,0]], [coords[i,1], coords[j,1]], 'r', alpha=0.7,linewidth=1)
    plt.show()

def covert_flat_coords_to_coords(coords_flat):
    """ Convert a one dimension coordinate vector into a x and y coords
    """
    x=[]
    y=[]
    n=int(coords_flat.size/2)
    for i in range(n):
        x.append(coords_flat[0][i*2])
        y.append(coords_flat[0][(i*2+1)])
    return x, y

def plot_connections_graph(coords_flat, connections):
    """ utility function to plot a tour
    """
    x,y =covert_flat_coords_to_coords(coords_flat)
    n =len( x)
    plt.scatter(x,y, s=[50 for _ in range(n)])
   
    for i in range(n):
        plt.text(x[i],y[i], str(i),                     fontdict={'weight': 'bold', 'size': 9})
    index=0
    for idx in connections:
        plt.plot([x[idx[0]], x[idx[1]]], [y[idx[0]], y[idx[1]]], 'r', alpha=0.7,linewidth=1)
        #if(index>0):
        #    plt.plot([x[index-1], x[index]], [y[index-1], y[index]], 'g', alpha=0.7,linewidth=0.5)
        index=index+1
    # Connect last with first
    #plt.plot([coords[idx_old,0], coords[0,0]], [coords[idx_old,1], coords[0,1]], 'r', alpha=0.7,linewidth=1)
    plt.show()