from onnx import * 

m=ModelProto()

with open("model1.onnx", "rb") as fp:
	m.ParseFromString(fp.read())

checker.check_model(m)
