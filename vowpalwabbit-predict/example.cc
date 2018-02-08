#include "example.h"

namespace vwp 
{
    const uint64_t constant = 11650396;
    const unsigned char constant_namespace = 128;

    example::example()
    {
        // TODO: there is an inconsistency in full-VW
        // - when loading a model, --noconstant is not persisted, and thus constant feature is always generated
        // - if the weight is 0, no problem, but in case of hash collisions

        // initialize with constant features
        features constant_features;
        float value = 1.f;
        constant_features.append(&constant, &value, 1);

        _namespaces.insert(std::make_pair(constant_namespace, constant_features));
    }

    void example::add_namespace(unsigned char feature_group, uint64_t weight_index, float x)
    {
        add_namespace(feature_group, &weight_index, &x, 1);
    }

    void example::add_namespace(unsigned char feature_group, const uint64_t* weight_indicies, const float* x, uint32_t n)
    {
        // find existing namespace
        auto existing = _namespaces.find(feature_group);
        if (existing != _namespaces.end())
        {
            existing->second.append(weight_indicies, x, n);
            return;
        } 

        // create new one
        features f;
        f.append(weight_indicies, x, n);

        _namespaces.insert(std::make_pair(feature_group, f));
    }

    example example::merge(const example& other)
    {
        example ex_new(*this);

        for(auto& kv : other._namespaces)
        {
            ex_new.add_namespace(kv.first, 
                &kv.second._weight_indices[0],
                &kv.second._x[0], 
                kv.second._x.size());
        } 

        return ex_new;
    }

    std::ostream& operator<<(std::ostream& os, const example& ex)
    {
        if (ex._namespaces.empty())
            return os;

        os << '{';

        size_t prepend_comma = false;
        for(auto& ns : ex._namespaces)
        {
            // exclude constant namespace
            if (ns.first == constant_namespace)
                continue;

            if (prepend_comma)
                os << ',';
            else 
                prepend_comma = true;

            os  << '\"' << ns.first << "\":{" << ns.second << '}';
        }

        return os << '}';
    }
}        
