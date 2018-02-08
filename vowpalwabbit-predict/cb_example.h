#pragma once

#include <ostream>
#include "model.h"

namespace vwp
{
    class cb_example
    {
        example* _shared;
        std::vector<example*> _actions;

    public:
        cb_example() : _shared(nullptr) { }
        
        void set_shared(example* shared) { _shared = shared; }

        void add_action(example* example) { _actions.push_back(example); }

        std::vector<float> predict(model& model);

        friend std::ostream& operator<<(std::ostream& os, const cb_example& cb_ex);
    };

    std::ostream& operator<<(std::ostream& os, const cb_example& cb_ex);
}