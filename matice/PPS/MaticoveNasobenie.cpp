#include <iostream>
#include <vector>
#include <chrono>
#include <string>
#include <fstream>
#include <random>

void fillMatrixWithRandomIntegers(std::vector<std::vector<int>>& matrix, int max_value) {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<int> dis(0, max_value);

    for (auto& row : matrix) {
        for (auto& elem : row) {
            elem = dis(gen);
        }
    }
}

void fillMatrixWithRandomFloats(std::vector<std::vector<float>>& matrix, float max_value) {
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_real_distribution<float> dis(0.0f, max_value);

    for (auto& row : matrix) {
        for (auto& elem : row) {
            elem = dis(gen);
        }
    }
}

void multiplyMatricesIntegers(const std::vector<std::vector<int>>& A, const std::vector<std::vector<int>>& B, std::vector<std::vector<int>>& C) {
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

void multiplyMatricesFloats(const std::vector<std::vector<float>>& A, const std::vector<std::vector<float>>& B, std::vector<std::vector<float>>& C) {
    int r1 = A.size(), c1 = A[0].size(), r2 = B.size(), c2 = B[0].size();
    if (c1 != r2) {
        std::cout << "Matrices cannot be multiplied" << std::endl;
        return;
    }
    for (int i = 0; i < r1; i++) {
        for (int j = 0; j < c2; j++) {
            C[i][j] = 0.0f;
            for (int k = 0; k < c1; k++) {
                C[i][j] += A[i][k] * B[k][j];
            }
        }
    }
}

void writeToJSON(const std::chrono::duration<double, std::milli>& elapsed, const std::vector<std::vector<int>>& C_int, const std::vector<std::vector<float>>& C_float, const std::string& method, const std::string& type, std::ofstream& outputFile) {
    outputFile << "  \"" << method << "_" << type << "\": {\n";
    outputFile << "    \"time\": \"" << elapsed.count() << " ms\"\n";
    outputFile << "  },\n";
}

int main() {
    std::srand(static_cast<unsigned>(std::chrono::system_clock::now().time_since_epoch().count()));

    std::vector<int> matrix_sizes = { 500, 1000, 2000 };
    std::ofstream outputFile("output/results.json");
    outputFile << "{\n";

    for (size_t size : matrix_sizes) {
        std::cout << "matrix size: " << size << "\n";
        std::vector<std::vector<int>> A_int(size, std::vector<int>(size));
        std::vector<std::vector<int>> B_int(size, std::vector<int>(size));
        std::vector<std::vector<int>> C_int(size, std::vector<int>(size));

        std::vector<std::vector<float>> A_float(size, std::vector<float>(size));
        std::vector<std::vector<float>> B_float(size, std::vector<float>(size));
        std::vector<std::vector<float>> C_float(size, std::vector<float>(size));

        fillMatrixWithRandomIntegers(A_int, 50);
        fillMatrixWithRandomIntegers(B_int, 50);

        fillMatrixWithRandomFloats(A_float, 50.0f);
        fillMatrixWithRandomFloats(B_float, 50.0f);

        std::cout << "multiplying int of size: " << size << "\n";
        auto start_int = std::chrono::high_resolution_clock::now();
        multiplyMatricesIntegers(A_int, B_int, C_int);
        auto end_int = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double, std::milli> elapsed_int = end_int - start_int;

        std::cout << "multiplying float of size: " << size << "\n";
        auto start_float = std::chrono::high_resolution_clock::now();
        multiplyMatricesFloats(A_float, B_float, C_float);
        auto end_float = std::chrono::high_resolution_clock::now();
        std::chrono::duration<double, std::milli> elapsed_float = end_float - start_float;

        writeToJSON(elapsed_int, C_int, C_float, "size_" + std::to_string(size), "int", outputFile);
        writeToJSON(elapsed_float, C_int, C_float, "size_" + std::to_string(size), "float", outputFile);
    }

    outputFile << "}\n";
    outputFile.close();

    return 0;
}
