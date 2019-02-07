vw -d data.txt --oaa 3 --readable_model readable_model.txt -f model.bin
vw -d test.txt -i model.bin -t -r raw_predictions.txt
vw -d test.txt -i model.bin -t -p predictions.txt --probabilities
