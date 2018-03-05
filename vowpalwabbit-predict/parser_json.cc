#include "parser_json.h"
#include <rapidjson/document.h>     // rapidjson's DOM-style API

using namespace rapidjson;

namespace vwp {

    model model_parser_json::parse(const char* json)
    {
        Document document;

        if (document.Parse(json).HasParseError())
        {
            // http://rapidjson.org/md_doc_dom.html#ParseError
            throw std::invalid_argument("JSON is invalid");
        }

        if (!document["bits"].IsNumber())
            throw std::invalid_argument("bits missing");
        uint64_t n = 1i64 << document["bits"].GetInt();

        auto& sparse = document["sparse"];         
        if (!sparse.IsObject())
            throw std::invalid_argument("sparse missing");

        // parses sparse weight representation
        auto& weights_json = sparse["weights"];
        auto& indices_json = sparse["indices"];

        if (!weights_json.IsArray())
            throw std::invalid_argument("sparse.weights missing");
        if (!indices_json.IsArray())
            throw std::invalid_argument("sparse.indices missing");

        if (weights_json.Size() != indices_json.Size())
            throw std::invalid_argument("sparse.indices and sparse.weights must be of equal size");

        std::vector<float> weights(n);
        for (SizeType i = 0; i < weights_json.Size(); i++) 
        {
            auto& index = indices_json[i];
            auto& value = weights_json[i];
            if (!index.IsUint())
                throw std::invalid_argument("sparse.indices[] must be uint");
            if (!value.IsNumber())
                throw std::invalid_argument("sparse.weights[] must be double");

            weights[index.GetUint()] = (float)value.GetDouble();
        }

        model model(&weights[0], weights.size());

        if (!document["contraction"].IsNumber())
            throw std::invalid_argument("contraction of type double missing");
        model.set_contraction((float)document["contraction"].GetDouble());

        auto& interactions_json = document["interactions"];
        if (interactions_json.IsArray())
        {
            std::vector<ustring> interactions(interactions_json.Size());
            for (SizeType i = 0; i < interactions_json.Size(); i++) 
            {
                auto& interaction = interactions_json[i];
                if (!interaction.IsArray())
                    throw std::invalid_argument("interactions[] elements must be arrays");
                
                ustring s;
                for (SizeType j = 0; j < interaction.Size(); j++) 
                {
                    auto& ns = interaction[j];
                    if (!ns.IsUint())
                        throw std::invalid_argument("namespace in interaction element must uint");
                        
                    s.push_back((unsigned char)ns.GetUint());
                }    

                interactions[i] = s;
            }

            model.set_interactions(interactions);
        }

        return model;
    }
}
