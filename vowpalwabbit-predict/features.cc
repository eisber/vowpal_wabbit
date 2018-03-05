#include "features.h"
#include <iterator>

namespace vwp {

    void features::append(const uint64_t* weight_indicies, const float* x, uint32_t n)
    {
        _weight_indices.insert(_weight_indices.end(), weight_indicies, weight_indicies + n);
        _x.insert(_x.end(), x, x + n);
    }

    features features::concat(features& f)
    {
        features c;

        c.append(&_weight_indices[0], &_x[0], (uint32_t)_x.size());
        c.append(&f._weight_indices[0], &f._x[0], (uint32_t)f._x.size());

        return c;
    }

    features::feature_iterator features::begin() const
    {
        return features::feature_iterator(*this, 0);
    }

    features::feature_iterator features::end() const
    {
        return feature_iterator(*this, _weight_indices.size());
    }

    features::feature_iterator::feature_iterator(const features& f, size_t n)
        : _weight_index(&f._weight_indices[0] + n), _x(&f._x[0] + n)
    { }

    features::feature_iterator features::feature_iterator::operator-(int n) const
    {
        features::feature_iterator lhs(*this);
        lhs._weight_index -= n;
        lhs._x -= n;

        return lhs;
    }

    std::ostream& operator<<(std::ostream& os, const features::feature_iterator& feature)
    {
        return os << '\"' << feature.weight_index() << "\":" << feature.x();
    }

    std::ostream& operator<<(std::ostream& os, const features& f)
    {
        if (f._x.empty())
            return os;

        std::copy(f.begin(), 
            f.end() - 1, 
            std::ostream_iterator<features::feature_iterator>(os, ","));
            
        return os << *(f.end() - 1);
    }
}
