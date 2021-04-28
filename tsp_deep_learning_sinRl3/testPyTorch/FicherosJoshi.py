import numpy as np
from pathlib import Path, PureWindowsPath
from scipy.spatial.distance import pdist, squareform, euclidean
import util_grafos as grafo
import pandas as pd
import random
def leer_fichero(row,num_nodes):
        """ Lee un fichero en formato Google con un problema y su solución optima
            Obtiene las coordenadas, el tour_optimal y su longitud (tour_optimal_len)
        """
            
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

def get_tour_len_from_distance_matrix(tour_distance_matrix):
    """ Get tour lenght using tour and distance matrix
    """
    # Compute node and edge representation of tour + tour_len
    tour_len = 0
    nodes_number = tour_distance_matrix.shape[0]
    for idx in range(nodes_number - 1):
         i = idx
         j = idx + 1
         tour_len += tour_distance_matrix[i][j]
            
    # Add final connection of tour in edge target
    tour_len += tour_distance_matrix[j][0]
    return (tour_len)

def get_tour_len_from_coordinates(tour, nodes_coord):
    """ Get tour lenght using tour and distance matrix
    """
    # Compute node and edge representation of tour + tour_len
    tour_len = 0
    tour_size = tour.size
    for idx in range(tour_size - 1):
         i = tour[idx]
         j = tour[idx + 1]         
         tour_len +=euclidean(nodes_coord[i], nodes_coord[j])
            
    # Add final connection of tour in edge target
    tour_len += euclidean(nodes_coord[j], nodes_coord[tour[0]])
    return (tour_len)


def obtener_matriz_coordenadas_desde_tour(tour_nodes, nodes_coord):
            
            nodes_coord_tmp = []
            for idx in tour_nodes:
                nodes_coord_tmp.append(nodes_coord[idx])
            arr = np.vstack(nodes_coord_tmp)
            return arr


def obtener_optimal_tour_desde_tour(tour_nodes):
            
            nodes_coord_tmp = tour_nodes.copy()
            for idx in tour_nodes:
                nodes_coord_tmp[tour_nodes[idx]] = idx
            
            return nodes_coord_tmp

def obtener_matriz_indicadora_tour_optimo_en_base_a_tour(tour_base, tour_optimo):
    """ This function receives a tour_base and prepare and
        indicator matrix with row and col ordered by tour_base
        but with indicators in tour_optimo position.
        Example: tour_base(2,1,0)
                 tour_optimal(0,1,2)
                 Matrix(3,3) with all zeroes except M(2,1)=1, M(1,0)=1, M(0,2)=1
        calculate the distance matrix in the same order that
        the tour. It is possible to include a value for the
        values in the principal diagonal of the matrix.
    """
    tour_size = tour_base.size
    matrix_indicator = np.zeros((tour_size, tour_size))
    i = -1
    j_first = 0
    for idx in tour_optimo:
        pos = np.where(tour_base == idx)
        j = pos[0][0]
        if(i > -1):
            matrix_indicator[i][j] = 1
            matrix_indicator[j][i] = 1
        else:
          j_first = j
        i = j
    matrix_indicator[i][j_first] = 1
    matrix_indicator[j_first][i] = 1
    return matrix_indicator


def obtener_matriz_indicadora_desde_tour(tour_optimo):
    """ This function receives a tour_base and prepare and
        indicator matrix with row and col ordered by tour_base
        but with indicators in tour_optimo position.
        Example: tour_base(2,1,0)
                 tour_optimal(0,1,2)
                 Matrix(3,3) with all zeroes except M(2,1)=1, M(1,0)=1, M(0,2)=1
        calculate the distance matrix in the same order that
        the tour. It is possible to include a value for the
        values in the principal diagonal of the matrix.
    """
    tour_size = tour_optimo.size
    matrix_indicator = np.zeros((tour_size, tour_size))
    i = -1
    j_first = 0
    for idx in tour_optimo:
        j = idx        
        if(i > -1):
            matrix_indicator[i][j] = 1
            matrix_indicator[j][i] = 1
        else:
            j_first = j
        i = j
    matrix_indicator[i][j_first] = 1
    matrix_indicator[j_first][i] = 1
    return matrix_indicator

