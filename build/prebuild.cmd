@echo off

set setter=%~dp0\tools\vsetter.exe
echo %setter%

if %2 == Release goto release
if %2 == Debug goto debug
echo no action performed
exit /B 5

:debug
%setter% "%1"  -f_._._.* -a_._._.*

:release
rem ��� �������� ������ ��������� ����� ��������� �� �������
rem %setter% "%1"  -f_._.+._ -a_._.+._