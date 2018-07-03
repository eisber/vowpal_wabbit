#include "explore.h"

// use extern "C" to disable C++ name mangling
extern "C" __declspec(dllexport) int generate_epsilon_greedy(float epsilon, uint32_t top_action, float* pdf, uint32_t size)
{
	return exploration::generate_epsilon_greedy(epsilon, top_action, pdf, pdf + size);
}

extern "C" __declspec(dllexport) int generate_softmax(float lambda, float* scores, uint32_t scores_size, float* pdf, uint32_t pdf_size)
{
	return exploration::generate_softmax(lambda, scores, scores + scores_size, pdf, pdf + pdf_size);
}

extern "C" __declspec(dllexport) int generate_bag(uint32_t* top_actions, uint32_t top_actions_size, float* pdf, uint32_t pdf_size)
{
	return exploration::generate_bag(top_actions, top_actions + top_actions_size, pdf, pdf + pdf_size);
}

extern "C" __declspec(dllexport) int sample_after_normalizing(const char* seed,
	float* pdf, uint32_t pdf_size, uint32_t& chosen_index)
{
	return exploration::sample_after_normalizing(seed, pdf, pdf + pdf_size, chosen_index);
}