def obtener_matriz_desde_tour(tour_nodes, nodes_coord, identity_value=0):
    """ This function receives a tour and the nodes_coord and
        calculate the distance matrix in the same order that
        the tour. It is possible to include a value for the
        values in the principal diagonal of the matrix.
    """
    nodes_coord_tmp = []
    for idx in tour_nodes:
        nodes_coord_tmp.append(nodes_coord[idx])
    # Compute distance matrix
    W_val = squareform(pdist(nodes_coord_tmp, metric='euclidean'))    
    # Obtain indentity matrix same shape as W_val qith identity_value
    identity = np.identity(W_val.shape[0]) * identity_value
    return (W_val + identity)

def obtener_distance_matrix_from_coords(nodes_coord, identity_value=0):
    """ This function receives a tour and the nodes_coord and
        calculate the distance matrix in the same order that
        the tour. It is possible to include a value for the
        values in the principal diagonal of the matrix.
    """
    # Compute distance matrix
    W_val = squareform(pdist(nodes_coord, metric='euclidean'))    
    # Obtain indentity matrix same shape as W_val qith identity_value
    identity = np.identity(W_val.shape[0]) * identity_value
    return (W_val + identity)


def realizar_cambio(tour_actual,tour_actual_distance_matrix, id_node_to_change1, id_node_to_change2):
    """ This function takes actual tour with actual_distance_matrix, and change id1 in tour with id2 in tour
        so the result is a new tour (tour_new) and a new distance matrix (tour_new_distance_matrix)
        IMPORTANT: files and columns order in the distance matrix is the same as in the tour
    """
    #Change tour
    tour_new = np.copy(tour_actual)
    tour_new[id_node_to_change1] = tour_actual[id_node_to_change2]
    tour_new[id_node_to_change2] = tour_actual[id_node_to_change1]
    
    #Change matrix
    tour_new_distance_matrix = np.copy(tour_actual_distance_matrix)
   
    # chage rows
    tour_new_distance_matrix[id_node_to_change1:id_node_to_change1 + 1] = tour_actual_distance_matrix[id_node_to_change2:id_node_to_change2 + 1]
    tour_new_distance_matrix[id_node_to_change2:id_node_to_change2 + 1] = tour_actual_distance_matrix[id_node_to_change1:id_node_to_change1 + 1]
    #Change columns
    tour_new_distance_matrix[:,id_node_to_change1:id_node_to_change1 + 1] = tour_actual_distance_matrix[:,id_node_to_change2:id_node_to_change2 + 1]
    tour_new_distance_matrix[:,id_node_to_change2:id_node_to_change2 + 1] = tour_actual_distance_matrix[:,id_node_to_change1:id_node_to_change1 + 1]
    
    return tour_new, tour_new_distance_matrix

