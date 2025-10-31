import os
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt

BASE_DIR = "../Results"
PLOTS_DIR = os.path.join(BASE_DIR, "Plots")
METHODS = ["COCG", "ComplexLocalOptimalScheme"]
SMOOTHING_TYPES = ["NoSmoothing", "ResidualSmoothing"]
THREADS = [1, 2, 4, 12]

os.makedirs(PLOTS_DIR, exist_ok=True)

def read_residual_data(file_path: str) -> pd.DataFrame | None:
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            lines = f.readlines()
    except FileNotFoundError:
        return None

    start_idx = None
    for i, line in enumerate(lines):
        if line.strip().startswith("Iteration"):
            start_idx = i + 2
            break
    if start_idx is None:
        return None

    iterations, residuals, smoothed = [], [], []
    for line in lines[start_idx:]:
        parts = line.strip().split()
        if len(parts) < 3:
            break
        try:
            iterations.append(int(parts[0]))
            residuals.append(float(parts[1].replace(",", ".")))
            smoothed.append(float(parts[2].replace(",", ".")))
        except ValueError:
            break

    return pd.DataFrame({
        "Iteration": iterations,
        "Residual": residuals,
        "SmoothedResidual": smoothed
    })

def read_total_time(file_path: str) -> float | None:
    try:
        with open(file_path, "r", encoding="utf-8") as f:
            for line in f:
                if line.startswith("Total time:"):
                    return float(line.split(":")[1].strip().split()[0])
    except FileNotFoundError:
        return None
    return None

def add_value_labels(ax):
    for p in ax.patches:
        height = p.get_height()
        if not np.isnan(height):
            ax.text(
                p.get_x() + p.get_width() / 2,
                height,
                f"{height:.1f}",
                ha="center", va="bottom", fontsize=8
            )

def plot_convergence():
    print("\nConvergence analysis...\n")
    for slae_dir in sorted(os.listdir(BASE_DIR)):
        slae_path = os.path.join(BASE_DIR, slae_dir)
        if not os.path.isdir(slae_path):
            continue

        slae_number = slae_dir
        for method in METHODS:
            method_path = os.path.join(slae_path, method)
            if not os.path.isdir(method_path):
                continue

            plt.figure(figsize=(8, 6))
            num_curves = 0

            for smoothing_type in SMOOTHING_TYPES:
                smoothing_path = os.path.join(method_path, smoothing_type)
                file_path = os.path.join(smoothing_path, "1 threads.txt")
                if not os.path.isfile(file_path):
                    continue

                df = read_residual_data(file_path)
                if df is None or df.empty:
                    continue

                y = (
                    df["SmoothedResidual"]
                    if smoothing_type == "ResidualSmoothing"
                    else df["Residual"]
                )
                plt.semilogy(df["Iteration"], y, label=smoothing_type)
                num_curves += 1

            if num_curves == 0:
                continue

            plt.title(f"{method} — SLAE {slae_number}")
            plt.xlabel("Iteration")
            plt.ylabel("Relative Residual")
            plt.grid(True, which="both", ls="--", lw=0.5)
            plt.legend(title="Type")
            plt.tight_layout()

            save_path = os.path.join(PLOTS_DIR, f"{method}_SLAE{slae_number}_residual.png")
            plt.savefig(save_path, dpi=300)
            print(f"Saved convergence plot: {save_path} ({num_curves} curves)")
            # plt.show()
            # plt.close()

def plot_performance():
    print("\nPerformance analysis...\n")
    for slae_dir in sorted(os.listdir(BASE_DIR)):
        slae_path = os.path.join(BASE_DIR, slae_dir)
        if not os.path.isdir(slae_path):
            continue

        slae_number = slae_dir
        for method in METHODS:
            method_path = os.path.join(slae_path, method)
            if not os.path.isdir(method_path):
                continue

            times = {st: {} for st in SMOOTHING_TYPES}

            for smoothing_type in SMOOTHING_TYPES:
                if slae_number == "4" and smoothing_type == "NoSmoothing":
                    continue

                smoothing_path = os.path.join(method_path, smoothing_type)
                for t in THREADS:
                    file_path = os.path.join(smoothing_path, f"{t} threads.txt")
                    time_val = read_total_time(file_path)
                    if time_val is not None:
                        times[smoothing_type][t] = time_val

            if not any(times[st] for st in times):
                continue

            fig, ax = plt.subplots(figsize=(8, 6))
            x = np.arange(len(THREADS))
            bar_width = 0.35
            offset = -bar_width / 2
            legend_entries = 0

            for smoothing_type in times:
                y = [times[smoothing_type].get(t, np.nan) for t in THREADS]
                if all(np.isnan(y)):
                    offset += bar_width
                    continue
                ax.bar(x + offset, y, width=bar_width, label=smoothing_type)
                legend_entries += 1
                offset += bar_width

            ax.set_xticks(x)
            ax.set_xticklabels([str(t) for t in THREADS])
            ax.set_xlabel("Number of Threads")
            ax.set_ylabel("Execution Time (ms)")
            ax.set_title(f"{method} — SLAE {slae_number}: Execution Time")
            ax.grid(True, axis="y", linestyle="--", lw=0.5)
            add_value_labels(ax)
            if legend_entries > 0:
                ax.legend(title="Type")
            plt.tight_layout()

            save_path = os.path.join(PLOTS_DIR, f"{method}_SLAE{slae_number}_time.png")
            plt.savefig(save_path, dpi=300)
            print(f"Saved performance plot: {save_path}")
            # plt.show()
            # plt.close()

if __name__ == "__main__":
    plot_convergence()
    plot_performance()
    print("\nFinished!\n")
