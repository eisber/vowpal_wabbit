#pragma once

#include <vector>
#include <map>
#include <stdint.h>
#include <ostream>

#include "features.h"

namespace vwp
{
    typedef std::map<unsigned, features> namespace_feature_map_t;
    
    class example 
    {
        namespace_feature_map_t _namespaces;

    public:
        example();
        example(example&&) = default;
        example(const example&) = default;

        void add_namespace(unsigned char feature_group, uint64_t weight_index, float x = 1.f);

        void add_namespace(unsigned char feature_group, const uint64_t* weight_indicies, const float* x, uint32_t n);

        example merge(const example& other);

        friend class cb_example;
        friend class model;
        friend std::ostream& operator<<(std::ostream& os, const example& ex);
    };

    std::ostream& operator<<(std::ostream& os, const example& ex);
}