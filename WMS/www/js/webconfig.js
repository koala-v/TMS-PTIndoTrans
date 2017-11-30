'use strict';
var appConfig = angular.module('EventMob.config', []);
appConfig.value('ENV', {
  'website': 'http://www.sysfreight.net:8081/app/tms/PTIndoTrans',
   // 'api': 'http://www.sysfreight.net:8081/apis/tms/PTIndoTrans',
  // 'website': 'http://www.sysfreight.net:8081/tmsapp',
 'api': 'http://localhost:18104',
  // 'api': 'http://192.168.0.229/Webapi',
// <<<<<<< HEAD

  'debug': true,
  'mock': false,
  'fromWeb': true,
  'appId': '9CBA0A78-7D1D-49D3-BA71-C72E93F9E48F',
  'rootPath': 'TmsApp',
  'configFile': 'config.txt',
  'mapProvider': 'baidu',
  'version': '1.0.26',


// =======
  // 'debug': true,
  // 'mock': false,
  // 'fromWeb': true,
  // 'appId': '9CBA0A78-7D1D-49D3-BA71-C72E93F9E48F',
  // 'rootPath': 'TmsApp',
  // 'configFile': 'config.txt',
  // 'mapProvider': 'baidu',
  // 'version': '1.0.24',


// >>>>>>> 5876cd204c8af32754e9c6023dc39c173f4500f7
});

var onGetRegistradionID = function(data) {
  try {
    log4web.log("JPushPlugin:registrationID is " + data)
  } catch (exception) {
    log4web.log(exception);
  }
};

var onStrToURL = function(strURL) {
  if (strURL.length > 0 && strURL.indexOf('http://') < 0 && strURL.indexOf('HTTP://') < 0) {
    strURL = "http://" + strURL;
  }
  return strURL;
};
