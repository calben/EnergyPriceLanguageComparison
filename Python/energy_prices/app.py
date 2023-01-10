from enum import Enum

import numpy
import typer
import time
import pandas
import pathlib
import json


class EnergyProductionMethod(Enum):
    Oil = 1
    Nuclear = 2
    Wind = 3
    Biomass = 4
    Solar = 5
    Hydro = 6
    Geothermal = 7


class Province(Enum):
    Ontario = 1
    Quebec = 2
    NovaScotia = 3
    NewBrunswick = 4
    BritishColumbia = 5


app = typer.Typer()


@app.command()
def generate_prices_data(path: pathlib.Path, number_of_rows: int = 2**14):
    path.parent.mkdir(exist_ok=True, parents=True)
    with open(path.absolute(), "w") as f:
        f.write(",".join([x.name for x in EnergyProductionMethod]) + ",Province" "\n")
        for i in range(number_of_rows):
            f.write(
                ",".join(
                    [
                        str(10 ** (i + 1) * x)
                        for i, x in enumerate(
                            numpy.random.rand(len(EnergyProductionMethod))
                        )
                    ]
                )
                + ","
                + numpy.random.choice(list(Province)).name
                + "\n"
            )


@app.command()
def generate_production_data(path: pathlib.Path, number_of_rows: int = 2**14):
    path.parent.mkdir(exist_ok=True, parents=True)
    num_production_methods = len(EnergyProductionMethod)
    with open(path.absolute(), "w") as f:
        f.write(",".join([x.name for x in EnergyProductionMethod]) + ",Province" "\n")
        for i in range(number_of_rows):
            f.write(
                ",".join(
                    [
                        str(int(100000 * x * (i + 1)))
                        for i, x in enumerate(
                            numpy.random.rand(len(EnergyProductionMethod))
                        )
                    ]
                )
                + ","
                + numpy.random.choice(list(Province)).name
                + "\n"
            )


@app.command()
def mean_production_per_province(input: pathlib.Path, output: pathlib.Path):
    summary = {}
    for path in input.glob("production*.csv"):
        start_io = time.time()
        df = pandas.read_csv(open(path.absolute(), "r", encoding="utf-8"))
        end_io = time.time()
        start_calculation = time.time()
        result = df.groupby("Province").mean().transpose().to_dict()
        end_calculation = time.time()
        summary[path.stem] = {
            "result": result,
            "ioElapsedMicroseconds": (end_io - start_io) * 1000000,
            "calculationElapsedMicroseconds": (end_calculation - start_calculation)
            * 1000000,
        }
    output.parent.mkdir(exist_ok=True, parents=True)
    output.write_text(json.dumps(summary, sort_keys=True, indent=4))


if __name__ == "__main__":
    app()