def get_initial_tour(tour_in,tour_in_distance_matrix):
    """ This function obtain a "good" initial solution
        for this its find the minimum distance node
        reachable from each node and
        select the maximum of this minimum (regret type)
        then repeat the process.
        The tour its return start always with the index of
        the initial_node that it is passed.
        OJO! No funciona
    """
        # copia la matriz
    distance_tmp = np.copy(tour_in_distance_matrix)
    num_rows = distance_tmp.shape[0]
    max_value = np.max(distance_tmp) * 10
    # Fill the diagonal with the value
    np.fill_diagonal(distance_tmp, max_value)
    # matriz recorrido
    tour_matrix = np.zeros(distance_tmp.shape)
    cuenta = 0
    # Obtain tour
    row_first = -1
    while(cuenta < (num_rows - 1)):
        #First step is to obtain the minimun distance node from each node and
        #to obtain the max of those mins
        row_idx, col_idx = get_max_min_nodes(distance_tmp,max_value)
        if(cuenta == 0):            
            distance_tmp[:,row_idx] = max_value
            row_first = row_idx
            
        #Marca como max_value ese row,col para no volverla a seleccionar
        distance_tmp[row_idx,:] = max_value
        distance_tmp[:,col_idx] = max_value
        distance_tmp[col_idx][row_idx] = max_value 
        tour_matrix[row_idx, col_idx] = 1
        row_idx = col_idx
        print(row_idx)
        cuenta = cuenta + 1
    # Add last value (from last to first)
    tour_matrix[row_idx,row_first] = 1
    print(tour_matrix)
    # Obtain the index with 1 in tour_matrix
    i, j = np.where(tour_matrix == 1)
    idx = 0
    tour_tmp = []
    tour = []
   # tour_tmp.append(idx)
   # tour.append(tour_in[idx])
    while(len(tour) < num_rows):
        idx = j[idx]
        tour_tmp.append(idx)
        tour.append(tour_in[idx])
    #pone el tour para que comience con el mismo indice que tour_in
    value = tour_in[0]
    result = np.where(np.array(tour) == value)
    idx = result[0][0]
    a = tour[idx:]
    b = tour[:idx]
    c = a + b
    return np.array(c)        

def get_initial_tour_min(tour_in_distance_matrix):
    """ This function obtain a "good" initial solution
        for this its find the minimum distance node
        and then its add the next node that is also
        the minimum with the actual.
    """
    # copia la matriz
    distance_tmp = np.copy(tour_in_distance_matrix)
    num_rows = distance_tmp.shape[0]
    max_value = np.max(distance_tmp) * 10
    # Fill the diagonal with the value
    np.fill_diagonal(distance_tmp, max_value)
    # matriz recorrido
    tour_matrix = np.zeros(distance_tmp.shape)
    cuenta = 0
    # Obtain tour
    row_first = -1
    while(cuenta < (num_rows - 1)):
        #First step is to obtain the minimun distance node from each node and
        #to obtain the max of those mins
        if(cuenta == 0):
            row_idx, col_idx = get_max_min_nodes(distance_tmp,max_value)
            distance_tmp[:,row_idx] = max_value
            row_first = row_idx
        else:
            col_idx = np.argmin(distance_tmp[row_idx,:])    
            
        #Marca como max_value ese row,col para no volverla a seleccionar
        distance_tmp[row_idx,:] = max_value
        distance_tmp[:,col_idx] = max_value
        distance_tmp[col_idx][row_idx] = max_value 
        tour_matrix[row_idx, col_idx] = 1
        row_idx = col_idx
#        print(row_idx)
        cuenta = cuenta + 1
    # Add last value (from last to first)
    tour_matrix[row_idx,row_first] = 1
 #   print(tour_matrix)
    # Obtain the index with 1 in tour_matrix
    i, j = np.where(tour_matrix == 1)
    idx = 0
    tour = []
   # tour_tmp.append(idx)
   # tour.append(tour_in[idx])
    while(len(tour) < num_rows):
        idx = j[idx]
        tour.append(idx)
    #pone el tour para que comience con el indice 0
    result = np.where(np.array(tour) == 0)
    idx = result[0][0]
    a = tour[idx:]
    b = tour[:idx]
    c = a + b
    return np.array(c)    


def get_max_min_nodes(tour_distance_matrix,max_value):
    """ this function calculate for each row its minimum
        column, and return the maximum (row and column) of all minimums
    """
    row_idx = np.argmin(tour_distance_matrix, axis=1)
    row_value = np.min(tour_distance_matrix,axis=1)
    # Solo selecciona aquellas menores que max_value, las max_values ya han
    # sido seleccionadas previamente
    valid_idx = np.where(row_value < max_value)[0]
    row_idx = valid_idx[row_value[valid_idx].argmax()]
    #row_idx=np.argmax(row_value)
    col_idx = np.argmin(tour_distance_matrix[row_idx,:])
    return row_idx, col_idx

