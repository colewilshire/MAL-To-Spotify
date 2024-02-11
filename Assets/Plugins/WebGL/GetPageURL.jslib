mergeInto(LibraryManager.library, {
  GetPageURL: function() {
    var url = window.location.href;
    console.log('Current Page URL:', url);
    var bufferSize = lengthBytesUTF8(url) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(url, buffer, bufferSize);
    return buffer;
  }
});