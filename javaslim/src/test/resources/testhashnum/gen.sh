vw -d data.txt --invert_hash invert.txt --readable_model readable_model.txt -f model.bin 
cat data.txt | vw -t -i model.bin -p predictions.txt

