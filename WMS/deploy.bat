@echo on
set target="\\192.168.0.230\wwwroot\app\wms\onhand"
xcopy /y/e/s www %target%\www

pause

copy /y index.html %target%
copy /y update.json %target%
copy /y onhand.apk %target%\onhand.apk
del onhand.apk /f /q

pause 