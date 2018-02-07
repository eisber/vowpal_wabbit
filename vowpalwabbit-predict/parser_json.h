#pragma once

#include "model.h"

namespace vwp {
    class model_parser_json {
    public:
        static model parse(const char* json);
    };
}