#pragma once

#include "stdafx.h"
#include <vector>
#include <string>

class CLogFile
{
public:
	
	CLogFile();

	void Create(std::wstring name);
	void Write(std::wstring message);
	void Close(bool save = true);
	void Activate(bool isActive = true);

	virtual ~CLogFile(void);

	static const int LF_LOG_ERROR = 300;

	enum errCodes
	{
		LF_NOT_ACTIVE = -1,
		LF_COMMON_FILE_ERROR = -2
	};

private:

	std::vector<std::wstring> m_logContent;
	std::wstring m_FileName;

	bool m_IsActive;

	void RaiseException(std::wstring msg, long code);
	void RaiseException(std::string msg, long code);

};

