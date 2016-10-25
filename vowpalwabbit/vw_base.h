#pragma once

#include "reductions.h"
#include <string>
#include <vector>
#include <functional>

class Learner
{
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
};

template<typename TPrediction, typename TLabel>
class TypedLearner: public Learner
{
protected:
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
	// optimized dispatch
	inline void learn(example &ec, TPrediction& pred, TLabel& label)
	{
		// needs to be function pointer as the TypedLearner cannot declare member method pointers to derived classes
		// TypedLearner::* are only methods on TypedLearner and above, not on subclasses
		(*_learn_method)(*this, ec, pred, label);
	} 

	inline void predict(example &ec, TPrediction& pred, TLabel& label)
	{
		(*_predict_method)(*this, ec, pred, label);
	}
};

class IReduction
{
public:
	virtual bool try_link(Learner& learner) = 0;
};

template<typename TPrediction, typename TLabel, typename TPredictionOfBase, typename TLabelOfBase>
class TypedReduction : public TypedLearner<TPrediction, TLabel>, public IReduction
{
public:
	typedef TypedLearner<TPredictionOfBase, TLabelOfBase> TBaseLearner;

private:
	TBaseLearner* _base;

protected:
	TypedReduction(PredictOrLearnMethod learn_method, PredictOrLearnMethod predict_method)
		: TypedLearner(learn_method, predict_method)
	{ }

	template<bool is_learn>
	void base_predict_or_learn(example& ec, TPredictionOfBase& pred, TLabelOfBase& label)
	{
		if (is_learn)
			base_learn(ec, pred, label);
		else
			base_predict(ec, pred, label);
	}

	void base_learn(example& ec, TPredictionOfBase& pred, TLabel& label)
	{
		_base->learn(ec, pred, label);
	}

	void base_predict(example& ec, TPredictionOfBase& pred, TLabel& label)
	{
		_base->predict(ec, pred, label);
	}

public:
	bool try_link(Learner& learner)
	{
		// verify type match
		_base = dynamic_cast<TBaseLearner*>(&learner);

		return _base != nullptr;
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
		static_cast<TDerived&>(that).predict_or_learn<is_learn>(ec, pred, label);
	}

protected:
	// get most derived implementation of Learn & Predict
	Reduction()
		: TypedReduction(&predict_or_learn_dispatch<true>, &predict_or_learn_dispatch<false>)
	{ }
};