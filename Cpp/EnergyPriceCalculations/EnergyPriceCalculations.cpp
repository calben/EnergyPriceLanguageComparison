#include "EnergyPriceCalculations.h"

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
main()
{
    std::filesystem::path root{"C:/Projects/Code/EnergyPriceCalculations"};
    nlohmann::json summary;
    for (const auto &path : std::filesystem::directory_iterator(root / "Dataset")) {
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
    std::ofstream ofs{root / "Results/cpp.json"};
    ofs << summary;
    return 0;
}
