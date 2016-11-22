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

	// returns reduction-base
	// TODO: maybe move to Reduction
	virtual Learner* base() = 0;
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

	PredictOrLearnMethod learn_method;
	PredictOrLearnMethod predict_method;
	MultiPredictMethod multi_predict_method;
	UpdateMethod update_method;
	SensitivityMethod sensitivity_method;
};

template<typename TPrediction, typename TLabel>
class TypedLearner: public Learner
{
public:
	// typedef 
private:
	const TypedLearnerVTable<TPrediction, TLabel>& _vtable;

protected:
	TypedLearner(vw& all, size_t weights, size_t increment, const TypedLearnerVTable<TPrediction, TLabel>& vtable) :
		Learner(all, weights, increment),
		_vtable(vtable)
	{ }

public:
	TypedLearnerVTable<TPrediction, TLabel> vtable() { return _vtable; }
};

template<typename TDerived, typename TPrediction, typename TLabel>
class LearnerBase : public TypedLearner<TPrediction, TLabel>
{
private:
	template<bool is_learn>
	static inline void predict_or_learn_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, TPrediction& pred, TLabel& label)
	{
		// invoke the most derived implementation of predict_or_learn_impl
		// this method is only called from the base class and "that" == "this"
		static_cast<TDerived&>(that).template predict_or_learn<is_learn>(ec, pred, label);
	}

	// TODO: template specialization for is_learn = false (predict) -> learn

	static inline void multi_predict_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions)
	{
		// invoke the most derived implementation of predict_or_learn_impl
		// this method is only called from the base class and "that" == "this"
		static_cast<TDerived&>(that).multi_predict(ec, lo, count, pred, label, finalize_predictions);
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
			ec.ft_offset += (uint32_t)_increment;
		}
	}

	static inline void update_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, size_t i)
	{
		// invoke the most derived implementation of predict_or_learn_impl
		// this method is only called from the base class and "that" == "this"
		static_cast<TDerived&>(that).update(ec, i);
	}

	void update(example& ec, size_t i)
	{ 
		// TODO: defaults to learn
	}

	static inline float sensitivity_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, size_t i)
	{
		// invoke the most derived implementation of predict_or_learn_impl
		// this method is only called from the base class and "that" == "this"
		return static_cast<TDerived&>(that).sensitivity(ec, i);
	}

	float sensitivity(example& ec, size_t i)
	{
		return 0.; // see noop_sensitivity
	}

	// need to re-declare for gcc
	typedef void(*PredictOrLearnMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, TPrediction& pred, TLabel& label);
	typedef void(*MultiPredictMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions);
	typedef void(*UpdateMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t i);
	typedef float(*SensitivityMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, size_t i);

protected:
	// get most derived implementation of Learn & Predict
	LearnerBase(vw& all, size_t weights = 1, size_t increment = 0)
		: TypedLearner<TPrediction, TLabel>(
			all, weights, increment,
			{
				static_cast<PredictOrLearnMethod>(&predict_or_learn_dispatch<true>),
				static_cast<PredictOrLearnMethod>(&predict_or_learn_dispatch<false>),
				static_cast<MultiPredictMethod>(&multi_predict_dispatch),
				static_cast<UpdateMethod>(&update_dispatch),
				static_cast<SensitivityMethod>(&sensitivity_dispatch)
			})
	{ }

	// iterate through one namespace (or its part), callback function T(some_data_R, feature_value_x, feature_weight)
	template <class R, void(TDerived::*T)(R&, const float, float&)>
	inline void foreach_feature(weight_parameters& weights, features& fs, R& dat, uint64_t offset = 0, float mult = 1.)
	{
		for (features::iterator& f : fs)
			T(dat, mult*f.value(), weights[(f.index() + offset)]);
	}

	// iterate through one namespace (or its part), callback function T(some_data_R, feature_value_x, feature_index)
	template <class R, void(TDerived::*T)(R&, float, uint64_t)>
	void foreach_feature(features& fs, R&dat, uint64_t offset = 0, float mult = 1.)
	{
		for (features::iterator& f : fs)
			T(dat, mult*f.value(), f.index() + offset);
	}

	// iterate through all namespaces and quadratic&cubic features, callback function T(some_data_R, feature_value_x, S)
	// where S is EITHER float& feature_weight OR uint64_t feature_index
	template <class R, class S, void(TDerived::*T)(R&, float, S)>
	inline void foreach_feature(example& ec, R& dat)
	{
		uint64_t offset = ec.ft_offset;

		for (features& f : ec)
			foreach_feature<R, T>(f, dat, offset);

		INTERACTIONS::generate_interactions<R, S, T>(ec, dat);
	}

	// iterate through all namespaces and quadratic&cubic features, callback function T(some_data_R, feature_value_x, feature_weight)
	template <class R, void(TDerived::*T)(R&, float, float&)>
	inline void foreach_feature(example& ec, R& dat)
	{
		foreach_feature<R, float&, T>(ec, dat);
	}
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
	T _value_initial;
	T& _value;

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

template<typename TDerived, typename TPrediction, typename TLabel, typename TPredictionOfBase, typename TLabelOfBase>
class Reduction : public LearnerBase<TDerived, TPrediction, TLabel>, public IReduction
{
private:
	typedef TypedLearner<TPredictionOfBase, TLabelOfBase> TBaseLearner;
	TBaseLearner* _base = nullptr;
	
