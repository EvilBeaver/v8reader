@echo off

call tools/setlocals.cmd

%setter% "%VersionFile%"  -f%1 -a%1
