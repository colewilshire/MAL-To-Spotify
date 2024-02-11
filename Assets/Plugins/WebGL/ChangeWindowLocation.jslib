// Assets/Plugins/WebGL/MyJavaScriptLib.jslib
mergeInto(LibraryManager.library, {
  ChangeWindowLocation: function(url) {
    url = UTF8ToString(url); // Convert integer pointer to string
    window.location.href = url;
  }
});