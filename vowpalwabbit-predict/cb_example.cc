#include "cb_example.h"

namespace vwp 
{
    cb_example::cb_example()
        : _shared(nullptr)
    { }

    void cb_example::set_shared(example* shared)
    { _shared = shared; }

    void cb_example::add(example* example)
    { _examples.push_back(example); }

    std::vector<float> cb_example::predict(model& model)
    {
        std::vector<float> predictions;

        for(example* ex : _examples)
        {
            float pred;
            if (!_shared)
                pred = model.predict(*ex);
            else
                pred = model.predict(ex->merge(*_shared));

            predictions.push_back(pred);
        }
    }
}        
