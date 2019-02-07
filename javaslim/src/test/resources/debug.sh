echo '1 |aa nw:1 nz:2 np:3 nb:4 cj=0 cj=1 cs=2' | vw -t -i ./model.bin -a 2>&1 | grep Constant | tr "\t" "\n" | sort