	TypedLearnerVTable<TPredictionOfBase, TLabelOfBase> _base_vtable;

protected:
	Reduction(vw& all, size_t weights = 1, size_t increment = 0)
		: LearnerBase<TDerived, TPrediction, TLabel>(all, weights, increment)
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
		ec.ft_offset += _increment * i;
		(*_base_vtable.learn_method)(*_base, ec, pred, label);
	}

	inline void base_predict(example& ec, TPredictionOfBase& pred, TLabel& label, size_t i = 0)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += _increment * i;
		(*_base_vtable.predict_method)(*_base, ec, pred, label);
	}

	inline void base_multi_predict(TBaseLearner&, example& ec, size_t lo, size_t count, TPrediction* pred, TLabel& label, bool finalize_predictions)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += _increment * lo;
		(*_base_vtable.multi_predict_method)(*_base, ec, lo, count, pred, label, finalize_predictions);
	}

	inline void base_update(TBaseLearner&, example& ec, size_t i = 0)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += _increment * lo;
		(*_base_vtable.update_method)(*_base, ec, i);
	}

	inline float base_sensitivity(TBaseLearner&, example& ec, size_t i = 0)
	{
		RESTORE_VALUE_ON_RETURN(ec.ft_offset);
		ec.ft_offset += _increment * lo;
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
		_increment = _base->increment() * _weights;

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
	static T* expand(TFactory& factory, S cond)
	{
		if (cond)
			return factory.create<ArgsGrow..., true>();
		else
			return factory.create<ArgsGrow..., false>();
	}

	/*GDFactory<Learner, decltype(this)> factory(all, *this);
	return template_expansion<Learner, GDFactory<Learner, decltype(this)>>
		::expand<bool, bool, bool>(factory, sparse_l2_t, all.invariant_updates, sqrt_rate, feature_mask_off);
*/
	template<typename TB, typename ...Arguments>
	static T* expand(TFactory& factory, TB cond, Arguments... args)
	{
		if (cond) // I need to specify count(Arguments) - 1, not Arguments
			return template_expansion<T, TFactory, ArgsGrow..., true>::template expand<>(factory, std::forward<Arguments>(args)...);
			//return template_expansion<T, TFactory, ArgsGrow..., true>::template expand(factory, args...);
		else
			return template_expansion<T, TFactory, ArgsGrow..., false>::template expand<>(factory, std::forward<Arguments>(args)...);

		//return nullptr;
		//else
		//	return template_expansion<T, TFactory, ArgsGrow..., false>::template expand<Arguments...>(factory, std::forward<Arguments>(args)...);
	}
};

template<typename T, typename TFactory, int N, int ...ArgsN>
class template_expansion_int
{
public:
	template<int ...ArgsGrow>
	class inner
	{
	public:
		template<typename S>
		static T* expand(TFactory& factory, S value)
		{
			if (value == N)
				return factory.create<ArgsGrow..., N>();
			else if (value < N)
				return template_expansion_int<T, TFactory, ArgsN..., N - 1>::inner<ArgsGrow...>::template expand<S>(factory, value);
			else
				return nullptr;
		}

		template<typename TI, typename ...Arguments>
		static T* expand(TFactory& factory, TI value, Arguments... args)
		{
			if (value == N)
				return template_expansion_int<T, TFactory, ArgsN...>::inner<N, ArgsGrow...>::template expand<>(factory, std::forward<Arguments>(args)...);
			else if (value < N)
				return template_expansion_int<T, TFactory, ArgsN..., N - 1>::inner<ArgsGrow...>::template expand<>(factory, value, std::forward<Arguments>(args)...);
			else
				return nullptr;
		}
	};

	template<typename ...Arguments>
	static T* expand(TFactory& factory, Arguments... args)
	{
		return inner<>::template expand<int>(factory, std::forward<Arguments>(args)...);
	}
};

template<typename T, typename TFactory, int ...ArgsN>
class template_expansion_int<T, TFactory, -1, ArgsN...>
{
public:
	template<int ...ArgsGrow>
	class inner
	{
	public:
		template<typename S>
		static T* expand(TFactory& factory, S b)
		{
			// throw exception
			return nullptr;
		}

		template<typename ...Arguments>
		static T* expand(TFactory& factory, int a, Arguments... args)
		{
			// throw exception
			return nullptr;
		}
	};
};

template<typename TDerived, typename T, typename TFactory> // , bool ...BoolArgs>
class BoolIntFactory
{
public:
	template<int ...IntArgs>
	class Inner
	{
	public:
		template<bool ...BoolArgs>
		class Inner2
		{
			class InnerFactory
			{
			public:
				// TODO: rename ArgsFinal to IntArgs, and IntArgs to max int
				template<int ...ArgsFinal>
				T* create(TFactory& factory)
				{
					return factory.create<BoolArgs..., ArgsFinal...>();
				}
			};

		public:
			template<typename ...Arguments> 
			static T* create(TFactory& factory, Arguments... args)
			{
				InnerFactory factory;
				return template_expansion_int<T, decltype(factory), IntArgs...>::expand(factory, std::forward<Arguments>(args)...);
			}
		};
	};

	// called by template_expansion::expand()
	template<bool ...BoolArgs>
	T* create()
	{
		// need another level of indirection as we can't generically capture the runtime values
		// in variables. Thus we'll dispatch down to a class holding the runtime arguments
		// which will call up to create passing in the configuration and runtime values
		return static_cast<TDerived&>(*this).dynamic_create<BoolArgs...>();
	}
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
class Arguments :  public IArguments
{
public:
	Learner* create(vw& all)
	{
		return new TLearner(static_cast<TDerived&>(*this), all);
	}
};
