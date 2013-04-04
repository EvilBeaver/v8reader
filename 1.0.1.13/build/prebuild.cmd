@echo off

set setter=%~dp0\vsetter.exe

if %2 == Release goto release
if %2 == Debug goto debug
echo no action performed
exit /B 5

:debug
%setter% "%1"  -f_._._.* -a_._._.*

:release
rem для релизной сборки инкремент будем запускать из скрипта
rem %setter% "%1"  -f_._.+._ -a_._.+._