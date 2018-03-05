# Options:
# 

# https://github.com/onnx/onnx/blob/master/docs/Operators.md

import onnx
from onnx_tf.backend import prepare
import onnx.helper as oh
import numpy as np

# build based on number of actions...
cdfmat_arr=np.array([[1, 1, 1], [0, 1, 1], [0, 0, 1]])
cdfmat=oh.make_tensor("cdfmat", onnx.TensorProto.FLOAT, [3,3], cdfmat_arr.flatten().astype(float))
epsilon=oh.make_tensor("epsilon", onnx.TensorProto.FLOAT, [1], np.asarray([0.3]))

input_tensors = [oh.make_tensor_value_info("scores", onnx.TensorProto.FLOAT, [1,3]),
                 oh.make_tensor_value_info("cdfmat", onnx.TensorProto.FLOAT, [3,3]),
                 oh.make_tensor_value_info("epsilon", onnx.TensorProto.FLOAT, [1])]

output_tensors = [oh.make_tensor_value_info("pdf", onnx.TensorProto.FLOAT, [3]),
                  # oh.make_tensor_value_info("top_action", onnx.TensorProto.INT32, [1]),
                 oh.make_tensor_value_info("chosen_action", onnx.TensorProto.INT32, [1]),
                 oh.make_tensor_value_info("cdf", onnx.TensorProto.FLOAT, [3])]

one = oh.make_node(
    'Constant',
    inputs=[],
    outputs=['one'],
    value=oh.make_tensor('one_tensor', onnx.TensorProto.FLOAT, [1], np.asarray([1])))

one_int = oh.make_node(
    'Constant',
    inputs=[],
    outputs=['one_int'],
    value=oh.make_tensor('one_tensor', onnx.TensorProto.INT64, [1], np.asarray([1])))

pdf_temp = oh.make_node(
    'Constant',
    inputs=[],
    outputs=['pdf_temp'],
    value=oh.make_tensor('pdf_temp_tensor', onnx.TensorProto.FLOAT, [3], np.asarray([1./3,1./3,1./3])))

initializer_tensors = [cdfmat, epsilon]

# epsilon-greedy
pdf_temp_2 = oh.make_node('Mul', ["pdf_temp", "epsilon"], ["pdf_temp_2"], broadcast=1)

# pdf = [eps/k, eps/k, ...] + ((float)(int)scores/max(scores)) * (1-eps)
max_score = oh.make_node('ReduceMax', ['scores'], ['max_score'], axes=[1], keepdims=0)
normalized_scores = oh.make_node('Div', ['scores', 'max_score'], ['normalized_scores'], broadcast=1)
# push to 0 and 1 
# replace with ceil
# one_hot_top_action_int = oh.make_node('Cast', ['normalized_scores'], ['one_hot_top_action_int'], to='int32')
# one_hot_top_action_float = oh.make_node('Cast', ['one_hot_top_action_int'], ['one_hot_top_action_float'], to='float')
one_hot_top_action = oh.make_node('Floor', ['normalized_scores'], ['one_hot_top_action'])
exploit_prob = oh.make_node('Sub', ['one', 'epsilon'], ['exploit_prob'])
exploit_top_action = oh.make_node('Mul', ['one_hot_top_action', 'exploit_prob'], ['exploit_vec'], broadcast=1)
# exploit_top_action = oh.make_node('Mul', ['one_hot_top_action_float', 'exploit_prob'], ['exploit_vec'], broadcast=1)

pdf = oh.make_node('Add', ['pdf_temp_2', 'exploit_vec'], ['pdf'])

# seed=42, seed=43
node_rand = oh.make_node('RandomUniform', [], ["r"], shape=[1], seed=43.0)

# can be expressed using Gemm alternatively
node = oh.make_node('MatMul', ["pdf","cdfmat"], ["cdf"])

# ArgMax(Clip(cdf + (1 - rand), max=1))
# node_rand_sub = oh.make_node('Sub', ["one","r"], ["rr"])
add_node = oh.make_node('Add', ["cdf", "r"], ["temp"], broadcast=1)
clip_node = oh.make_node('Clip', ["temp"], ["cdf_clipped"], min=0.0, max=1.0)
topk_node = oh.make_node('ArgMax', ["cdf_clipped"], ["chosen_action"], axis=1, keepdims=0)

graph = oh.make_graph([one, one_int, pdf_temp, pdf_temp_2,
                       max_score, normalized_scores, # top_action_1, # top_action_2,
                       # one_hot_top_action_int, one_hot_top_action_float, 
                       one_hot_top_action,
                       exploit_prob, exploit_top_action,
                       pdf, # node_rand_sub, 
                       node, node_rand, add_node, clip_node, topk_node], 
                    'compute_graph', input_tensors, output_tensors, initializer_tensors)
model = oh.make_model(graph, producer_name='explore')

f = open("model1.onnx", "wb")
f.write(model.SerializeToString())
f.close()

tf_model = onnx.load('model1.onnx')

tf_rep = prepare(tf_model)

sample = tf_rep.run(np.asarray([[.3,.3,.4]]))
print(sample)
