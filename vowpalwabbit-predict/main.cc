#include "model.h"
#include "cb_example.h"
#include "parser_json.h"

#include <iostream>
#include <string>
#include <fstream>
#include <streambuf>

using namespace vwp;
using namespace std;

enum Features
{
    // Shared
    Modality_Audio      = 0,
    Modality_Video      = 1,
    CallType_P2P        = 2,
    CallType_Server     = 3,
    NetworkType_Wired   = 4,
    NetworkType_Wifi    = 5,
    // ADF
    Action1             = 6,
    Action2             = 7,
    Action3             = 8
};

enum Namespaces
{
    SharedA             = 0,
    SharedB             = 1,
    SharedC             = 2,
    ActionDependentX    = 3,
    ActionDependentY    = 4,
};

int main(int argc, char *argv[])
{
//     example e;
//     uint64_t weight_indicies[] = { 0, 1, 5};
//     float x[] = { 7, 1, .24f };
//     e.add_namespace(' ', weight_indicies, x, 3);

    std::ifstream model_json(argv[1]);
    std::string json((std::istreambuf_iterator<char>(model_json)),
                    std::istreambuf_iterator<char>());

    cout << json << endl;

    model m = model_parser_json::parse(json.c_str());

    // ../vowpalwabbit/vw -b 4 -d ../../vwds/vowpalwabbit-predict/data.txt -f model1 --readable_model model1.txt
    // cout << "predict: " << m.predict(e) << endl;    


    // train
    // ../vowpalwabbit/vw -b 8 -d data-cb.json -f model-cb1 --predict_model_json model-cb.json --cb_adf --json
    // predict
    // head -n 1 data-cb.json | ../vowpalwabbit/vw -i model-cb1 -t --json -a --examples 1

    // setup shared context
    example shared;
    // 1-hot encoded feature
    shared.add_namespace(Namespaces::SharedA, Features::Modality_Audio); 
    shared.add_namespace(Namespaces::SharedB, Features::CallType_P2P); 
    shared.add_namespace(Namespaces::SharedC, Features::NetworkType_Wired); 

    // setup actions
    example action1, action2, action3;
    action1.add_namespace(Namespaces::ActionDependentX, Features::Action1);
    action2.add_namespace(Namespaces::ActionDependentX, Features::Action2);
    action3.add_namespace(Namespaces::ActionDependentX, Features::Action3);

    // construct contextual-bandit example
    cb_example cb_ex;
    cb_ex.set_shared(&shared);

    cb_ex.add_action(&action1);
    cb_ex.add_action(&action2);
    cb_ex.add_action(&action3);

    cout << cb_ex << endl;
                
    std::vector<float> scores = cb_ex.predict(m); 

    std::copy(scores.begin(), scores.end(), std::ostream_iterator<float>(cout, " "));
    std::cout << std::endl; 
}