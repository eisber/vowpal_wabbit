#pragma once

#include "model.h"

namespace vwp
{
    class cb_example
    {
        example* _shared;
        std::vector<example*> _examples;

    public:
        cb_example();
        
        void set_shared(example* shared);

        void add(example* example);

        std::vector<float> predict(model& model);
    };
}