#include "example.h"

namespace vwp {


// layers of library interface

// 1 lowest level (<500 lines)
// -- weight_index level (no hashing)
// -- model parser

// level 2 | with string hashing -> python dictionaries, or C# types

// python { 'xyz': { 'f1': 7 } } 

// ex = vw_example_python_builder.build(python dict)
// model.predict(ex)

// level 3 | JSON
// JSON string 
// ex =vw_example_json_builder.build("{ 'xyz': { 'f1': 7 } }")

// model.predict(ex)


    class hash {
        public:
        hash(const char* name);
         // from string
         // from string + hash()
         // produce hash
    };

    class feature_builder
    {
    public:
        void x(float x);
    };

    class namespace_builder
    {
    // feature_group;
    public:
        feature_builder* add_feature(const char* name);

        feature_builder* add_feature(uint64_t weight_index);
    };

    class example_builder {
        example* _example;
    public:
        example_builder();

        void bind(example* example);

        namespace_builder* add_namespace(char feature_group, const char* name = nullptr);

        namespace_builder&*
    };

    void foo()
    {
        // schema-based construction for C#?

        // construct pre-hashed graph based on schema
        example_builder eb;
        namespace_builder* ns = eb.add_namespace('x', "yz");
        feature_builder* f1 = ns->add_feature("f1");

        // bind target example
        example ex;
        eb.bind(&ex);

        // set values
        f1->x(1);


        // on the fly construction
        // |xyz f1:5

        // add x
        // hash(f1, hash(xyz)) : 5

        // {'xyz':{'f1':5}}

    }
}