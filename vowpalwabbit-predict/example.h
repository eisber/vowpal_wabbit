#pragma once

#include <vector>
#include <map>
#include <stdint.h>
#include "features.h"

namespace vwp
{
    class example 
    {
        std::map<char, features> _namespaces;

    public:
        example();
        example(example&&) = default;
        example(const example&) = default;

        // uint64_t namespace_offset -> assuming it's part of weight_indicies 
        void add_namespace(char feature_group, const uint64_t* weight_indicies, const float* x, uint32_t n);

        example merge(const example& other);

        friend class cb_example;
        friend class model;
    };
}