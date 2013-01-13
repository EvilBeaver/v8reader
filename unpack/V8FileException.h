#pragma once

#include "stdafx.h"
#include <string>

class CV8FileException
{
public:
	CV8FileException(std::wstring message, long err_code):m_message(message),m_errCode(err_code) {}
	~CV8FileException(void){}

	std::wstring& GetMessage() {return m_message;}
	long GetCode() {return m_errCode;}

private:
	
	std::wstring m_message;
	long m_errCode;

};

