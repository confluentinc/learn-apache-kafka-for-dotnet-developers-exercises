@echo off
setlocal enabledelayedexpansion

IF "%selfWrapped%"=="" (
  @REM this is necessary so that we can use "exit" to terminate the batch file,
  @REM and all subroutines, but not the original cmd.exe
  SET selfWrapped=true
  %ComSpec% /s /c ""%~0" %*"
  GOTO :EOF
)

SET EXERCISE_DIR=.\
SET SOLUTIONS_DIR=..\solutions
SET STAGING_DIR=..\staging

IF "%1"=="stage" CALL :stage %2
IF "%1"=="solve" CALL :solve %2 %3
IF "%1"=="list" CALL :list
IF "%1"=="help" CALL :help

EXIT 0

@REM This function will display the help text.ß
:help
    CALL :log "Usage:"
    CALL :log  "  exercise.bat ^<Command^>"
    CALL :log "  Commands:"
    CALL :log "    stage ^<Exercise Filter^> - Setup the exercise."
    CALL :log "        ^<Exercise Filter^> - A portion of the exercise name (eg. the exercise number) that will be used to select the exercise."
    CALL :log "    solve ^<Exercise Filter^> ^<File Filter^> - Solve the exercise."
    CALL :log "        ^<Exercise Filter^> - A portion of the exercise name (eg. the exercise number) that will be used to select the exercise."
    CALL :log "        ^<File Filter^> - (Optional) A portion of a file name that will be used to select while file to copy from the solution."
    CALL :log "    list - List all exercises."
    CALL :log "  Exercise Filter: A portion of the name of the exercise. Eg. The Exercise Number. If multiple matches are found, the last one will be chosen."

    GOTO :eof

@REM This is a basic string replacement function.
@REM %1: The string to search for the replacement
@REM %2: What to replace in the original string
@REM %3: What to replace it with in the original string
@REM %4: The name of the variable that will be populated with the result.
:replace
    set ORIGINAL=%~1
    set REPLACE=%~2
    set WITH=%~3
    CALL SET "RETURNVAL=%%ORIGINAL:%REPLACE%=%WITH%%%"
    SET %~4=%RETURNVAL%

    GOTO :eof

@REM This will extract the directory path from a filename
@REM %1: The filename
@REM %2: The name of the variable that will be populated with the result
:getpath
    set %~2=%~dp1
    GOTO :eof

@REM Log a message. Helps eliminate double quoting issues.
:log
    echo %~1
    GOTO :eof

@REM Log an error message, print the help, then exit.
:error
    CALL :log %1
    CALL :help
    EXIT 1

@REM This function will stage an exercise
@REM %1: A portion of the exercise name (eg. the exercise number) that will be used to select the exercise.
:stage
    if "%1" == "" CALL :error "MISSING EXERCISE FILTER"

    SET EXERCISE_FILTER=%1

    @REM Locate an exercise that matches the filter.
    for /d %%D in (%STAGING_DIR%\*%EXERCISE_FILTER%*) do (
        SET EXERCISE=%%~nxD
    )

    if "%EXERCISE%" == "" CALL :error "UNABLE TO FIND EXERCISE"

    CALL :log "STAGING %EXERCISE%"

    xcopy /y /s /e /q %STAGING_DIR%\%EXERCISE%\* .\

    GOTO :eof
}

@REM This function will solve an exercise
@REM %1: A portion of the exercise name (eg. the exercise number) that will be used to select the exercise.
@REM %2: (Optional) A portion of a file name that will be used to select while file to copy from the solution.
:solve
    if "%2"=="" (CALL :solve_exercise %1) ELSE (CALL :solve_file %1 %2)

    GOTO :eof

@REM This function will solve an exercise
@REM %1: A portion of the exercise name (eg. the exercise number) that will be used to select the exercise.
:solve_exercise
    if "%1" == "" CALL :error "MISSING EXERCISE FILTER"

    SET EXERCISE_FILTER=%1

    @REM Locate an exercise that matches the filter.
    for /d %%D in (%SOLUTIONS_DIR%\*%EXERCISE_FILTER%*) do (
        SET EXERCISE=%%~nxD
    )

    if "%EXERCISE%" == "" CALL :error "UNABLE TO FIND EXERCISE"

    CALL :log "SOLVING EXERCISE %EXERCISE%"

    xcopy /y /s /e /q %SOLUTIONS_DIR%\%EXERCISE%\* .\

    GOTO :eof

@REM This function will solve a single file in an exercise
@REM %1: A portion of the exercise name (eg. the exercise number) that will be used to select the exercise.
@REM %2: (Optional) A portion of a file name that will be used to select while file to copy from the solution.
:solve_file
    if "%1" == "" CALL :error "MISSING EXERCISE FILTER"
    if "%2" == "" CALL :error "MISSING FILE FILTER"

    SET EXERCISE_FILTER=%1
    SET FILE_FILTER=%2

    @REM Locate an exercise that matches the filter.
    for /d %%D in (%SOLUTIONS_DIR%\*%EXERCISE_FILTER%*) do (
        SET EXERCISE=%%~nxD
    )

    if "%EXERCISE%" == "" CALL :error "UNABLE TO FIND EXERCISE"

    @REM Locate a file that matches the filter, and obtain the relative path.
    for /R  "%SOLUTIONS_DIR%\%EXERCISE%" %%F in (*%FILE_FILTER%*) do (
        call :replace %%F %SOLUTIONS_DIR%\%EXERCISE%\ "" RETURN
        SET FILE=!RETURN!
    )

    if "%FILE%" == "" CALL :error "UNABLE TO FIND FILE"

    CALL :log "COPYING FILE %SOLUTIONS_DIR%\%EXERCISE%\%FILE% TO %FILE%"

    @REM Ensure the file/directory exist to avoid errors/prompts during the copy.
    call :getpath %FILE% DIRECTORY
    if not exist "%DIRECTORY%" mkdir "%DIRECTORY%"
    echo "" > %FILE%

    xcopy /y /s /e /q %SOLUTIONS_DIR%\%EXERCISE%\%FILE% %FILE%

    GOTO :eof

@REM This function will list all exercises.
:list
    for /D %%F in (%SOLUTIONS_DIR%\*) do (
        CALL :log %%~nF
    )
    GOTO :eof