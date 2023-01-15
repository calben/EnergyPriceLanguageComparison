#include "EnergyPriceCalculations.h"

#include "../include/p-ranav/argparse/include/argparse/argparse.hpp"
#include "../include/nlohmann/json/single_include/nlohmann/json.hpp"

#include <chrono>
#include <filesystem>
#include <fstream>
#include <iostream>
#include <sstream>
#include <string>

auto
get_energy_generation_from_csv(std::filesystem::path path)
{
    std::vector<energy_generation> result;
    std::ifstream ifs{path};
    if (ifs.is_open()) {
        std::string row_string;
        std::getline(ifs, row_string);
        while (std::getline(ifs, row_string)) {
            uint8_t i = 0;
            energy_generation row;
            std::stringstream row_string_stream{row_string};
            std::string column;
            while (std::getline(row_string_stream, column, ',')) {
                if (i < EnergyProductionMethod::_size_constant) {
                    row.generation[i] = SI::joule_t<uint64_t>(std::stoull(column));
                } else if (i == EnergyProductionMethod::_size_constant) {
                    row.province = Province::_from_string_nocase(column.c_str());
                }
                ++i;
            }
            result.emplace_back(row);
        }
    }
    return result;
}

auto
get_mean_generation_per_province(const std::vector<energy_generation> &data)
{
    SI::joule_t<uint64_t> intermediate[Province::_size_constant][EnergyProductionMethod::_size_constant] = {
        SI::joule_t<uint64_t>(0)};
    size_t count_per_province[Province::_size_constant] = {0};
    for (const auto &row : data) {
        ++count_per_province[row.province];
        for (uint8_t j = 0; j < EnergyProductionMethod::_size_constant; ++j) {
            intermediate[row.province][j] += row.generation[j];
        }
    }
    nlohmann::json j;
    for (Province p : Province::_values()) {
        for (EnergyProductionMethod e : EnergyProductionMethod::_values()) {
            j[p._to_string()][e._to_string()] = intermediate[p][e].value() / count_per_province[p];
        }
    }
    return j;
}

int
main(int argc, char *argv[])
{
    argparse::ArgumentParser program("EnergyPriceCalculations");
    program.add_argument("input_directory")
        .help("Directory in which to search for energy production CSV files.")
        .required();
    program.add_argument("output_json").help("Path to which to output the json with results and timings.").required();
    try {
        program.parse_args(argc, argv);
    } catch (const std::runtime_error &err) {
        std::cerr << err.what() << std::endl;
        std::cerr << program;
        std::exit(1);
    }
    nlohmann::json summary;
    for (const auto &path :
         std::filesystem::directory_iterator(std::filesystem::path(program.get<std::string>("input_directory")))) {
        if (path.is_regular_file() && path.path().filename().string().starts_with("production")) {
            const auto io_start = std::chrono::steady_clock::now();
            const auto data = get_energy_generation_from_csv(path);
            const auto io_end = std::chrono::steady_clock::now();

            const auto calculation_start = std::chrono::steady_clock::now();
            const auto result = get_mean_generation_per_province(data);
            const auto calculation_end = std::chrono::steady_clock::now();
            nlohmann::json j;
            j["ioElapsedMicrosecnods"] = std::chrono::duration_cast<std::chrono::microseconds>(io_end - io_start)
                                             .count();
            j["calculationElapsedMicroseconds"] =
                std::chrono::duration_cast<std::chrono::microseconds>(calculation_end - calculation_start).count();
            j["result"] = result;
            summary[path.path().filename().string()] = j;
        }
    }
    std::ofstream ofs{program.get<std::string>("output_json")};
    ofs << summary;
    return 0;
}