def get_indicator_matrix_for_optimal_base_on_tour(tour):
    """ This function obtain the indicator matrix for optimal
        tour based on actual tour. Optimal tour is 0,1,2,3,...
        But we wanted it with the ordering of tour
    """
    tmp_dict = {}
    idx = 0
    for value in tour:
        tmp_dict[value] = idx
        idx = idx + 1
    
    indicator_matrix = np.zeros((len(tour),len(tour)), dtype=int)
    for idx in range(len(tour) - 1):
        i = tmp_dict[idx]
        j = tmp_dict[idx + 1]
        indicator_matrix[i][j] = 1
        indicator_matrix[j][i] = 1
    i = tmp_dict[tour[len(tour) - 1]]
    j = tmp_dict[tour[0]]
    indicator_matrix[i][j] = 1
    indicator_matrix[j][i] = 1
    return indicator_matrix

def get_line_data_square(tour_random_distance_matrix,tour_optimal_indicator_matrix,nodes_coord_tour):
    data1 = tour_random_distance_matrix.flatten('C') # Flatten by row
    data2 = tour_optimal_indicator_matrix.flatten('C')
    data3 = nodes_coord_tour.flatten()
    data = np.asarray(np.concatenate((data1,data2,data3)))
    return data

def get_line_data_triangle(tour_random_distance_matrix,tour_optimal_indicator_matrix):
    data1 = get_upper_matrix_flatten(tour_random_distance_matrix)
    data2 = get_upper_matrix_flatten(tour_optimal_indicator_matrix)
    
    data = np.asarray(np.concatenate((data1,data2)))
    return data

def get_upper_matrix_flatten(matrix):
    numcol = matrix[0].size
    data = []
    for i in range(0, numcol): 
        for j in range(i + 1, numcol):
           data.append(matrix[i][j]) 
    return  np.array(data)

# Python3 program to find
# minimum number of swaps
# required to sort an array

# Function returns the minimum
# number of swaps required to
# sort the array
def minSwaps(arr):
	n = len(arr)
	
	# Create two arrays and use
	# as pairs where first array
	# is element and second array
	# is position of first element
	arrpos = [*enumerate(arr)]
	
	# Sort the array by array element
	# values to get right position of
	# every element as the elements
	# of second array.
	arrpos.sort(key = lambda it : it[1])
	
	# To keep track of visited elements.
	# Initialize all elements as not
	# visited or false.
	vis = {k : False for k in range(n)}
	
	# Initialize result
	ans = 0
	for i in range(n):
		
		# alreadt swapped or
		# alreadt present at
		# correct position
		if vis[i] or arrpos[i][0] == i:
			continue
			
		# find number of nodes
		# in this cycle and
		# add it to ans
		cycle_size = 0
		j = i
		
		while not vis[j]:
			
			# mark node as visited
			vis[j] = True
			
			# move to next node
			j = arrpos[j][0]
			cycle_size += 1
			
		# update answer by adding
		# current cycle
		if cycle_size > 0:
			ans += (cycle_size - 1)
			
	# return answer
	return ans


