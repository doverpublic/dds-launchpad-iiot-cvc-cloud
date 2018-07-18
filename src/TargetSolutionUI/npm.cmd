:: Edited for use in the Launchpad IoT platform
@ECHO OFF

SETLOCAL

IF EXIST ".\node_modules.tar" (
	IF NOT EXIST ".\node_modules" (
		MD .\node_modules
		.\7z.exe x -ttar -o.\node_modules .\node_modules.tar
	) 
	DEL .\node_modules.tar
)

SET "NODE_EXE=%~dp0\nodejs\node.exe"
IF NOT EXIST "%NODE_EXE%" (
  SET "NODE_EXE=node"
)

SET "NPM_CLI_JS=%~dp0\nodejs\node_modules\npm\bin\npm-cli.js"
IF NOT EXIST "%NPM_CLI_JS%" (
  SET "NPM_CLI_JS=%~dp0\node_modules\npm\bin\npm-cli.js"
)

FOR /F "delims=" %%F IN ('CALL "%NODE_EXE%" "%NPM_CLI_JS%" prefix -g') DO (
  SET "NPM_PREFIX_NPM_CLI_JS=%%F\node_modules\npm\bin\npm-cli.js"
)

IF EXIST "%NPM_PREFIX_NPM_CLI_JS%" (
  SET "NPM_CLI_JS=%NPM_PREFIX_NPM_CLI_JS%"
)

"%NODE_EXE%" "%NPM_CLI_JS%" %*
