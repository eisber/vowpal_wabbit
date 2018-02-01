conda-build --python 3.5 .

az ml service create realtime -f score_iris.py --model-file model.pkl -s service_schema.json -n irisapp9 -r python --collect-model-data true -c aml_config\conda_dependencies.yml

# clean local docker images to be able to re-create image using same version
az ml experiment clean -a --project C:\work\aml\marcozo1\marcozo1 -t docker

sudo apt-get install libcpprest2.8 libcpprest-dev rapidjson-dev libboost-all-dev

choco install swig
