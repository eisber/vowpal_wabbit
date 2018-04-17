#include "ds_explore_py.h"
#include "ds_explore.h"

using namespace Microsoft::DecisionService::ExplorationStrategies;
using namespace std;

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

std::vector<float> ExplorationStrategies::enforce_minimum_probability(float min_prob, std::vector<float> probability_distribution)
{
  enforce_minimum_probability(min_prob, &probability_distribution[0], (uint32_t)probability_distribution.size());
  return probability_distribution;
}

std::vector<float> ExplorationStrategies::bag(std::vector<int> top_actions, int num_actions) throw(std::out_of_range)
{
  std::vector<float> probs(num_actions);
  std::vector<uint32_t> top_actions_uint(top_actions.begin(), top_actions.end());

  bag(&top_actions_uint[0], (uint32_t)top_actions_uint.size(), &probs[0], (uint32_t)num_actions);

  return probs;
}

std::vector<float> ExplorationStrategies::softmax(float lambda, std::vector<float> scores) throw(std::out_of_range)
{
  std::vector<float> probs(scores.size());
  softmax(lambda, &scores[0], &probs[0], (uint32_t)scores.size());
  return probs;
}
