#include "StdAfx.h"
#include "LogFile.h"
#include "V8FileException.h"
#include <fstream>

CLogFile::CLogFile(void)
{
	m_IsActive = false;
}


CLogFile::~CLogFile(void)
{
	Close();
}

void CLogFile::Create(std::wstring name)
{
	m_FileName = name;
}

void CLogFile::Activate(bool isActive)
{
	m_IsActive = isActive;
}

void CLogFile::Write(std::wstring message)
{
	if(m_IsActive)
		m_logContent.push_back(message);
}

void CLogFile::Close(bool Save)
{

	if(!Save || !m_IsActive)
		return;

	
	std::ofstream writer(m_FileName, std::ios_base::out);
	try
	{
		writer.exceptions ( std::ofstream::failbit | std::ofstream::badbit );

		for (std::vector<std::wstring>::iterator it = m_logContent.begin(); it != m_logContent.end(); ++it)
		{
			std::wstring d = *it;
			writer << d.c_str() << std::endl;
		}
	}
	catch(std::ofstream::failure e)
	{
		RaiseException(std::string(e.what()), LF_COMMON_FILE_ERROR);
	}

	writer.close();
}

void CLogFile::RaiseException(std::wstring msg, long code)
{
	throw CV8FileException(msg, LF_LOG_ERROR + code);
}

void CLogFile::RaiseException(std::string msg, long code)
{
	std::wstring wstr(msg.length(), L' ');
  
     // Copy string to wstring.
     std::copy(msg.begin(), msg.end(), wstr.begin());

	 RaiseException(wstr, code);
}