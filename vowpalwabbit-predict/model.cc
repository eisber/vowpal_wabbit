#include "model.h"

#include <stdexcept>
#include <iostream>

namespace vwp 
{
    model::model(const float* weights, uint64_t n)
        : _weight_mask(n - 1), // assuming n = 1 << bits 
          _weights(weights, weights + n), 
          _contraction(1.f)
    { 
         if (!(n && !(n & (n - 1))))
            throw std::invalid_argument("model size must be of power 2");
    }

    // recursive implementation of interactions
    // computes weight_index: f3 ^ k*(f2 ^ k*f1)
    float model::predict(const example& example, const unsigned char* interaction, uint64_t weight_index, float x)
    {
        if (!*interaction)
            return predict(weight_index, x);

        // search for namespace
        auto ns = example._namespaces.find(*interaction);

        // skip if not found
        if (ns == example._namespaces.end())
            return 0;

        weight_index *= FNV_prime;
        
        float pred = 0.f;
        for (auto& f : ns->second)
            pred += predict(
                example, 
                interaction + 1, // move to next namespace
                weight_index ^ f.weight_index(), // combine feature hashes 
                x * f.x()); // create polynomial

        return pred;
    }

    float model::predict(const example& example)
    {
        // at prediction time ec.l.simple.initial=0 as set by simple_label.c:default_simple_label()
        float pred = 0.f;

        for(auto& ns : example._namespaces)
        {
            // std::cout << "namespace: " << (int)ns.first << std::endl;
            for (auto& f : ns.second)
                pred += predict(f.weight_index(), f.x());
        }

        // interactions
        for (ustring& interaction : _interactions)
            pred += predict(example, interaction.c_str());

        // audit
        // std::cout << std::endl;

        // Note: 
        // - pred is equal to partial_prediction
        // - min_label, max_label truncation is not performed
        return pred * _contraction;
    }
}