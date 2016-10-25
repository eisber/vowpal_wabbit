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
		ec.total_sum_feat_sq += fs.sum_feat_sq;

		//base_predict_or_learn<is_learn>(ec, pred, label);
		base_predict_or_learn<true>(ec, pred, label);

		ec.total_sum_feat_sq -= fs.sum_feat_sq;
		fs.erase();
		ec.indices.pop();
	}

public:
	// options object
	virtual po::options_description* options()
	{
		auto opts = new po::options_description();
		opts->add_options()("autolink", po::value<uint32_t>(&d), "create link function with polynomial d");
		return opts;
	}

	virtual void init(vw& all)
	{ 
		stride_shift = all.weights.stride_shift();
	}
};

Learner* autolink_create() { return new AutoLink(); }


// 2nd hierarchy path to enable learn_impl, predict_impl vs predict_or_learn<t/f>
//// helper for reductions
//template<typename Derived>
//class ReductionBase : public Reduction
//{
//private:
//
//protected:
//	// get most derived implementation of Learn & Predict
//	ReductionBase() : Reduction(Derived::learn_impl, Derived::predict_impl)
//	{
//	}
//
//	void learn_impl(example& ec)
//	{
//		// TODO: default 
//	}
//
//	void predict_impl(example& ec)
//	{
//		// TODO: default
//	}
//};

/*

class features_builder
{

};

class example_holder
{
private:
example& _ec;
public:
example_holder(example& ec) : _ec(ec) // list of namespaces
{
// push to indicies
// +sum_sq
}

~example_holder()
{
// pop to indicies
// -sum_sq
}

operator example&()
{
return _ec;
}
};

class example_builder
{
private:
example& _ec;
// TODO: this should be cached in between invocations
v_array<features_builder> _x;

public:
example_builder(example& ec) : _ec(ec)
{}

~example_builder()
{

}

features_builder create_namespace()
{
// record namespaces
}

example_holder create()
{
return example_holder(_ec);
}
};
*/

// holder code
//// #1
////base.predict(ec);
////float base_pred = ec.pred.scalar;
//base_predict(ec, pred, label);

//// #2
//// add features of label
////ec.indices.push_back(autolink_namespace);
////features& fs = ec.feature_space[autolink_namespace];

//// Resource allocation is initialization
//// feature_namespace invokes fs.erase() & ec.inidices.pop() in destructor
//// pro: works with exception
//example_builder builder(ec);

//// not yet pushed to indicies
//feature_builder fs = builder.create_namespace(autolink_namespace);
//float base_pred = pred;
//for (size_t i = 0; i < d; i++)
//	if (pred != 0.)
//	{
//		fs.push_back(base_pred, autoconstant + (i << stride_shift));
//		// base_pred *= ec.pred.scalar;
//		base_pred *= pred;
//	}

//// #3
////ec.total_sum_feat_sq += fs.sum_feat_sq;
////if (is_learn)
////	base_learn(ec, pred);
////else
////	base_predict(ec, pred);

//// builder.create(), performs ec.total_sum_feat_sq += fs.sum_feat_sq;
//// EXPANDED form
////example_holder exh = builder.create();
////base_predict_or_learn(exh, pred);
//
//base_predict_or_learn<is_learn>(builder.create(), pred);

//// #4
//// ec.total_sum_feat_sq -= fs.sum_feat_sq;
//
//// automatically invoked
//// (builder.create() temporary obj).~example_holder();

//// #5
////fs.erase();
////ec.indices.pop();

//// automatically invoked
//// fs.~feature_namespace_builder();