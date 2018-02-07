#pragma once

#include <vector>
#include <map>
#include <stdint.h>

namespace vwp
{
    // assume externally managed
    class features
    {
        // TODO: not sure if need for interaction, assume it's all inside weight_indecies 
        // uint64_t _namespace_offset;
        std::vector<uint64_t> _weight_indices; 
        std::vector<float> _x;

    public:
        features() = default;
        features(const features&) = default;
        features(features&&) = default;

        void append(const uint64_t* weight_indicies, const float* x, uint32_t n);

        features concat(features& f);

        class feature_iterator
        {
            const uint64_t* _weight_index;
            const float* _x;

            feature_iterator(const features& f, size_t n);

        public:
            feature_iterator(feature_iterator&&) = default;

            inline float x() { return *_x; }

            inline uint64_t weight_index() { return *_weight_index; } 

            inline feature_iterator& operator++()
            {
                ++_weight_index;
                ++_x;
            }

            feature_iterator& operator*() { return *this; }

            bool operator==(const feature_iterator& rhs) { return _x == rhs._x; }

            bool operator!=(const feature_iterator& rhs) { return _x != rhs._x; }

            friend class features;
        };

        feature_iterator begin() const;

        feature_iterator end() const;

        friend class feature_iterator;
        friend class model;
        friend class example;
    };
}