#pragma once

#include "reductions.h"
#include <string>
#include <vector>
#include <functional>

//class ILearnerFactory
//{
//public:
//	virtual po::options_description* options() = 0;
//
//	virtual Learner* create() = 0;
//};

class Learner
{
protected:
	vw& _all;
	size_t _weights; //this stores the number of "weight vectors" required by the learner.
	size_t _increment; // TODO: both uint64_t

	Learner(vw& all, size_t weights, size_t increment)
		: _all(all), _weights(weights), _increment(increment)
	{
		// move to learner impl.
		// learner
		//ret.weights = 1;
		//ret.increment = params_per_weight;
	}

public:
	size_t increment()
	{
		return _increment;
	}

	virtual void save_load(io_buf& buf, bool read, bool text)
	{ }

	// note: this is not auto-recursion
	virtual void finish_example(example& ec)
	{ 
		output_and_account_example(_all, ec);
		// TODO: VW::finish_example(_all, &ec);
	}

	virtual void end_pass() 
	{ }

	// TODO: document call sequence
	virtual void end_examples()
	{ }

	// TODO: unclear what this does (compare to init()
	virtual void init_driver()
	{ }

	//// returns reduction-base
	//// TODO: maybe move to Reduction
	//virtual Learner* base() = 0;
};

class IArguments
{
public:
	virtual po::options_description* options() = 0;

	virtual Learner* create(vw& all) = 0;

	// optional dependencies
	virtual const char** dependencies()
	{
		return nullptr;
	}
};

template<typename TDerived, typename TLearner>
class Arguments : public IArguments
{
public:
	Learner* create(vw& all)
	{
		return new TLearner(static_cast<TDerived&>(*this), all);
	}
};

template<typename TPrediction, typename TLabel>
class TypedLearner;

// convention
// method name: _
// class names: _, lower case
// private/protected: prefix with _
// template paramters uppercase with _
// typed_learner_explicit_vtable
template<typename TPrediction, typename TLabel>
class TypedLearnerVTable
{
public:
	typedef void(*PredictOrLearnMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, TPrediction& pred, TLabel& label);
	typedef void(*MultiPredictMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions);
	typedef void(*UpdateMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t i);
	typedef float(*SensitivityMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t i);

	PredictOrLearnMethod learn_method = nullptr;
	PredictOrLearnMethod predict_method = nullptr;
	MultiPredictMethod multi_predict_method = nullptr;
	UpdateMethod update_method = nullptr;
	SensitivityMethod sensitivity_method = nullptr;
};

template<typename TPrediction, typename TLabel>
class TypedLearner: public Learner
{
public:
	// typedef 
private:
	const TypedLearnerVTable<TPrediction, TLabel>& _vtable;

protected:
	TypedLearner(vw& all, size_t weights, size_t increment, const TypedLearnerVTable<TPrediction, TLabel> vtable) :
		Learner(all, weights, increment),
		_vtable(vtable)
	{ }

public:
	TypedLearnerVTable<TPrediction, TLabel> vtable() { return _vtable; }
};

template<typename TDerived, typename TPrediction, typename TLabel, void(TDerived::*M)(example&, TPrediction&, TLabel&)>
inline void predict_or_learn_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, TPrediction& pred, TLabel& label)
{
	// invoke the most derived implementation of predict_or_learn_impl
	// this method is only called from the base class and "that" == "this"
	(static_cast<TDerived&>(that).*M)(ec, pred, label);
}

template<typename TDerived, typename TPrediction, typename TLabel, void(TDerived::*M)(example&, size_t, size_t, TPrediction*, TLabel&, bool)>
inline void multi_predict_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions)
{
	// invoke the most derived implementation of predict_or_learn_impl
	// this method is only called from the base class and "that" == "this"
	(static_cast<TDerived&>(that).*M)(ec, lo, count, pred, label, finalize_predictions);
}

template<typename TDerived, typename TPrediction, typename TLabel, void(TDerived::*M)(example&, size_t)>
static inline void update_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, size_t i)
{
	// invoke the most derived implementation of predict_or_learn_impl
	// this method is only called from the base class and "that" == "this"
	(static_cast<TDerived&>(that).*M)(ec, i);
}

