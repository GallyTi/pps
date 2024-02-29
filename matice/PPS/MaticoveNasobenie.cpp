#include <iostream>
#include <vector>
#include <chrono>
#include <fstream>
#include <omp.h>

void fillMatrixWithRandomNumbers(std::vector<std::vector<int>>& matrix, int max_value) {
    for (auto& row : matrix) {
        for (auto& elem : row) {
            elem = std::rand() % max_value;  // Náhodné číslo do max_value
        }
    }
}

void multiplyMatricesSequentially(const std::vector<std::vector<int>>& A, const std::vector<std::vector<int>>& B, std::vector<std::vector<int>>& C) {
    int r1 = A.size(), c1 = A[0].size(), r2 = B.size(), c2 = B[0].size();
    if (c1 != r2) {
        std::cout << "Matrices cannot be multiplied" << std::endl;
        return;
    }
    for (int i = 0; i < r1; i++) {
        for (int j = 0; j < c2; j++) {
            C[i][j] = 0;
            for (int k = 0; k < c1; k++) {
                C[i][j] += A[i][k] * B[k][j];
            }
        }
    }
}

void multiplyMatricesParallel(const std::vector<std::vector<int>>& A, const std::vector<std::vector<int>>& B, std::vector<std::vector<int>>& C) {
    int r1 = A.size(), c1 = A[0].size(), r2 = B.size(), c2 = B[0].size();
    if (c1 != r2) {
        std::cout << "Matrices cannot be multiplied" << std::endl;
        return;
    }
#pragma omp parallel for collapse(2)
    for (int i = 0; i < r1; i++) {
        for (int j = 0; j < c2; j++) {
            C[i][j] = 0;
            for (int k = 0; k < c1; k++) {
                C[i][j] += A[i][k] * B[k][j];
            }
        }
    }
}

void writeToJSON(const std::chrono::duration<double, std::milli>& elapsed, const std::vector<std::vector<int>>& C, const std::string& method, std::ofstream& outputFile) {
    outputFile << "  \"" << method << "\": {\n";
    outputFile << "    \"time\": \"" << elapsed.count() << " ms\",\n";
    outputFile << "    \"result\": [\n";
    for (size_t i = 0; i < C.size(); ++i) {
        outputFile << "      [";
        for (size_t j = 0; j < C[i].size(); ++j) {
            outputFile << C[i][j];
            if (j < C[i].size() - 1) outputFile << ", ";
        }
        outputFile << "]";
        if (i < C.size() - 1) outputFile << ",";
        outputFile << "\n";
    }
    outputFile << "    ]\n";
    outputFile << "  },\n";
}

int main() {
    omp_set_dynamic(0);             // Zakáže dynamické priradenie vlákien.
    omp_set_num_threads(omp_get_num_procs());// or 2,4,8 // Nastaví počet vlákien na počet procesorov.
    std::srand(static_cast<unsigned>(std::chrono::system_clock::now().time_since_epoch().count()));  // Seed pre náhodné čísla

    // Vytvorte väčšie matice pre zložitejší výpočet.
    size_t n = 1000; // Napríklad 50x50 matice.
    std::vector<std::vector<int>> A(n, std::vector<int>(n));
    std::vector<std::vector<int>> B(n, std::vector<int>(n));
    std::vector<std::vector<int>> C(n, std::vector<int>(n));

    fillMatrixWithRandomNumbers(A, 50); // Naplňte A náhodnými číslami
    fillMatrixWithRandomNumbers(B, 50); // Naplňte B náhodnými číslami

    std::ofstream outputFile("output\\results.json");
    outputFile << "{\n";

    // Sekvenčný výpočet
    auto start = std::chrono::high_resolution_clock::now();
    multiplyMatricesSequentially(A, B, C);
    auto end = std::chrono::high_resolution_clock::now();
    std::chrono::duration<double, std::milli> elapsed = end - start;
    writeToJSON(elapsed, C, "sequential", outputFile);

    outputFile.close(); // Zatvorte súbor pred začiatkom paralelného výpočtu.

    // Reset C pre paralelný výpočet
    C.assign(n, std::vector<int>(n, 0));

    outputFile.open("output\\results.json", std::ios_base::app);

    // Paralelný výpočet
    start = std::chrono::high_resolution_clock::now();
    multiplyMatricesParallel(A, B, C);
    end = std::chrono::high_resolution_clock::now();
    elapsed = end - start;
    writeToJSON(elapsed, C, "parallel", outputFile);

    outputFile << "}\n";
    outputFile.close();

    return 0;
}