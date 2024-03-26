#ifndef SAGE_COMPILER_PARSER_
#define SAGE_COMPILER_PARSER_

class Parser
{
public:
	Parser();
	~Parser();

	void Convert(std::vector<std::string>& tokens);

private:
	std::unordered_map<std::string, std::string> m_Keywords;
};

#endif // !SAGE_COMPILER_PARSER_
