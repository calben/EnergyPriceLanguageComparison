#pragma once

#include "C:\Projects\Code\nlohmann\json\single_include\nlohmann\json.hpp"
#include "C:\Projects\Code\bernedom\SI\include\SI\energy.h"

#include "enum.hpp"

#include <iostream>
#include <chrono>
#include <filesystem>
#include <fstream>
#include <sstream>
#include <string>
#include <unordered_map>

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
