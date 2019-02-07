vw -d data.txt --readable_model readable_model.txt -f model.bin --link logistic
vw -d test.txt -i model.bin -t -p predictions.txt --link logistic
