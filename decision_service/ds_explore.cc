#include "ds_explore.h"

#include <algorithm>
#include <cstring>

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

    std::vector<float> ExplorationStrategies::epsilon_greedy(float epsilon, int top_action, int num_actions) throw(std::out_of_range)
    {
      if (top_action < 0)
        throw out_of_range("top_action must be greater or equal to zero");

      if (num_actions < 0)
        throw out_of_range("num_actions must be greater or equal to zero");

      std::vector<float> probs(num_actions);
      epsilon_greedy(epsilon, (uint32_t)top_action, &probs[0], (uint32_t)num_actions);
      return probs;
    }

    void ExplorationStrategies::epsilon_greedy(float epsilon, uint32_t top_action, float* probability_distribution, uint32_t num_actions)
      throw(out_of_range)
    {
      if (num_actions == 0)
        return;

      if (top_action >= num_actions)
        throw out_of_range("top_action must be smaller than num_actions");

      float prob = epsilon/(float)num_actions;

      // size & initialize vector to prob
      float* prob_end = probability_distribution + num_actions;
      for(float* d = probability_distribution;d!=prob_end;++d)
        *d = prob;

      probability_distribution[top_action] += 1.f - epsilon;
    }

    std::vector<float> ExplorationStrategies::softmax(float lambda, std::vector<float> scores) throw(std::out_of_range)
    {
      std::vector<float> probs(scores.size());
      softmax(lambda, &scores[0], &probs[0], scores.size());
      return probs;
    }

    void ExplorationStrategies::softmax(float lambda, float* scores, float* probability_distribution, uint32_t num_actions)
      throw(out_of_range)
    {
        if (num_actions == 0)
          return;

        float norm = 0.;
        float max_score = *std::max_element(scores, scores + num_actions);

        float* prob_end = probability_distribution + num_actions;
        for(float *d = probability_distribution, *s = scores;d!=prob_end;++d,++s)
        {
            float prob = exp(lambda*(*s - max_score));
            norm += prob;
             
            *d = prob;
        }

        // normalize
        for(float* d = probability_distribution;d!=prob_end;++d)
            *d /= norm;
    }

    std::vector<float> ExplorationStrategies::bag(std::vector<int> top_actions, int num_actions) throw(std::out_of_range)
    {
      std::vector<float> probs(num_actions);
      std::vector<uint32_t> top_actions_uint(top_actions.begin(), top_actions.end());

      bag(&top_actions_uint[0], top_actions_uint.size(), &probs[0], num_actions);

      return probs;
    }

    void ExplorationStrategies::bag(uint32_t* top_actions, uint32_t num_models, float* probability_distribution, uint32_t num_actions)
      throw(out_of_range)
    {
        if (num_actions == 0)
          return;

        if (num_models == 0)
          throw out_of_range("must supply at least one top_action from a model");

        // determine probability per model
        float prob = 1.f / (float)num_models;

        memset(probability_distribution, 0, num_actions);
        
        uint32_t* top_actions_end = top_actions + num_models;
        for (uint32_t* ta = top_actions;ta!=top_actions_end;++ta)
        {
          uint32_t top = *ta;
          if (top >= num_actions)
            throw out_of_range("top action is larger than num_action");

          probability_distribution[*ta] += prob;
        } 
    }

    std::vector<float> ExplorationStrategies::enforce_minimum_probability(float min_prob, std::vector<float> probability_distribution)
    {
        enforce_minimum_probability(min_prob, &probability_distribution[0], probability_distribution.size());
        return probability_distribution;
    }

    void ExplorationStrategies::enforce_minimum_probability(float min_prob, float* probability_distribution, uint32_t num_actions)
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
                for(float* d = probability_distribution;d!=prob_end;++d)
                  if (*d == 0)
                      support_size--;
            }
        
            for(float* d = probability_distribution;d!=prob_end;++d)
                if (zeros || *d > 0)
                    *d = 1.f / support_size;

            return;
        }

        min_prob /= num_actions;
        float touched_mass = 0.;
        float untouched_mass = 0.;
        
        for(float* d = probability_distribution;d!=prob_end;++d)
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
            for(float* d = probability_distribution;d!=prob_end;++d)
                if (*d > min_prob)
                    *d *= ratio;
        }
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
            return (int)choose_action(event_id, &probability_distribution[0], probability_distribution.size());
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
                  return action_index;

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

            rank(event_id, &probability_distribution[0], &action_ordering_uint[0], action_ordering_uint.size());

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
            rank_by_score(event_id, &probability_distribution[0], &action_ordering[0], &scores[0], action_ordering.size());

            return std::vector<int>(action_ordering.begin(), action_ordering.end());
          }
  }
}