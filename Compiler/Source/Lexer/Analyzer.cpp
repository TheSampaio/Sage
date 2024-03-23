#include "Core.h"
#include "Analyzer.h"

Analyzer::Analyzer()
	: m_Reader(nullptr), m_Writer(nullptr)
{
}

Analyzer::~Analyzer()
{
	// Frees allocated memory
	if (m_Reader)
		delete m_Reader;

	if (m_Writer)
		delete m_Writer;
}

bool Analyzer::Read(const std::string& filePath)
{
	// Allocates memory
	fileName = filePath;
	m_Reader = new std::ifstream(fileName);

	// Tries to open the file
	if (!m_Reader->is_open())
		return false;

	// Generates all tokens
	GenerateTokens();

	// Reads the file "line by line"
	while (std::getline(*m_Reader, m_Line))
	{
		// Iterates all keywords inside a line
		for (const auto& token : tokens)
		{
			size_t position = 0;

			// Replaces keywords by tokens if it exists
			while ((position = m_Line.find(token.first, position)) != std::string::npos)
			{
				m_Line.replace(position, token.first.length(), token.second);
				position += token.second.length();
			}
		}

#ifdef _DEBUG
		// Prints the current line (Debug Only)
		if (m_Line.size() > 0)
			std::cout << m_Line << std::endl;
#endif
		if (m_Line.size() > 0)
			m_CommandBuffer.push_back(m_Line);
	}

	// Closes the file
	m_Reader->close();
	return true;
}

bool Analyzer::Write(const std::string& filePath)
{
	// Allocates memory
	m_Writer = new std::ofstream(filePath);

	// Tries to create and write the file
	if (!m_Writer->is_open())
		return false;

	// Writes the file "line by line"
	for (const auto& command : m_CommandBuffer)
		*m_Writer << command << std::endl;

	// Closes the file
	m_Writer->close();
	return true;
}

void Analyzer::GenerateTokens()
{
	// Binds all possible keywords for tokens
	tokens =
	{
		//	<SAGE>	|	<CPP>

		// Symbols
		{	" ",		"@WHITESPACE@"},
		{	"\n",		"@NEW_LINE@"},
		{	"\t",		"@TAB@"},
		{	";",		"@SEMICOLON@"},
		{	"(",		"@PARENTHESIS_BEGIN@"},
		{	")",		"@PARENTHESIS_END@"},
		{	"{",		"@BRACKET_BEGIN@"},
		{	"}",		"@BRACKET_END@"},
		{	"<",		"@OPERATOR_LESS@"},
		{	">",		"@OPERATOR_GREATER@"},
		{	"=",		"@OPERATOR_ASSIGN@"},
		{	"->",		"@OPERATOR_ARROW@"},
		{	"::",		"@OPERATOR_SCOPE@"},
		{	"&&",		"@OPERATOR_AND@"},
		{	"||",		"@OPERATOR_OR@"},
		{	"<<",		"@SHIFT_LEF@"},
		{	">>",		"@SHIFT_RIGHT@"},

		// Math
		{	"+",		"@PLUS@"},
		{	"-",		"@MINUS@"},
		{	"*",		"@MULTIPLY@"},
		{	"/",		"@DIVIDE@"},

		// Keywords
		{	"class",	"@CLASS@"},
		{	"define",	"@DEFINE@"},
		{	"delete",	"@DELETE@"},
		{	"fn",		"@FUNCTION@"},
		{	"Main",		"@ENTRY_POINT@"},
		{	"new",		"@NEW@"},
		{	"return",	"@RETURN@"},
		{	"use",		"@INCLUDE@"},

		// Types
		{	"f32",		"@FLOAT_32@"},
		{	"f64",		"@FLOAT_64@"},

		{	"i8",		"@INTEGER_8@"},
		{	"i16",		"@INTEGER_16@"},
		{	"i32",		"@INTEGER_32@"},
		{	"i64",		"@INTEGER_64@"},

		{	"u8",		"@UNSIGNED_INTEGER_8@"},
		{	"u16",		"@UNSIGNED_INTEGER_16@"},
		{	"u32",		"@UNSIGNED_INTEGER_32@"},
		{	"u64",		"@UNSIGNED_INTEGER_64@"},

		{	"str",		"@STRING@"},
	};
}
