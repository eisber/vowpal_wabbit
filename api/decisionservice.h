#pragma once

#include "vw_api.h"
#include <vector>

namespace DecisionService {
	namespace API {
		using namespace VW::API;

		class ranking
		{
		public:

		};

		class decision_service
		{
		private:
			prediction_pool _pred_pool;

		public:
			decision_service()
			{
				// TODO: setup background upload thread

				// TODO: setup download thread for models
			}

			// key encoded into per example seed
			int choose_action(examples* context)
			{
				return 0;
			}

			// TODO: default ranking
			void choose_ranking(examples* context, std::vector<int>* result) 
			{
				prediction pred;
				_pred_pool.predict(context, &pred);

				// TODO: where to do the sampling?

				// TODO: logging
				context->write_json();

				// TODO: convert pred into result
			}

			void update_model()
			{

			}
		};

} }