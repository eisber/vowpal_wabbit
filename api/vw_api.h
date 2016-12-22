#pragma once

#include <string>
#include <stdint.h>

namespace VW {
	namespace API {
		using namespace std;

		class example_writer;
		class example;
		class examples;

		class namespace_hash
		{
		private:
			uint64_t _hash;
			string _name;

		public:
			namespace_hash(const char* name) :  _name(name)
			{
				// _weight_index(weight_index), = hash()
			}
		};

		class feature_hash
		{
		private:
			uint64_t _hash;
			string _name;

		public:
			feature_hash(const char* name) : _name(name)
			{
				// _weight_index(weight_index), = hash()
			}
		};
		
		class example
		{
		public:
			example() {}
			~example() {}
		};

		class examples
		{
			// v_array of examples
			example* _array;

			// TODO: JSON output buffer
		public:
			examples() {}
			~examples() {}

			void write_json(/* stream? */)
			{}
		};

		template<bool audit, bool serialize>
		class examples_writer_template
		{
		private:
			examples* _examples;
			example* _example;

			void write_feature(const char* name, uint64_t hash, float x)
			{
			}

		public:
			examples_writer_template() {
			}

			~examples_writer_template()
			{
			}

			// need to pass from target language to enable pooling
			void start_examples(examples* examles) 
			{
			}

			void end_examples()
			{
			}

			void start_example()
			{
			}

			void end_example()
			{
			}

			void start_namespace(const char* name)
			{
			}

			void start_namespace(namespace_hash* ns)
			{
			}

			void end_namespace()
			{
			}

			void write_feature(const char* name, float x)
			{
			}

			void write_feature(feature_hash* f, float x)
			{
			}

			void pre_allocate_features(size_t size)
			{
			}

#ifdef SWIGCSHARP
			void write_feature_unchecked(float* x_array, size_t n)
			{
			}
#endif

			void write_feature(float x)
			{
			}

			void start_dense_features(const char* name)
			{
			}

			void end_dense_features()
			{
			}

			void write_aux(const char* name, const char* json)
			{
			}

			// multiple overloads for different label types
			//void write_label();
			//void write_tag();
		};

		//class examples_writer : public examples_writer_base<false, false>
		//{

		//};

		class prediction
		{
		public:
			// type()
			// union of all
		};

		class learner
		{
		public:
			//void learn(examples* examples);
			//// avoid re-allocation
			//void learn(examples* examples, prediction* prediction);
			//void predict(examples* examples, prediction* prediction);
		};

		class prediction_pool
		{
		public:
			void update_model(/* byte* or so */)
			{
				// thread-save update
			}

			void predict(examples* examples, prediction* prediction)
			{

			}
		};
	}
}