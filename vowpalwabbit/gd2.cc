#include "vw_base.h"

class GDLearnerFactory : ILearnerFactory
{
private:
	float sparse_l2 = 0.;
	bool adaptive = false;
	bool invariant = false;
	vw& _all;

public:
	GDLearnerFactory(vw& all) : _all(all)
	{ }

	po::options_description* options()
	{
		auto opts = new po::options_description("Gradient Descent options");
		//opts->add_options()("autolink", po::value<uint32_t>(&d), "create link function with polynomial d");
		//return opts;

		opts->add_options()
			("sgd", po::value<decltype(sparse_l2)>(&sparse_l2), "use regular stochastic gradient descent update.")
			("adaptive", po::value<decltype(adaptive)>(&adaptive)->default_value(false), "use adaptive, individual learning rates.")
			("invariant", "use safe/importance aware updates.")
			("normalized", "use per feature normalized updates")
			("sparse_l2", po::value<float>()->default_value(0.f), "use per feature normalized updates");

	}

	Learner* create()
	{

	}
};

template<bool sparse_l2, bool invariant, bool sqrt_rate, uint64_t adaptive, uint64_t normalized, uint64_t spare, uint64_t next>
class GD : LearnerBase<GD<sparse_l2, invariant, sqrt_rate, adapative, normalized, spare, next>, float, float>
{
	//double normalized_sum_norm_x;
	double total_weight = 0;
	size_t no_win_counter = 0;
	size_t early_stop_thres = 3;
	float initial_constant = 0.;
	float neg_norm_power = 0.;
	float neg_power_t = 0.;
	float update_multiplier = 0.;
	//void(*predict)(gd&, base_learner&, example&);
	//void(*learn)(gd&, base_learner&, example&);
	//void(*update)(gd&, base_learner&, example&);
	//float(*sensitivity)(gd&, base_learner&, example&);
	//void(*multipredict)(gd&, base_learner&, example&, size_t, size_t, polyprediction*, bool);
	bool normalized = false;

public:
	GD(vw& all)
	{ 
		all.normalized_sum_norm_x = 0;

	}
};


// 2 step creation process
// - create factory first
// - factory creates actual learn instance, allows for templatization of learner based on command args

ILearnerFactory* gd_createfactory(vw& all) {
	return new GDLearnerFactory(); 
}