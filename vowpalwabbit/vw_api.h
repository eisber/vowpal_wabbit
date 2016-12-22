#pragma once

#include <string>
#include <stdint.h>
#include "vw.h"

#define RAPIDJSON_HAS_STDSTRING
#include <rapidjson/writer.h>
#include <rapidjson/stringbuffer.h>

namespace VW {
	namespace API {
		using namespace std;
		using namespace rapidjson;

		class example_writer;
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

			string& name()
			{
				return _name;
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

			string& name()
			{
				return _name;
			}

			uint64_t hash()
			{
				return _hash;
			}
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

		template<bool audit>
		class examples_writer_template
		{
		protected:
			examples* _examples;
			example* _example;
			// TODO: needs to be set
			hash_func_t _hasher;

			bool write_feature(uint64_t hash, float x)
			{
				if (x == 0.)
					return false;

				return true;
			}

		public:
			examples_writer_template(hash_func_t hasher) : _examples(nullptr), _example(nullptr), _hasher(hasher)
			{
				
			}

			~examples_writer_template()
			{
			}

			// need to pass from target language to enable pooling (otherwise proxies need to be re-created everytime)
			void start_examples(examples* examles) 
			{
				//if (_examples)
				//	throw exception;
				_examples = examples;
			}

			void end_examples()
			{
				// TODO, write closeing }
				_examples = nullptr;
			}

			// need to pass from target language to enable pooling
			void start_example(example* example)
			{
				//if (_examples != nullptr)
				//	throw

				if (serialize)
					_writer.StartObject();

				if (_example)
				{
					// _multi, case 
				}
				else
				{
					// shared case
				}
			}

			void end_example()
			{
				if (serialize)
					_writer.EndObject();
			}

			void start_namespace(const char* name)
			{
				if (serialize)
				{
					_writer.String(name);
					_writer.StartObject();
				}
			}

			void start_namespace(namespace_hash* ns)
			{
				if (serialize)
				{
					_writer.String(ns.name());
					_writer.StartObject();
				}
			}

			void end_namespace()
			{
				if (serialize)
				{
					_writer.EndObject();
				}
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

		template<typename OutputStream, typename SourceEncoding = UTF8<>, typename TargetEncoding = UTF8<>, typename StackAllocator = CrtAllocator, unsigned writeFlags = kWriteDefaultFlags>
		class SimpleWriter : public Writer<OutputStream, SourceEncoding, TargetEncoding, CrtAllocator, writeFlags>
		{
		public:
			SimpleWriter(Writer(OutputStream& os, StackAllocator* stackAllocator = 0, size_t levelDepth = kDefaultLevelDepth) 
				: _ Writer<OutputStream, SourceEncoding, TargetEncoding, CrtAllocator, writeFlags>(os, stackAllocator, levelDepth)
			{}

			bool RawValue(const Ch* json, size_t length) { return WriteRawValue(json, length); }
		};

		template<bool audit>
		class examples_writer_serialize_template : examples_writer_template<audit>
		{
		private:
			StringBuffer _buffer;
			SimpleWriter<StringBuffer> _writer;
			bool _multi_detected;

			void write_feature(const char* name, size_t length, uint64_t hash, float x)
			{
				if (!examples_writer_template<audit>::write_feature(hash, x))
					return;

				_writer.String(name, length);
				_writer.Double(x);
			}

		public:
			examples_writer_serialize_template(hash_func_t hasher) : examples_writer_template<audit>(hasher), _writer(_buffer)
			{
			}

			// need to pass from target language to enable pooling (otherwise proxies need to be re-created everytime)
			void start_examples(examples* examles)
			{
				examples_writer_template<audit>::start_examples(examples);
				_multi_detected = false;
			}

			void end_examples()
			{
				examples_writer_template<audit>::end_examples(examples);
				
				if (_multi_detected)
					_writer.EndObject();
			}

			// need to pass from target language to enable pooling
			void start_example(example* ex)
			{
				examples_writer_template<audit>::start_example(ex);


				if (_example)
				{
					_multi_detected = true;

				}

				_writer.StartObject();
			}

			void end_example()
			{
				examples_writer_template<audit>::end_example();

				_writer.EndObject();
			}

			void start_namespace(const char* name)
			{
				examples_writer_template<audit>::start_namespace(name);

				_writer.String(name);
				_writer.StartObject();
			}

			void start_namespace(namespace_hash* ns)
			{
				examples_writer_template<audit>::start_namespace(ns);

				_writer.String(ns.name());
				_writer.StartObject();
			}

			void end_namespace()
			{
				examples_writer_template<audit>::end_namespace();

				_writer.EndObject();
			}

			void write_feature(const char* name, float x)
			{
				// TODO: improve hasher by removing '\0' check if len is supplied
				write_feature(name, strlen(name), _hasher(name), x);
			}

			void write_feature(feature_hash* f, float x)
			{
				write_feature(x.name().c_str(), x.name().length(), f.hash(), x);
			}


			void pre_allocate_features(size_t size)
			{
				examples_writer_template<audit>::pre_allocate_features(size);

				// TODO: pre-alloc stringbuffer string?
			}

#ifdef SWIGCSHARP
			void write_feature_unchecked(float* x_array, size_t n)
			{
				examples_writer_template<audit>::write_feature_unchecked(x_array, n);
				float end = x_array + n;
				for (float* p = x_array; p < end;++p)
					_writer.Double(*p);
			}
#endif

			void write_feature(float x)
			{
				examples_writer_template<audit>::write_feature(x);
				_writer.Double(x);
			}

			// TODO: std::string?
			void start_dense_features(const char* name)
			{
				examples_writer_template<audit>::start_dense_features(name);
				_writer.String(name);
				_writer.StartArray();
			}

			void end_dense_features()
			{
				examples_writer_template<audit>::end_dense_features();
				_writer.EndArray();
			}

			// TODO: use std. string
			void write_aux(const char* name, const char* json, size_t length)
			{
				_writer.String(name);
				_writer.RawValue(json, length);
			}
		};

		class example_allocator
		{
		private:
			typedef void(*default_label_func_t)(void*);
			typedef void(*delete_label_func_t)(void*);

			hash_func_t _hasher;
			void(*_default_label)(void*);
			void(*_delete_label)(void*);

		public:
			example_allocator(hash_func_t hasher, default_label_func_t default_label, delete_label_func_t delete_label);

			example* new_example();
			void delete_example(example* ex);

			hash_func_t hasher hasher()
			{
				return _hasher;
			}
		};

		class vowpal_wabbit
		{
		private:
			vw* _all;

		public:
			vowpal_wabbit();
			~vowpal_wabbit();

			example_allocator get_example_allocator();
		};

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

			void predict(examples& examples, prediction& prediction)
			{

			}
		};
	}
}