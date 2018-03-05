#include "ds_explore.h"

#include <algorithm>
#include <numeric>
#include <cstring>
#include <vector>
#include <string>

namespace Microsoft {
  namespace DecisionService {
      using namespace std;

    // IExplorer::IExplorer()
    // { }

    IExplorer::~IExplorer()
    { }

    ActionProbabilities IExplorer::explore(PredictorContainer& container)
    {
      return ActionProbabilities(0);
    }

#include "utility.h"
using namespace MultiWorldTesting;

          Sampling::Sampling() : _seed_from_app_id(0)
          { }

          Sampling::Sampling(const char* app_id)
          {
            _seed_from_app_id = app_id ? MultiWorldTesting::HashUtils::Compute_Id_Hash(app_id) : 0;
          }

          int Sampling::choose_action(const char* event_id, std::vector<float> probability_distribution)
          {
            return (int)choose_action(event_id, &probability_distribution[0], (uint32_t)probability_distribution.size());
          }

          uint32_t Sampling::choose_action(const char* event_id, float* probability_distribution, uint32_t num_actions)
          {
              float* prob_end = probability_distribution + num_actions;
            
              uint64_t seed = _seed_from_app_id + HashUtils::Compute_Id_Hash(event_id);
              PRG::prg random_generator(seed);
              float draw = random_generator.Uniform_Unit_Interval();

              // Create a discrete_distribution based on the returned weights. This class handles the
              // case where the sum of the weights is < or > 1, by normalizing agains the sum.
              float total = std::accumulate(probability_distribution, prob_end, 0.f);

              if (total == 0)
                throw std::invalid_argument("At least one score must be positive.");

              // TODO: the C# version also checks for sum to 1
              draw *= total;
              if (draw > total) //make very sure that draw can not be greater than total.
                draw = total;

              size_t action_index = 0;
              float sum = 0.f;
              for(float* d = probability_distribution;d!=prob_end;++d)
              {
                sum += *d;
                if (sum > draw)
                  return (uint32_t)action_index;

                  action_index++;
              }
              
              // return the last index
              return num_actions - 1;
          }

          std::vector<int> Sampling::rank(const char* event_id, std::vector<float> probability_distribution, std::vector<int> action_ordering)
          {
            if (probability_distribution.size() != action_ordering.size())
              throw std::invalid_argument("probability_distribution and action_ordering must be of equal length");

            std::vector<uint32_t> action_ordering_uint(action_ordering.begin(), action_ordering.end());

            rank(event_id, &probability_distribution[0], &action_ordering_uint[0], (uint32_t)action_ordering_uint.size());

            // TODO: copy?
            for(size_t i=0;i<action_ordering.size();++i)
              action_ordering[i] = (int)action_ordering_uint[i];

            return action_ordering;
          }

          void Sampling::rank(const char* event_id, float* probability_distribution, uint32_t* action_ordering, uint32_t num_actions)
          {
            uint32_t index = choose_action(event_id, probability_distribution, num_actions);

            // swap top slot with chosen ne  
            iter_swap(action_ordering, action_ordering + index);
          }

          void Sampling::rank_by_score(const char* event_id, float* probability_distribution, uint32_t* action_ordering, float* scores, uint32_t num_actions)
          {
            // initialize from 0...n-1
            for (uint32_t action = 0;action!=num_actions;++action)
                action_ordering[action] = action;

            // make keep order even for equal scores
            std::stable_sort(action_ordering, action_ordering + num_actions, [=](const auto& a, const auto& b) {
              return scores[a] < scores[b];   
            });

            rank(event_id, probability_distribution, action_ordering, num_actions);
          }

          std::vector<int> Sampling::rank_by_score(const char* event_id, std::vector<float> probability_distribution, std::vector<float> scores)
          {
            std::vector<uint32_t> action_ordering(scores.size());
            rank_by_score(event_id, &probability_distribution[0], &action_ordering[0], &scores[0], (uint32_t)action_ordering.size());

            return std::vector<int>(action_ordering.begin(), action_ordering.end());
          }
  }
}
