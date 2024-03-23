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

	bool Read(const std::string& file);
	bool Write(const std::string& file);

private:
	// === Attributes ===

	std::ifstream* m_Reader;
	std::ofstream* m_Writer;
	std::string m_Line;
	std::vector<std::string> m_CommandBuffer;

	// === Methods ===

	void GenerateTokens();
};

#endif // !_SAGE_COMPILER_ANALYZER_
