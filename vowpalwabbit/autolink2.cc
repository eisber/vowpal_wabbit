#include "reductions.h"

#include "vw_base.h"

class AutoLink : public Reduction<AutoLink, float, polylabel, float, polylabel>
{
private:
	const int autoconstant = 524267083;
	
	uint32_t d = 0; // degree of the polynomial
	uint32_t stride_shift = 0; 

public:
	template<bool is_learn>
	void predict_or_learn(example& ec, float& pred, polylabel& label)
	{
		float base_pred;
		base_predict(ec, base_pred, label);

		// add features of label
		ec.indices.push_back(autolink_namespace);
		features& fs = ec.feature_space[autolink_namespace];
		for (size_t i = 0; i < d; i++)
			if (base_pred != 0.)
			{
				fs.push_back(base_pred, autoconstant + (i << stride_shift));
				base_pred *= ec.pred.scalar;
			}

		RESTORE_VALUE_ON_RETURN(ec.total_sum_feat_sq);
		ec.total_sum_feat_sq += fs.sum_feat_sq;

		base_predict_or_learn<is_learn>(ec, pred, label);

		// TODO: implement finally
		fs.erase();
		ec.indices.pop();
	}

public:
	AutoLink(vw& all) : Reduction(all) 
	{
		stride_shift = all.weights.stride_shift();
	}

	// options object
	virtual po::options_description* options()
	{
		auto opts = new po::options_description();
		opts->add_options()("autolink", po::value<decltype(d)>(&d), "create link function with polynomial d");
		return opts;
	}
};

Learner* autolink_create(vw& all) { return new AutoLink(all); }