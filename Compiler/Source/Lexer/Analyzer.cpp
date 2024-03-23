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
			m_CommandBufer.push_back(m_Line);
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
	for (const auto& command : m_CommandBufer)
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
		{	" ",		"TT_WHITESPACE"},
		{	"\n",		"TT_NEW_LINE"},
		{	"\t",		"TT_TAB"},
		{	";",		"TT_SEMICOLON"},
		{	"(",		"TT_PARENTHESIS_BEGIN"},
		{	")",		"TT_PARENTHESIS_END"},
		{	"{",		"TT_BRACKET_BEGIN"},
		{	"}",		"TT_BRACKET_END"},
		{	"<",		"TT_OPERATOR_LESS"},
		{	">",		"TT_OPERATOR_GREATER"},
		{	"=",		"TT_OPERATOR_ASSIGN"},
		{	"->",		"TT_OPERATOR_ARROW"},
		{	"::",		"TT_OPERATOR_SCOPE"},
		{	"&&",		"TT_OPERATOR_AND"},
		{	"||",		"TT_OPERATOR_OR"},
		{	"<<",		"TT_SHIFT_LEF"},
		{	">>",		"TT_SHIFT_RIGHT"},

		// Math
		{	"+",		"TT_PLUS"},
		{	"-",		"TT_MINUS"},
		{	"*",		"TT_MULTIPLY"},
		{	"/",		"TT_DIVIDE"},

		// Keywords
		{	"class",	"TT_CLASS"},
		{	"define",	"TT_DEFINE"},
		{	"delete",	"TT_DELETE"},
		{	"fn",		"TT_FUNCTION"},
		{	"Main",		"TT_ENTRY_POINT"},
		{	"new",		"TT_NEW"},
		{	"return",	"TT_RETURN"},
		{	"use",		"TT_INCLUDE"},

		// Types
		{	"f32",		"TT_FLOAT_32"},
		{	"f64",		"TT_FLOAT_64"},

		{	"i8",		"TT_INTEGER_8"},
		{	"i16",		"TT_INTEGER_16"},
		{	"i32",		"TT_INTEGER_32"},
		{	"i64",		"TT_INTEGER_64"},

		{	"u8",		"TT_UNSIGNED_INTEGER_8"},
		{	"u16",		"TT_UNSIGNED_INTEGER_16"},
		{	"u32",		"TT_UNSIGNED_INTEGER_32"},
		{	"u64",		"TT_UNSIGNED_INTEGER_64"},

		{	"str",		"TT_STRING"},
	};
}
