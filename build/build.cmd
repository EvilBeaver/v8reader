@echo off
echo Build started

call tools/setlocals.cmd

call "%vsvars%vsvars32.bat"
devenv "%SolutionPath%" /build Release
call getfiles.cmd

%setter% "%VersionFile%"  -f_._.+.0 -a_._.+.0

echo Done