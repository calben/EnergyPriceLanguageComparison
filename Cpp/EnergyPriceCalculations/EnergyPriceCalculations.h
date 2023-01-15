#pragma once

#include "../include/nlohmann/json/single_include/nlohmann/json_fwd.hpp"
#include "../include/bernedom/SI/include/SI/energy.h"
#include "../include/aantron/better-enums/enum.h"
#define __STDC_FORMAT_MACROS
#include <inttypes.h> 

BETTER_ENUM(Province, uint8_t, Ontario, Quebec, NovaScotia, NewBrunswick, BritishColumbia);

BETTER_ENUM(EnergyProductionMethod, uint8_t, Oil, Nuclear, Wind, Biomass, Solar, Hydro, Geothermal);

struct energy_data_base {
    std::chrono::time_point<std::chrono::system_clock> time;
    Province province = Province::Ontario;
};

struct energy_price : public energy_data_base {
    uint64_t price[EnergyProductionMethod::_size_constant] = {0};
};

struct energy_generation : public energy_data_base {
    SI::joule_t<uint64_t> generation[EnergyProductionMethod::_size_constant] = {SI::joule_t<uint64_t>(0)};
};
