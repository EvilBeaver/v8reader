@echo off
echo Build started

call tools/setlocals.cmd

%setter% "%VersionFile%"  -f_._.+._ -a_._.+._

call "%vsvars%vsvars32.bat"
devenv "%SolutionPath%" /build Release
call getfiles.cmd
echo Done