template<typename TDerived, typename TPrediction, typename TLabel, float(TDerived::*M)(example&, size_t)>
static inline float sensitivity_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, size_t i)
{
	// invoke the most derived implementation of predict_or_learn_impl
	// this method is only called from the base class and "that" == "this"
	return (static_cast<TDerived&>(that).*M)(ec, i);
}

template<typename TDerived, typename TArgs, typename TPrediction, typename TLabel>
class LearnerBase : public TypedLearner<TPrediction, TLabel>
{
	typedef LearnerBase<TDerived, TArgs, TPrediction, TLabel> TThis;
protected:
	template<bool is_learn>
	void predict_or_learn(example& ec, TPrediction& pred, TLabel& label)
	{

	}

	void multi_predict(example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions)
	{
		// TODO: undo ft_offset increment
		for (size_t c = 0; c<count; c++)
		{
			// TODO: look into more detail on how to structure pred returning
			TPrediction temp_pred;
			static_cast<TDerived&>(*this).template predict_or_learn<false>(ec, temp_pred, label);
			// learn_fd.predict_f(learn_fd.data, *learn_fd.base, ec);

			// TODO: the below is just broken!!!!
			assert(false);
			//if (finalize_predictions) pred[c] = ec.pred; // TODO: this breaks for complex labels because = doesn't do deep copy!
			//else                      pred[c].scalar = ec.partial_prediction;
			ec.ft_offset += (uint32_t)Learner::_increment;
		}
	}

	void update(example& ec, size_t i)
	{ 
		// TODO: defaults to learn?
	}

	float sensitivity(example& ec, size_t i)
	{
		return 0.; // see noop_sensitivity
	}

public:

	// need to re-declare for gcc
	typedef void(*PredictOrLearnMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, TPrediction& pred, TLabel& label);
	typedef void(*MultiPredictMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions);
	typedef void(*UpdateMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t i);
	typedef float(*SensitivityMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t i);

protected:
	TArgs& _args;
	
public:
	// get most derived implementation of Learn & Predict
	LearnerBase(TArgs& args, vw& all, size_t weights = 1, size_t increment = 0)
		: TypedLearner<TPrediction, TLabel>(
			all, weights, increment,
			{
				&predict_or_learn_dispatch<TDerived, TPrediction, TLabel, &TDerived::template predict_or_learn<true>>,
				&predict_or_learn_dispatch<TDerived, TPrediction, TLabel, &TDerived::template predict_or_learn<false>>,
				&multi_predict_dispatch<TDerived, TPrediction, TLabel, &TDerived::multi_predict>,
				&update_dispatch<TDerived, TPrediction, TLabel, &TDerived::update>,
				&sensitivity_dispatch<TDerived, TPrediction, TLabel, &TDerived::sensitivity>
			}),
		_args(args)
	{ }

	LearnerBase(TArgs& args, vw& all, TypedLearnerVTable<TPrediction, TLabel> vtable, size_t weights = 1, size_t increment = 0)
		: TypedLearner<TPrediction, TLabel>(
			all, weights, increment, vtable),
		_args(args)
	{ }

	/*LearnerBase(vw& all, size_t weights = 1, size_t increment = 0)
		: TypedLearner<TPrediction, TLabel>(
			all, weights, increment,
			FillDefaults(TDerived::GetVTable(args)))
	{ }*/

	//// iterate through one namespace (or its part), callback function T(some_data_R, feature_value_x, feature_weight)
	//template <class R, void(TDerived::*T)(R&, const float, float&)>
	//inline void foreach_feature(weight_parameters& weights, features& fs, R& dat, uint64_t offset = 0, float mult = 1.)
	//{
	//	for (features::iterator& f : fs)
	//		// T(dat, mult*f.value(), weights[(f.index() + offset)]);
	//		(static_cast<TDerived&>(that).T)(dat, mult*f.value(), weights[(f.index() + offset)]);
	//}

