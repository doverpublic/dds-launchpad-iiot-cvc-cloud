@IF EXIST "%~dp0\nodejs\node.exe" (
  "%~dp0\nodejs\node.exe"  "%~dp0\nodejs\node_modules\@angular\cli\bin\ng" %*
) ELSE (
  @SETLOCAL
  @SET PATHEXT=%PATHEXT:;.JS;=;%
  node  "%~dp0\node_modules\@angular\cli\bin\ng" %*
)