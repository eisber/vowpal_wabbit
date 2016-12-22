#include "vw_api.h"

namespace VW {
	namespace API {

		vowpal_wabbit::vowpal_wabbit()
		{

		}


		vowpal_wabbit::~vowpal_wabbit()
		{

		}

		example_allocator vowpal_wabbit::get_example_allocator()
		{
			return example_allocator(_all->p->hasher, _all->p->lp.default_label, _all->p->lp.delete_label);
		}

		example_allocator::example_allocator(hash_func_t hasher, default_label_func_t default_label, delete_label_func_t delete_label)
			: _hasher(hasher), _default_label(default_label), _delete_label(_delete_label)
		{
		}

		// TODO: %newobject? 
		//anyway target language needs to build memory pool
		example* example_allocator::new_example()
		{
			auto ex = VW::alloc_examples(0, 1);
			_default_label(&ex->l);

			return ex;
		}

		void example_allocator::delete_example(example* ex)
		{
			VW::dealloc_example(_delete_label, *ex);
			::free_it(ex);
		}
	}
}
