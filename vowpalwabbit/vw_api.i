
%module VW
%{

 /* Includes the header in the wrapper code */
 #include "vw_api.h"
 #include "decisionservice.h"
 %}

 %include "std_vector.i"
namespace std {
  #ifdef SWIGCSHARP
   %template(ListInt) vector<int>;
   #else
   %template(vector_int) vector<int>;
   #endif
}

#ifdef SWIGCSHARP
	%rename("%(camelcase)s", notregexmatch$name="examples_writer_template") "";


	%include "arrays_csharp.i"
	%apply float FIXED[] {float *x_array}
#endif

#ifdef SWIGJAVA
	%rename("%(camelcase)s", notregexmatch$name="examples_writer_template|vector") "";

	%rename("%(lowercamelcase)s", %$isfunction, %$ismember) "";
	// TODO: rename methods seperately
#endif


 %ignore VW::API::example; 
 
 %include "vw_api.h"
 %include "decisionservice.h"
 

 // TODO: also for Java, move to include
  #ifdef SWIGCSHARP
	
	%csmethodmodifiers write_feature_unchecked "public unsafe";

	%template(ExamplesWriter)                 VW::API::examples_writer_template<false,false>;
	%template(ExamplesWriterAudit)            VW::API::examples_writer_template<true, false>;
	%template(ExamplesWriterSerialize)        VW::API::examples_writer_template<false, true>;
	%template(ExamplesWriterAuditSerialize)   VW::API::examples_writer_template<true, true>;

  #else

	%template(examples_writer)                  VW::API::examples_writer_template<false,false>;
	%template(examples_writer_audit)            VW::API::examples_writer_template<true, false>;
	%template(examples_writer_serialize)        VW::API::examples_writer_template<false, true>;
	%template(examples_writer_audit_serialize)  VW::API::examples_writer_template<true, true>;

  #endif 
  

 // TODO: memory pooling has to be done in target language, otherwise we can't cache proxies...
