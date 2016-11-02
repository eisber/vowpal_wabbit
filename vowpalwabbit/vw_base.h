#pragma once

#include "reductions.h"
#include <string>
#include <vector>
#include <functional>

class Learner
{
protected:
	size_t weights; //this stores the number of "weight vectors" required by the learner.
	size_t increment;

public:
	// optional dependencies
	virtual const char** dependencies()
	{ 
		return nullptr;
	}

	// options object
	virtual po::options_description* options() = 0;

	virtual void init(vw& all)
	{ }

	virtual void save_load(io_buf& buf, bool read, bool text)
	{ }

	virtual void finish_example(example& ec)
	{ }

	virtual void end_pass()
	{ }

	virtual void end_examples()
	{ }
};

template<typename TPrediction, typename TLabel>
class TypedLearner: public Learner
{
public:
	typedef void (*PredictOrLearnMethod)(TypedLearner<TPrediction, TLabel>&, example& ec, TPrediction& pred, TLabel& label);

private:
	// function pointer to avoid double vtable dispatch
	PredictOrLearnMethod _learn_method;
	PredictOrLearnMethod _predict_method;

protected:
	TypedLearner(PredictOrLearnMethod learn_method, PredictOrLearnMethod predict_method)
		: _learn_method(learn_method), _predict_method(predict_method)
	{ }

public:
	PredictOrLearnMethod learn_method() { return _learn_method; }

	PredictOrLearnMethod predict_method() { return _predict_method; }

	//// optimized dispatch
	//inline void learn(example &ec, TPrediction& pred, TLabel& label)
	//{
	//	// needs to be function pointer as the TypedLearner cannot declare member method pointers to derived classes
	//	// TypedLearner::* are only methods on TypedLearner and above, not on subclasses
	//	(*_learn_method)(*this, ec, pred, label);
	//} 

	//inline void predict(example &ec, TPrediction& pred, TLabel& label)
	//{
	//	(*_predict_method)(*this, ec, pred, label);
	//}
};

class IReduction
{
public:
	virtual bool try_link(Learner& learner) = 0;
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

	template<typename S>
	T operator+=(S val)
	{
		return _value += val;
	}

	~RestoreValueOnReturn()
	{
		_value = _value_initial;
	}
};

template<typename TPrediction, typename TLabel, typename TPredictionOfBase, typename TLabelOfBase>
class TypedReduction : public TypedLearner<TPrediction, TLabel>, public IReduction
{
private:
	typedef TypedLearner<TPredictionOfBase, TLabelOfBase> TBaseLearner;

	// need to re-declare as gcc can't find the base class typedef
	typedef void(*PredictOrLearnMethod)(TBaseLearner&, example& ec, TPrediction& pred, TLabel& label);
	typedef void(*BasePredictOrLearnMethod)(TBaseLearner&, example& ec, TPrediction& pred, TLabel& label);

	TBaseLearner* _base = nullptr;
	BasePredictOrLearnMethod _base_learn_method = nullptr;
	BasePredictOrLearnMethod _base_predict_method = nullptr;

protected:
	TypedReduction(PredictOrLearnMethod learn_method, PredictOrLearnMethod predict_method)
		: TypedLearner<TPrediction, TLabel>(learn_method, predict_method)
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
		// increment gets reversed on return even in case of exception
		// EXPLAIN: ec.ft_offset += (uint32_t)(increment*i);
		ValueGuard<decltype(ec.ft_offset)> ft_offset(ec.ft_offset);
		ft_offset += increment * i;
		
		(*_base_learn_method)(*_base, ec, pred, label);
	}

	inline void base_predict(example& ec, TPredictionOfBase& pred, TLabel& label, size_t i = 0)
	{
		// increment gets reversed on return even in case of exception
		// EXPLAIN: ec.ft_offset += (uint32_t)(increment*i);
		RestoreValueOnReturn<decltype(ec.ft_offset)> ft_offset(ec.ft_offset);
		ft_offset += increment * i;

		(*_base_predict_method)(*_base, ec, pred, label);
	}

public:
	bool try_link(Learner& learner)
	{
		// verify type match
		_base = dynamic_cast<TBaseLearner*>(&learner);

		// TODO: exception?
		if (_base == nullptr)
			return false;

		_base_learn_method = _base->learn_method();
		_base_predict_method = _base->predict_method();

		return true;
	}
};

template<typename TDerived, typename TPrediction, typename TLabel, typename TPredictionOfBase, typename TLabelOfBase>
class Reduction : public TypedReduction<TPrediction, TLabel, TPredictionOfBase, TLabelOfBase>
{
private:
	template<bool is_learn>
	static inline void predict_or_learn_dispatch(TypedLearner<TPrediction, TLabel>& that, example& ec, TPrediction& pred, TLabel& label)
	{
		// invoke the most derived implementation of predict_or_learn_impl
		// this method is only called from the base class and "that" == "this"
		static_cast<TDerived&>(that).template predict_or_learn<is_learn>(ec, pred, label);
	}

protected:
	// get most derived implementation of Learn & Predict
	Reduction()
		: TypedReduction<TPrediction, TLabel, TPredictionOfBase, TLabelOfBase>(&predict_or_learn_dispatch<true>, &predict_or_learn_dispatch<false>)
	{ }
};