#include "Core.h"
#include "Lexer/Analyzer.h"

// Pauses and returns an error
#define RETURN_FAILURE return (std::getchar() != '\0') ? EXIT_FAILURE : EXIT_FAILURE

int main(int argc, char* argv[])
{
    // Allocates memory
    Analyzer* analyzer = new Analyzer();

#ifdef _DEBUG
    const std::string path = "../Application/Source/";
    const std::string file = "Main.sg";

    // Reads a ".sg" file
    if (!analyzer->Read(path + file))
    {
        // Logs if it failed
        std::cout << "[ERROR] Failed to read the file \"" << analyzer->fileName.c_str() << "\".";
        RETURN_FAILURE;
    }

    // Writes a ".sgc" file
    if (!analyzer->Write(path + file + "c"))
    {
        // Logs if it failed
        std::cout << "[ERROR] Failed to write the file \"" << analyzer->fileName.c_str() << "\".";
        RETURN_FAILURE;
    }
#else
    if (argc > 0)
    {
        const std::string file = argv[1];

        // Reads a ".sg" file
        if (!analyzer->Read(file))
        {
            // Logs if it failed
            std::cout << "[ERROR] Failed to read the file \'" << analyzer->fileName.c_str() << "\'.";
            RETURN_FAILURE;
        }

        // Writes a ".sgc" file
        if (!analyzer->Write(file + "c"))
        {
            // Logs if it failed
            std::cout << "[ERROR] Failed to write the file \'" << analyzer->fileName.c_str() << "\'.";
            RETURN_FAILURE;
        }
    }
#endif

    // Frees allocated memory
    delete analyzer;

    // Ends the program with success
    return EXIT_SUCCESS;
}