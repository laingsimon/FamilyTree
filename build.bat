@echo off

git checkout deploy
git merge master
msbuild FamilyTree.sln

if %ERRORLEVEL%==0 (
	git add bin/
	git commit -m "Committed binaries"
	
	git push
	git checkout master
)