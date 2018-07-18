@IF EXIST "%~dp0\nodejs\node.exe" (
  "%~dp0\nodejs\node.exe" %*
) ELSE (
  @SETLOCAL
  @SET PATHEXT=%PATHEXT:;.JS;=;%
  node %*
)