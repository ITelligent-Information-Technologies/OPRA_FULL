import warnings
import torch
import numpy as np
import os
import json
from tqdm import tqdm
from multiprocessing.dummy import Pool as ThreadPool
from multiprocessing import Pool
import torch.nn.functional as F
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
            
        # Conecta con el ultimo punto y devuelve el valor
        tour_optimal_len += W_val[j][tour_optimal[0]]
        return tour_optimal,np.array(nodes_coord), tour_optimal_len




def load_args(filename):
    with open(filename, 'r') as f:
        args = json.load(f)

    # Backwards compatibility
    if 'data_distribution' not in args:
        args['data_distribution'] = None
        probl, *dist = args['problem'].split("_")
        if probl == "op":
            args['problem'] = probl
            args['data_distribution'] = dist[0]
    return args


def load_problem(name):
    from utilidades.problem_tsp import TSP
    problem = {
        'tsp': TSP
    }.get(name, None)
    assert problem is not None, "Currently unsupported problem: {}!".format(name)
    return problem

def torch_load_cpu(load_path):
    return torch.load(load_path, map_location=lambda storage, loc: storage)  # Load on CPU


def _load_model_file(load_path, model):
    """Loads the model with parameters from the file and returns optimizer state dict if it is in the file"""

    # Load the model parameters from a saved state
    load_optimizer_state_dict = None
    print('  [*] Loading model from {}'.format(load_path))

    load_data = torch.load(
        os.path.join(
            os.getcwd(),
            load_path
        ), map_location=lambda storage, loc: storage)

    if isinstance(load_data, dict):
        load_optimizer_state_dict = load_data.get('optimizer', None)
        load_model_state_dict = load_data.get('model', load_data)
    else:
        load_model_state_dict = load_data.state_dict()

    state_dict = model.state_dict()

    state_dict.update(load_model_state_dict)

    model.load_state_dict(state_dict)

    return model, load_optimizer_state_dict




def load_model(path, epoch=None):
    from utilidades.attention_model import AttentionModel
    #from nets.pointer_network import PointerNetwork

    if os.path.isfile(path):
        model_filename = path
        path = os.path.dirname(model_filename)
    elif os.path.isdir(path):
        if epoch is None:
            epoch = max(
                int(os.path.splitext(filename)[0].split("-")[1])
                for filename in os.listdir(path)
                if os.path.splitext(filename)[1] == '.pt'
            )
        model_filename = os.path.join(path, 'epoch-{}.pt'.format(epoch))
    else:
        assert False, "{} is not a valid directory or file".format(path)

    args = load_args(os.path.join(path, 'args.json'))

    problem = load_problem(args['problem'])

    model_class = {
        'attention': AttentionModel,
        #'pointer': PointerNetwork
    }.get(args.get('model', 'attention'), None)
    assert model_class is not None, "Unknown model: {}".format(model_class)

    model = model_class(
        args['embedding_dim'],
        args['hidden_dim'],
        problem,
        n_encode_layers=args['n_encode_layers'],
        mask_inner=True,
        mask_logits=True,
        normalization=args['normalization'],
        tanh_clipping=args['tanh_clipping'],
        checkpoint_encoder=args.get('checkpoint_encoder', False),
        shrink_size=args.get('shrink_size', None)
    )
    # Overwrite model parameters by parameters to load
    load_data = torch_load_cpu(model_filename)
    model.load_state_dict({**model.state_dict(), **load_data.get('model', {})})

    model, *_ = _load_model_file(model_filename, model)

    model.eval()  # Put in eval mode

    return model, args
