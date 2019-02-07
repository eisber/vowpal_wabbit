vw -d data.txt --readable_model readable_model.txt -f model.bin --link glf1
vw -d test.txt -i model.bin -t -p predictions.txt --link glf1
