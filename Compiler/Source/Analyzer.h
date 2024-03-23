#ifndef _SAGE_COMPILER_ANALYZER_
#define _SAGE_COMPILER_ANALYZER_

class Analyzer
{
public:
	Analyzer();
	~Analyzer();
	
	// === Attributes ===

	std::string fileName;
	std::unordered_map<std::string, std::string> tokens;

	// === Methods ===

	bool Read(const std::string& filePath);

private:
	// === Attributes ===

	std::ifstream* m_Reader;
	std::string m_Line;

	// === Methods ===

	void GenerateTokens();
};

#endif // !_SAGE_COMPILER_ANALYZER_
