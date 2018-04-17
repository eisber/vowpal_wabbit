#pragma once

#include <vector>
#include <stdexcept>

class ExplorationStrategies
{
public:
  ExplorationStrategies() = delete;

  // python signature
  std::vector<float> epsilon_greedy(float epsilon, int top_action, int num_actions) throw(std::out_of_range);

  std::vector<float> softmax(float lambda, std::vector<float> scores) throw(std::out_of_range);

  std::vector<float> bag(std::vector<int> top_actions, int num_actions) throw(std::out_of_range);

  std::vector<float> enforce_minimum_probability(float min_prob, std::vector<float> probability_distribution);
};
