#include "model.h"
#include "cb_example.h"
#include "parser_json.h"

#include <iostream>
#include <string>
#include <fstream>
#include <streambuf>

using namespace vwp;
using namespace std;

int main(int argc, char *argv[])
{
    example e;
    uint64_t weight_indicies[] = { 0, 1, 5};
    float x[] = { 7, 1, .24f };
    e.add_namespace(' ', weight_indicies, x, 3);

    std::ifstream model_json(argv[1]);
    std::string json((std::istreambuf_iterator<char>(model_json)),
                    std::istreambuf_iterator<char>());

    model m = model_parser_json::parse(json.c_str());

    // ../vowpalwabbit/vw -b 4 -d ../../vwds/vowpalwabbit-predict/data.txt -f model1 --readable_model model1.txt
    cout << "predict: " << m.predict(e) << endl;    
}