#include "ds_explore.h"

#include <stdexcept>
#include <algorithm>
#include <numeric>
#include <cstring>

using namespace std;

namespace Microsoft {
  namespace DecisionService {
    namespace ExplorationStrategies {

      void epsilon_greedy(float epsilon, uint32_t top_action, float* probability_distribution, uint32_t num_actions)
      {
        if (num_actions == 0)
          return;

        if (top_action >= num_actions)
          throw out_of_range("top_action must be smaller than num_actions");

        float prob = epsilon / (float)num_actions;

        // size & initialize vector to prob
        float* prob_end = probability_distribution + num_actions;
        for (float* d = probability_distribution; d != prob_end; ++d)
          *d = prob;

        probability_distribution[top_action] += 1.f - epsilon;
      }

      void softmax(float lambda, float* scores, float* probability_distribution, uint32_t num_actions)
      {
        if (num_actions == 0)
          return;

        float norm = 0.;
        float max_score = *std::max_element(scores, scores + num_actions);

        float* prob_end = probability_distribution + num_actions;
        for (float *d = probability_distribution, *s = scores; d != prob_end; ++d, ++s)
        {
          float prob = exp(lambda*(*s - max_score));
          norm += prob;

          *d = prob;
        }

        // normalize
        if (norm > 0)
          for (float* d = probability_distribution; d != prob_end; ++d)
            *d /= norm;
      }

      void bag(uint32_t* top_actions, uint32_t num_models, float* probability_distribution, uint32_t num_actions)
      {
        if (num_actions == 0)
          return;

        if (num_models == 0)
          throw out_of_range("must supply at least one top_action from a model");

        // determine probability per model
        float prob = 1.f / (float)num_models;

        memset(probability_distribution, 0, num_actions);

        uint32_t* top_actions_end = top_actions + num_models;
        for (uint32_t* ta = top_actions; ta != top_actions_end; ++ta)
        {
          uint32_t top = *ta;
          if (top >= num_actions)
            throw out_of_range("top action is larger than num_action");

          probability_distribution[*ta] += prob;
        }
      }

      void enforce_minimum_probability(float min_prob, float* probability_distribution, uint32_t num_actions)
      {
        float* prob_end = probability_distribution + num_actions;
        bool zeros = false;

        //input: a probability distribution
        //output: a probability distribution with all events having probability > min_prob.  This includes events with probability 0 if zeros = true
        if (min_prob > 0.999) // uniform exploration
        {
          size_t support_size = num_actions;
          if (!zeros)
          {
            for (float* d = probability_distribution; d != prob_end; ++d)
              if (*d == 0)
                support_size--;
          }

          for (float* d = probability_distribution; d != prob_end; ++d)
            if (zeros || *d > 0)
              *d = 1.f / support_size;

          return;
        }

        min_prob /= num_actions;
        float touched_mass = 0.;
        float untouched_mass = 0.;

        for (float* d = probability_distribution; d != prob_end; ++d)
        {
          float& prob = *d;
          if ((prob > 0 || (prob == 0 && zeros)) && prob <= min_prob)
          {
            touched_mass += min_prob;
            prob = min_prob;
          }
          else
            untouched_mass += prob;
        }

        if (touched_mass > 0.)
        {
          if (touched_mass > 0.999)
            throw invalid_argument("Cannot safety this distribution");

          float ratio = (1.f - touched_mass) / untouched_mass;
          for (float* d = probability_distribution; d != prob_end; ++d)
            if (*d > min_prob)
              *d *= ratio;
        }
      }
    }
  }
}
