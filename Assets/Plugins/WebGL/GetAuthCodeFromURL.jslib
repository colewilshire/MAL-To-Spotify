mergeInto(LibraryManager.library, {
  GetAuthCodeFromURL: function() {
    var urlParams = new URLSearchParams(window.location.search);
    var code = urlParams.get('code'); // Extract the 'code' parameter

    // Log the code for debugging
    console.log('Auth Code:', code);

    if (code !== null) {
        var bufferSize = lengthBytesUTF8(code) + 1;
        var buffer = _malloc(bufferSize);
        stringToUTF8(code, buffer, bufferSize);
        return buffer;
    } else {
        return 0; // Return 0 or null pointer if 'code' parameter doesn't exist
    }
  }
});
