@echo on

call cordova build --release android

pause

cd platforms\android\build\outputs\apk

jarsigner -verbose -sigalg SHA1withRSA -digestalg SHA1 -keystore ../../../../../my-release-key.keystore android-release-unsigned.apk alias_name

<<<<<<< HEAD
F:\Android\sdk\android-sdk\build-tools\22.0.1\zipalign.exe -v 4 android-release-unsigned.apk TMS.apk
=======
C:\Android\sdk\android-sdk\build-tools\22.0.1\zipalign.exe -v 4 android-release-unsigned.apk TMS.apk
>>>>>>> 5876cd204c8af32754e9c6023dc39c173f4500f7

xcopy /y "%~dp0platforms\android\build\outputs\apk\TMS.apk" "%~dp0"

del "%~dp0platforms\android\build\outputs\apk\TMS.apk" /f /q

pause