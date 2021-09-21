import numpy as np
from scipy.spatial.distance import pdist, squareform, euclidean


def leer_fichero(path,num_nodes):
        """ Lee un fichero en formato Google con un problema y su solución optima
            Obtiene las coordenadas, el tour_optimal y su longitud (tour_optimal_len)
        """
        with open(path) as f:
            row=f.readline()

        line = row.split(" ")  # Split into list
            
        # Compute signal on nodes
        nodes = np.ones(num_nodes)  # All 1s for TSP...
            
        # Convert node coordinates to required format
        nodes_coord_tmp = []
        for idx in range(0, 2 * num_nodes, 2):
            nodes_coord_tmp.append([float(line[idx]), float(line[idx + 1])])
        # Obtain tour_optimal
        tour_optimal_tmp = [int(node) - 1 for node in line[line.index('output') + 1:-1]][:-1]
        # Order coordentes by tour_optimal
        nodes_coord = []
        for idx in tour_optimal_tmp:
            nodes_coord.append(nodes_coord_tmp[idx])
        # Compute distance matrix
        W_val = squareform(pdist(nodes_coord, metric='euclidean'))
        # Obtain tour_optimal (it is just the order of the numbers since we
        # haver ordered the coordiantes)
        tour_optimal = np.arange(num_nodes)
        # Compute node and edge representation of tour + tour_len
        tour_optimal_len = 0
        for idx in range(len(tour_optimal) - 1):
            i = tour_optimal[idx]
            j = tour_optimal[idx + 1]
            tour_optimal_len += W_val[i][j]
            
        # Add final connection of tour in edge target
        tour_optimal_len += W_val[j][tour_optimal[0]]
        return tour_optimal,np.array(nodes_coord), tour_optimal_len





#path="D:\\PytorchEnVisualStudio\\TSP_problem_data\\tsp_100_1.txt"
#num_cities=100
#tour_optimal,nodes_coord, tour_optimal_len = leer_fichero(path,num_cities)

