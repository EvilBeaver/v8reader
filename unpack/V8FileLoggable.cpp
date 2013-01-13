#include "StdAfx.h"
#include "V8FileLoggable.h"


CV8FileLoggable::CV8FileLoggable(void):CV8File()
{
}

CV8FileLoggable::CV8FileLoggable(BYTE *pFileData, bool boolUndeflate):CV8File(pFileData, boolUndeflate)
{
}


CV8FileLoggable::~CV8FileLoggable(void)
{
}
