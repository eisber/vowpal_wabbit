#include "cb_example.h"
#include <iterator>

namespace vwp 
{
    std::vector<float> cb_example::predict(model& model)
    {
        std::vector<float> predictions;

        for(example* ex : _actions)
            predictions.push_back(_shared ? 
                model.predict(*ex) : 
                model.predict(ex->merge(*_shared)));

        return predictions;
    }

    std::ostream& operator<<(std::ostream& os, const example* ex)
    { return os << *ex; }

    std::ostream& operator<<(std::ostream& os, const cb_example& cb_ex)
    {
        os << '{';
        if (cb_ex._shared)
            os << "\"shared\":" << *cb_ex._shared << ",";

        if (!cb_ex._actions.empty())
        {
            os << "\"_multi\":[";

            std::copy(cb_ex._actions.begin(), 
                cb_ex._actions.end() - 1, 
                std::ostream_iterator<example*>(os, ","));

            os << *(cb_ex._actions.end() - 1)
               << ']';
        }

        return os << '}';
    }
}        
