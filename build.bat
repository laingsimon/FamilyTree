@echo off

echo Moving to 'deploy' branch
git checkout deploy

echo Merging in latest changes
git merge master

echo Building in Release...
msbuild FamilyTree.sln /verbosity:minimal /p:Configuration=Release /nologo

if %ERRORLEVEL%==0 (
	echo Build succeeded, committing binaries

	git add bin/
	git commit -m "Committed binaries"
	
	echo Pushing deployment...
	git push
	
	echo Moving back to 'master' branch
	git checkout master
	
	echo Rebuilding in Debug...
	msbuild FamilyTree.sln /verbosity:minimal /p:Configuration=Debug /nologo
	
	echo Finished.
) else (
	echo Build failed.
)