	//// iterate through one namespace (or its part), callback function T(some_data_R, feature_value_x, feature_index)
	//template <class R, void(TDerived::*T)(R&, float, uint64_t)>
	//void foreach_feature(features& fs, R&dat, uint64_t offset = 0, float mult = 1.)
	//{
	//	for (features::iterator& f : fs)
	//		(static_cast<TDerived&>(*this).T)(dat, mult*f.value(), f.index() + offset);
	//		// T(dat, mult*f.value(), f.index() + offset);
	//}

	//// iterate through all namespaces and quadratic&cubic features, callback function T(some_data_R, feature_value_x, S)
	//// where S is EITHER float& feature_weight OR uint64_t feature_index
	//template <class R, class S, void(TDerived::*T)(R&, float, S)>
	//inline void foreach_feature(example& ec, R& dat)
	//{
	//	uint64_t offset = ec.ft_offset;

	//	for (features& f : ec)
	//		foreach_feature<R, T>(f, dat, offset);

	//	// TODO INTERACTIONS::generate_interactions<R, S, T>(ec, dat);
	//}

	//// iterate through all namespaces and quadratic&cubic features, callback function T(some_data_R, feature_value_x, feature_weight)
	//template <class R, void(TDerived::*T)(R&, float, float&)>
	//inline void foreach_feature(example& ec, R& dat)
	//{
	//	foreach_feature<R, float&, T>(ec, dat);
	//}



	template<typename ...Args>
	class foreach_feature_temp
	{
	private:
		TDerived& _that;
	public:
		foreach_feature_temp(TDerived& that) : _that(that)
		{ } 

		template <void(TDerived::*T)(float, float&, Args...)>
		inline void values(example& ec, Args... args)
		{
			weight_parameters weights = _that._all.weights;
			float mult = 1.;
			uint64_t offset = ec.ft_offset;

			for (features& fs : ec)
				for (features::iterator& f : fs)
					(_that.*T)(mult*f.value(), weights[(f.index() + offset)], args...);
		}

		template <void(TDerived::*T)(float, uint64_t, Args...)>
		inline void inidices(example& ec, Args... args)
		{
			weight_parameters weights = _that._all.weights;
			float mult = 1.;
			uint64_t offset = ec.ft_offset;

			for (features& fs : ec)
				for (features::iterator& f : fs)
					(_that.*T)(mult*f.value(), f.index() + offset, args...);
		}
	};

	template<typename ...Args>
	foreach_feature_temp<Args...> foreach_feature(TDerived& that, Args... args)
	{
		return foreach_feature_temp<Args...>(that);
	}

	// need to use a macro to be able to pass __VA_ARGS__ twice. 
	// 1. let type inference figure out the types for foreach_feature<>
	// 2. pass the actual arguments
#define foreach_feature_m(method, ec, ...) foreach_feature<>(*this, __VA_ARGS__).values<&method>(ec, __VA_ARGS__)


	template<typename ...Args>
	class ff
	{
	public:
		template <void(TDerived::*T)(float, float&, Args...)>
		static inline void foreach_feature(TDerived& that, example& ec, Args... args)
		{
			weight_parameters weights = that._all.weights;
			float mult = 1.;
			uint64_t offset = ec.ft_offset;

			for (features& fs : ec)
				for (features::iterator& f : fs)
					(that.*T)(mult*f.value(), weights[(f.index() + offset)], args...);
					// Tabc(mult*f.value(), f.index() + offset, std::forward<Arguments>(args)...);
		}

		template <void(TDerived::*T)(float, uint64_t, Args...)>
		static inline void foreach_feature(TDerived& that, example& ec, Args... args)
		{
			float mult = 1.;
			uint64_t offset = ec.ft_offset;

			for (features& fs : ec)
				for (features::iterator& f : fs)
					(that.*T)(mult*f.value(), f.index() + offset, args...);
			// Tabc(mult*f.value(), f.index() + offset, std::forward<Arguments>(args)...);
		}
	};
};

class IReduction
{
public:
	virtual bool try_link(Learner& learner) = 0;

	virtual Learner* base() = 0;
};

