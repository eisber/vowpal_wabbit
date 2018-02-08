#pragma once

#include <vector>
#include <map>
#include <stdint.h>
#include <iostream>

#include "example.h"

namespace vwp
{
    // FNV-like hash constant for 32bit
    // http://www.isthe.com/chongo/tech/comp/fnv/#FNV-param
    const uint32_t FNV_prime = 16777619;

    typedef std::basic_string<unsigned char> ustring;

    class model
    {
        // the weight_mask does not have the stride shift applied
        const uint64_t _weight_mask;

        // dense only for now
        std::vector<float> _weights;
        std::vector<ustring> _interactions;
        float _contraction;

        float predict(const example& example, const unsigned char* interaction, uint64_t weight_index = 0, float x = 1.f);
        
        inline float predict(uint64_t weight_index, float x)
        { 
            // mask is constructed to never exceed the boundaries
            uint64_t masked_index = weight_index & _weight_mask;

            // TODO: SORT BY (_weights[*weight_index] * *x) DESC to get audit (-a) equivalent output
            // std::cout << masked_index << ":" << x << ":" << _weights[masked_index] << "\t";

            return _weights[masked_index] * x;
        }

    public:
        // contraction, min_label, max_label
        model(const float* weights, uint64_t n);
        model(model&&) = default;

        void set_interactions(std::vector<ustring>& interactions) { _interactions = interactions; }

        void set_contraction(float contraction) { _contraction = contraction; }

        // produce weighted sums over index/value pairs
        // generate interaction terms --> 
        float predict(const example& example);
    };
}