#pragma once
#include "v8file.h"
#include "LogFile.h"

class CV8FileLoggable :
	public CV8File
{
public:
	CV8FileLoggable(void);
	CV8FileLoggable(BYTE *pFileData, bool boolUndeflate = true);
	virtual ~CV8FileLoggable(void);

	CLogFile m_Log;
};