template<typename T>
class RestoreValueOnReturn
{
private:
	T& _value;
	T _value_initial;

public:
	RestoreValueOnReturn(T& value) : _value(value), _value_initial(value)
	{ }

	~RestoreValueOnReturn()
	{
		_value = _value_initial;
	}
};

// TODO: name, save in scope, 
#define RESTORE_VALUE_ON_RETURN(x) RestoreValueOnReturn<decltype(x)> rvor_##__LINE__(x)

template<typename TDerived, typename TArgs, typename TPrediction, typename TLabel, typename TPredictionOfBase, typename TLabelOfBase>
class Reduction : public LearnerBase<TDerived, TArgs, TPrediction, TLabel>, public IReduction
{
private:
	typedef TypedLearner<TPredictionOfBase, TLabelOfBase> TBaseLearner;
	TBaseLearner* _base = nullptr;
	
	TypedLearnerVTable<TPredictionOfBase, TLabelOfBase> _base_vtable;

protected:
	Reduction(TArgs args, vw& all, size_t weights = 1, size_t increment = 0)
		: LearnerBase<TDerived, TArgs, TPrediction, TLabel>(args, all, weights, increment)
	{ }

	template<bool is_learn>
	void base_predict_or_learn(example& ec, TPredictionOfBase& pred, TLabelOfBase& label, size_t i=0)
	{
		if (is_learn)
			base_learn(ec, pred, label, i);
		else
			base_predict(ec, pred, label, i);
	}

	inline void base_learn(example& ec, TPredictionOfBase& pred, TLabel& label, size_t i = 0)
	{
		// RestoreValueOnReturn<uint64_t> x(ec.ft_offset);
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += Learner::_increment * i;
		(*_base_vtable.learn_method)(*_base, ec, pred, label);
	}

	inline void base_predict(example& ec, TPredictionOfBase& pred, TLabel& label, size_t i = 0)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += Learner::_increment * i;
		(*_base_vtable.predict_method)(*_base, ec, pred, label);
	}

	inline void base_multi_predict(TBaseLearner&, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += Learner::_increment * lo;
		(*_base_vtable.multi_predict_method)(*_base, ec, lo, count, pred, label, finalize_predictions);
	}

	inline void base_update(TBaseLearner&, example& ec, size_t i = 0)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += Learner::_increment; // TODO *lo;
		(*_base_vtable.update_method)(*_base, ec, i);
	}

	inline float base_sensitivity(TBaseLearner&, example& ec, size_t i = 0)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += Learner::_increment; //TODO *lo;
		return (*_base_vtable.sensitivity_method)(*_base, ec, i);
	}
public:
	bool try_link(Learner& learner)
	{
		// verify type match
		_base = dynamic_cast<TBaseLearner*>(&learner);

		// TODO: exception?
		if (_base == nullptr)
			return false;

		_base_vtable = _base->vtable();

		// REMOVEME: as done in init_learner()
		Learner::_increment = _base->increment() * Learner::_weights;

		return true;
	}

	virtual Learner* base()
	{
		// Note: instead of recursing up through calling the base member impl
		// and therfore relying on implementation to call Reduction::end_pass()
		// let the driver code iterate through the reduction stack.
		// TODO: we could expose the function pointers and build a shortcut loop
		return _base;
	}
};

#include <utility>
#include "vw_exception.h"

// helper to support template parameter expansion from dynamic variables
// 
template<typename T, typename TFactory, bool ...ArgsGrow>
class template_expansion
{
public:
	template<typename S>
	static T expand(S cond)
	{
		if (cond)
			return TFactory::template create<ArgsGrow..., true>();
		else
			return TFactory::template create<ArgsGrow..., false>();
	}

	template<typename TB, typename ...Arguments>
	static T expand(TB cond, Arguments... args)
	{
		if (cond) // I need to specify count(Arguments) - 1, not Arguments
			return template_expansion<T, TFactory, ArgsGrow..., true>::template expand<>(std::forward<Arguments>(args)...);
		else
			return template_expansion<T, TFactory, ArgsGrow..., false>::template expand<>(std::forward<Arguments>(args)...);
	}
};