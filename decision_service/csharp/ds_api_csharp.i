%module(directors="1") decision_service

#define ARRAYS_OPTIMIZED
#define SWIG_CSHARP


%include "../ds_api.i"
%include <arrays_csharp.i>

%template(ListInt) std::vector<int>;
%template(ListFloat) std::vector<float>;

%rename("%(camelcase)s", notregexmatch$name=".*_template") "";

// %apply unsigned char FIXED[] {unsigned char* model}
// %csmethodmodifiers Microsoft::DecisionService::DecisionServiceClient::update_model "public unsafe"; 

%define CSHARP_IN_ARRAYS( TYPE )
	%typemap(cstype) const Microsoft::DecisionService::Array<TYPE>& "TYPE[]"
	%typemap(csin,
			 pre=       "    fixed ( int* swig_ptrTo_$csinput = $csinput ) {\n" 
						"        Array<TYPE> swig_arrayTo_$csinput = { swig_ptrTo_$csinput, $csinput.Length }; ",
			 terminator="    }") 
			const Microsoft::DecisionService::Array<TYPE>& "swig_arrayTo_$csinput" 
%enddef // CSHARP_IN_ARRAYS

CSHARP_IN_ARRAYS(int)
CSHARP_IN_ARRAYS(float)

// required for fast vector copying
// TODO: conditional include for full framework
%ignore Microsoft::DecisionService::DecisionServiceClient::explore_and_log(const char* features, const char* event_id, const Array<float>& scores);
// %csmethodmodifiers Microsoft::DecisionService::DecisionServiceClient::explore_and_log(const char* features, const char* event_id, const Array<float>& scores) "public unsafe";


/*
%ignore std::vector<int>;
%ignore std::vector<float>;
%ignore Microsoft::DecisionService::DecisionServiceClient::rank_cstyle;
%ignore Microsoft::DecisionService::DecisionServiceClient::rank_vector;
%rename(rank) Microsoft::DecisionService::DecisionServiceClient::rank_struct;
*/

// must be at the end
%include "ds_predictors.h"
%include "ds_explore.h"
%include "ds_api.h"
