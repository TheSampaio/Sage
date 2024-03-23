#include "Core.h"
#include "Analyzer.h"

// Pauses and returns an error
#define RETURN_FAILURE return (std::getchar() != '\0') ? EXIT_FAILURE : EXIT_FAILURE

int main()
{
    // Allocates memory
    Analyzer* analyzer = new Analyzer();

    // Reads a ".sg" file
    if (!analyzer->Read("Test/Main.sg"))
    {
        // Logs if it failed
        std::cout << "[ERROR] Failed to read the file \'" << analyzer->fileName.c_str() << "\'.";
        RETURN_FAILURE;
    }

    // Frees allocated memory
    delete analyzer;

    // Ends the program with success
    return EXIT_SUCCESS;
}