#include <iostream>
#include <fstream>
#include <random>

int main() {
    // Define the size of the matrix
    const int rows = 1000;
    const int cols = 1000;
    std::cout << "kokot1" << std::endl;
    // Open the output file
    std::ofstream outputFile("matrix.txt");

    // Check if the file opened successfully
    if (!outputFile) {
        std::cerr << "Error: Unable to open file." << std::endl;
        return 1;
    }

    // Initialize random number generator
    std::random_device rd;
    std::mt19937 gen(rd());
    std::uniform_int_distribution<int> dis(1, 100); // Change the range if needed

    // Generate and write random numbers to the file
    for (int i = 0; i < rows; ++i) {
        for (int j = 0; j < cols; ++j) {
            int randomNumber = dis(gen);
            outputFile << randomNumber << " ";
        }
        outputFile << "\n";
    }

    // Close the file
    outputFile.close();

    std::cout << "Matrix generated and written to file successfully." << std::endl;

    return 0;
}