def GenerarEjemplosDiferentes():
    with open("D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\data_rnd_" + str(num_cities) + "_" + str(str_tipo) + "_train.csv", 'a') as fw:
        with open(strFilePath) as f:
            for line in f:
                tour_optimal,nodes_coord, tour_optimal_len = leer_fichero(line,num_cities)
                #grafo.plot_tour_graph(nodes_coord, tour_optimal)
            
                # Obtain a random tour
                tour_random = tour_optimal.copy()
                random.shuffle(tour_random)
                # grafo.plot_tour_graph(nodes_coord, tour_random)
                # Obtain random tour distance matrix
                tour_random_distance_matrix = obtener_matriz_desde_tour(tour_random,nodes_coord,-1)
                # Obtain indicator matrix for optimal tour in tour_random order
                tour_optimal_indicator_matrix = obtener_matriz_indicadora_tour_optimo_en_base_a_tour(tour_random, tour_optimal)
                # obtiene las coordenadas referidas al tour_random
                nodes_coord_tour = obtener_matriz_coordenadas_desde_tour(tour_random,nodes_coord)
                #grafo.plot_tour_graph(nodes_coord_tour, tour_optimal)
                # Obtiene los datos para guardar
                if(str_tipo == "square"):
                    data = get_line_data_square(tour_random_distance_matrix,tour_optimal_indicator_matrix,nodes_coord_tour)
                elif (str_tipo == "triangle"):
                    data = get_line_data_triangle(tour_random_distance_matrix,tour_optimal_indicator_matrix)
                else:
                    raise ValueError('A very specific bad thing happened.')

    #
    #        print(data)
            # save to csv file
                cuenta = cuenta + 1
                print(cuenta)
                np.savetxt(fw, [data],fmt='%1.8f', delimiter=';')

def GenerarMismoEjemplo(strPathFileOut,num_cities,intRepeticiones, strline):
    with open(strPathFileOut, 'a') as fw:
        for cuenta in range(intRepeticiones):
                tour_optimal,nodes_coord, tour_optimal_len = leer_fichero(strline,num_cities)
                #grafo.plot_tour_graph(nodes_coord, tour_optimal)
            
                # Obtain a random tour
                tour_random = tour_optimal.copy()
                random.shuffle(tour_random)
                #grafo.plot_tour_graph(nodes_coord, tour_random)
                 # obtiene las coordenadas referidas al tour_random y el nuevo
                 # optimal referido al tour_random
                nodes_coord_tour = obtener_matriz_coordenadas_desde_tour(tour_random,nodes_coord)
                tour_random_optimal = obtener_optimal_tour_desde_tour(tour_random)
                #grafo.plot_tour_graph(nodes_coord_tour, tour_random_optimal)
                # Obtain distance matrix using new coords
                tour_random_distance_matrix = obtener_distance_matrix_from_coords(nodes_coord_tour,-1)
                # Obtain indicator matrix for tour_random_optimal
                #tour_optimal_indicator_matrix = obtener_matriz_indicadora_tour_optimo_en_base_a_tour(tour_random, tour_optimal)
                # Obtain random tour distance matrix
                #tour_random_distance_matrix2 = obtener_matriz_desde_tour(tour_random,nodes_coord,-1)
                # Obtain indicator matrix for optimal tour in tour_random order
                tour_optimal_indicator_matrix = obtener_matriz_indicadora_desde_tour(tour_random_optimal)
                #grafo.plot_indicator_matrix_graph(nodes_coord_tour,tour_optimal_indicator_matrix)
                # Obtiene los datos para guardar
                data = get_line_data_square(tour_random_distance_matrix,tour_optimal_indicator_matrix,nodes_coord_tour)
                print(cuenta)
                np.savetxt(fw, [data],fmt='%1.8f', delimiter=';')
    #
    #        print(data)
            # save to csv file
                

# This code is contributed
# by Dharan Aditya


# Read data from files
np.random.seed(42)
random.seed(42)
#torch.manual_seed(42)
num_cities = 10
intRepeticiones=2
num_distintos=15000
strFilePathData = "D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\tsp-data\\tsp" + str(num_cities) + "_train_concorde.txt"
strFilePathOut="D:\\PytorchEnVisualStudio\\pyTorchTSP\\TSP_problem_data_concorde_joshi\\data_rnd_" + str(num_cities) + "_varios_train.csv"
strLine = ""
with open(strFilePathData) as f:
    for i in range(num_distintos):
         strLine = f.readline()
         GenerarMismoEjemplo(strFilePathOut,num_cities,intRepeticiones,strLine)

