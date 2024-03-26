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
		size_t position = m_Line.find('\t');
		
		// Iterates all keywords inside a line
		for (int i = 0; i < m_Line.size(); i++)
		{
			switch (m_Line.at(i))
			{
			case '\t':
				m_Line.replace(position, sizeof('\t'), 0, '\0');
				break;

			case ' ':
				m_Line.at(i) = '\n';
				break;
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
		{	"\n",		"@NEW_LINE@"},
		{	"\t",		"@TAB@"},
		{	";",		"@SEMICOLON@"},
		{	"(",		"@PARENTHESIS_LEFT@"},
		{	")",		"@PARENTHESIS_RIGHT@"},
		{	"{",		"@BRACKET_LEFT@"},
		{	"}",		"@BRACKET_RIGHT@"},

		{	"<",		"LESS"},
		{	">",		"GREATER"},
		{	"=",		"ASSIGN"},
		{	"->",		"ARROW"},
		{	"::",		"SCOPE"},
		{	"&&",		"AND"},
		{	"||",		"OR"},
		{	"<<",		"SHIFT_LEFT"},
		{	">>",		"SHIFT_RIGHT"},

		// Math
		{	"+",		"PLUS"},
		{	"-",		"MINUS"},
		{	"*",		"MULTIPLY"},
		{	"/",		"DIVIDE"},

		// Keywords
		{	"class",	"CLASS"},
		{	"define",	"DEFINE"},
		{	"delete",	"DELETE"},
		{	"fn",		"FUNCTION"},
		{	"Main",		"ENTRY_POINT"},
		{	"new",		"NEW"},
		{	"return",	"RETURN"},
		{	"use",		"INCLUDE"},

		// Types
		{	"f32",		"FLOAT_32"},
		{	"f64",		"FLOAT_64"},

		{	"i8",		"INT_8"},
		{	"i16",		"INT_16"},
		{	"i32",		"INT_32"},
		{	"i64",		"INT_64"},

		{	"u8",		"UINT_8"},
		{	"u16",		"UINT_16"},
		{	"u32",		"UINT_32"},
		{	"u64",		"UINT_64"},

		{	"str",		"STRING"},
	};
}
