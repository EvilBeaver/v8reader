// unpack.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "V8File.h"
#include "V8FileException.h"

#define EXPORT_C extern "C" __declspec(dllexport) 
typedef LPSTR StringPtr;
typedef std::string StringClass;

std::string WStringToString(StringClass &s)
{
    std::string temp(s.length(), ' ');
    std::copy(s.begin(), s.end(), temp.begin());
    return temp; 
}


EXPORT_C long Unpack(StringPtr name_in, StringPtr name_out_dir)
{
	
	bool LogIsActive = false;//(log_name != NULL);

	CV8File Parser;
	if(LogIsActive)
		Parser.LogFile.Activate();

	//std::wstring in(name_in);
	//std::wstring out(name_out_dir);

	int result = -1;

	try
	{
		result = Parser.Parse(name_in, name_out_dir);

		if(LogIsActive)
		{
			//Parser.LogFile.Create(log_name);
			Parser.LogFile.Close(true);
		}
	}
	catch(CV8FileException e)
	{
		return e.GetCode();
	}

	return result;

}

// для просмотра нам нужны методы V8File::Parse (чтение) и V8File::Build для обратной сборки.