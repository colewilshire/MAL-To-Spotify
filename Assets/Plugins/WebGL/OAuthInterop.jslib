mergeInto(LibraryManager.library, {
  StartOAuthFlow: function(url, redirectUri) {
    url = UTF8ToString(url);
    redirectUri = UTF8ToString(redirectUri);

    // JavaScript to open OAuth window and monitor for the redirect
    var oauthWindow = window.open(url, 'oauthWindow', 'width=600,height=700');
    var interval = setInterval(function() {
      try {
        console.log("Test 1")
        if (oauthWindow.location.href.indexOf(redirectUri) !== -1) {
          console.log("Test 2")
          clearInterval(interval);
          oauthWindow.close();
          // Extract authorization code from the URL
          var authCode = new URL(oauthWindow.location.href).searchParams.get('code');
          // Call back into Unity with the auth code
          var gameObjectName = 'MAL Controller'; // Make sure this matches your GameObject's name in Unity
          var methodName = 'ReceiveAuthCode';
          var unityInstance = unityInstance || {};
          if (unityInstance.SendMessage) {
            console.log("Test 3")
            unityInstance.SendMessage(gameObjectName, methodName, authCode);
          }
        }
      } catch (e) {
        console.log("Test 0")
      }
    }, 1000);
  }
});