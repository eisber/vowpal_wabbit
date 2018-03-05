#pragma once

#include <vector>
#include <iterator>
#include <map>
#include <stdint.h>
#include <ostream>

namespace vwp
{
    class features
    {
        std::vector<uint64_t> _weight_indices; 
        std::vector<float> _x;

    public:
        features() = default;
        features(const features&) = default;
        features(features&&) = default;

        void append(const uint64_t* weight_indicies, const float* x, uint32_t n);

        features concat(features& f);

        class feature_iterator : public std::iterator<std::input_iterator_tag, feature_iterator, long, feature_iterator*, feature_iterator>
        {
            const uint64_t* _weight_index;
            const float* _x;

            feature_iterator(const features& f, size_t n);

        public:
            feature_iterator(const feature_iterator&) = default;
            feature_iterator(feature_iterator&&) = default;

            inline float x() const { return *_x; }

            inline uint64_t weight_index() const { return *_weight_index; } 

            inline feature_iterator& operator++()
            {
                ++_weight_index;
                ++_x;

                return *this;
            }

            feature_iterator& operator*() { return *this; }

            feature_iterator operator-(int) const;

            bool operator==(const feature_iterator& rhs) { return _x == rhs._x; }

            bool operator!=(const feature_iterator& rhs) { return _x != rhs._x; }

            friend class features;
        };

        feature_iterator begin() const;

        feature_iterator end() const;

        friend class feature_iterator;
        friend class model;
        friend class example;

        friend std::ostream& operator<<(std::ostream& os, const features& f);
    };

    std::ostream& operator<<(std::ostream& os, const features& f);
}
